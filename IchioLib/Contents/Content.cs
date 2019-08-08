using System.Collections;
using System.Collections.Generic;

namespace ILib.Contents
{
	using Caller;
	using System;
	using Routines;

	public abstract class Content<T> : Content where T : IContentParam
	{
		protected new T Param => (T)base.Param;
	}

	public abstract partial class Content : IHasDispatcher, IHasRoutineOwner
	{
		Content m_Parent;

		/// <summary>
		/// 親のコンテンツです。
		/// </summary>
		protected IContentRef Parent { get; private set; }

		/// <summary>
		/// 所属するコントローラーです。
		/// </summary>
		protected ContentsController Controller { get; private set; }
		LockCollection<Content> m_Children = new LockCollection<Content>();

		public bool HasChildren => m_Children.Count > 0;

		bool m_Shutdown;
		TransLock m_TransLock = new TransLock();

		protected object Param { get; private set; }

		/// <summary>
		/// 実行中か？
		/// </summary>
		protected bool Running { get; private set; }

		/// <summary>
		/// コンテンツのイベントに関してエラー時に自身のハンドルに例外をスローします
		/// </summary>
		protected bool IsSelfThrowErrorIfNeeded { get; set; } = true;

		/// <summary>
		/// イベントの発火装置です。
		/// </summary>
		protected Call Call { get; private set; }

		/// <summary>
		/// イベントの発火装置です。
		/// </summary>
		public IDispatcher Dispatcher { get; private set; }

		/// <summary>
		/// 自身と子を対象とするモジュールです。
		/// </summary>
		public ModuleCollection Modules { get; private set; }

		UnityEngine.MonoBehaviour IHasRoutineOwner.RoutineOwner => Controller;

		/// <summary>
		/// 起動処理です。
		/// </summary>
		protected virtual ITriggerAction OnBoot() => Trigger.Successed;
		/// <summary>
		/// 有効時の処理です。
		/// </summary>
		protected virtual ITriggerAction OnEnable() => Trigger.Successed;
		/// <summary>
		/// 実行時の処理です。
		/// </summary>
		protected virtual ITriggerAction OnRun() => Trigger.Successed;
		/// <summary>
		/// 実行処理が完了した際の処理です。
		/// </summary>
		protected virtual void OnCompleteRun() { }
		/// <summary>
		/// 停止時の処理です。
		/// </summary>
		protected virtual ITriggerAction OnSuspend() => Trigger.Successed;
		/// <summary>
		/// 無効時の処理です。
		/// </summary>
		protected virtual ITriggerAction OnDisable() => Trigger.Successed;
		/// <summary>
		/// 終了直前の処理です。
		/// </summary>
		protected virtual void OnPreShutdown() { }
		/// <summary>
		/// 終了時の処理です。
		/// </summary>
		protected virtual ITriggerAction OnShutdown() => Trigger.Successed;

		ITriggerAction<bool> _Routine(IEnumerator enumerator)
		{
			var routine = Controller.Routine(enumerator);
			if (IsSelfThrowErrorIfNeeded) routine.AddFail(ThrowException);
			return routine.Action;
		}

		ITriggerAction<T> _Routine<T>(IEnumerator enumerator)
		{
			var routine = Controller.TaskRoutine<T>(enumerator);
			if (IsSelfThrowErrorIfNeeded) routine.AddFail(ThrowException);
			return routine.Action;
		}

		/// <summary>
		/// 自身の子にコンテンツを追加します。
		/// </summary>
		protected ITriggerAction<IContentRef> Append(IContentParam prm)
		{
			return Append(prm.GetContentType(), prm);
		}

		/// <summary>
		/// 自身の子にコンテンツを追加します。
		/// </summary>
		protected ITriggerAction<IContentRef> Append<T>(object prm)
		{
			return Append(typeof(T), prm);
		}

		/// <summary>
		/// 自身の子にコンテンツを追加します。
		/// </summary>
		protected ITriggerAction<IContentRef> Append(Type type, object prm)
		{
			var content = (Content)Activator.CreateInstance(type);
			m_Children.Add(content);
			return _Routine<IContentRef>(content.Boot(Controller, this, prm));
		}

		/// <summary>
		/// 停止後の復帰処理を行います。
		/// </summary>
		protected ITriggerAction<bool> Resume() => _Routine(DoRun());

		/// <summary>
		/// 停止処理を開始します。
		/// </summary>
		protected ITriggerAction<bool> Suspend() => _Routine(DoSuspend());

		/// <summary>
		/// 終了処理を開始します。
		/// </summary>
		protected ITriggerAction<bool> Shutdown() => _Routine(DoShutdown());

		/// <summary>
		/// 終了処理と指定コンテンツへの遷移を開始します。
		/// </summary>
		protected ITriggerAction<IContentRef> Switch(IContentParam prm) => _Routine<IContentRef>(DoSwitch(prm.GetContentType(), prm));

		/// <summary>
		/// 終了処理と指定コンテンツへの遷移を開始します。
		/// </summary>
		protected ITriggerAction<IContentRef> Switch<T>(object prm = null) => _Routine<IContentRef>(DoSwitch(typeof(T), prm));

		/// <summary>
		/// 終了処理と指定コンテンツへの遷移を開始します。
		/// </summary>
		protected ITriggerAction<IContentRef> Switch(Type type, object prm = null) => _Routine<IContentRef>(DoSwitch(type, prm));

		bool HasModule(ModuleType type) => (type & Modules.Type) == type;

		void PreBoot(ContentsController controller, Content parent, object prm)
		{
			Param = prm;
			Controller = controller;
			m_Parent = parent;
			if (m_Parent != null)
			{
				Parent = new Ref(m_Parent);
			}
			Call = m_Parent != null ? m_Parent.Call.SubCall() : Controller.SubCall();
			Call.Bind(this);
			Dispatcher = new Dispatcher(Call);
			Modules = m_Parent != null ? new ModuleCollection(m_Parent.Modules) : new ModuleCollection(Controller.Modules);
		}

		internal IEnumerator Boot(ContentsController controller, Content parent, object prm)
		{
			PreBoot(controller, parent, prm);
			using (m_TransLock.Lock(TransLockFlag.Boot))
			{
				if (HasModule(ModuleType.PreBoot)) yield return Modules.OnPreBoot(this) ?? Trigger.Successed;
				yield return OnBoot();
				if (HasModule(ModuleType.Boot)) yield return Modules.OnBoot(this) ?? Trigger.Successed;
				yield return DoRun();
			}
			yield return Result<IContentRef>.Create(new Ref(this));
		}

		IEnumerator DoEnable()
		{
			using (m_TransLock.Lock(TransLockFlag.EnableOrDisable))
			{
				if (!Running) yield break;
				Call.Enabled = true;
				if (HasModule(ModuleType.PreEnable)) yield return Modules.OnPreEnable(this) ?? Trigger.Successed;
				yield return OnEnable() ?? Trigger.Successed;
				foreach (var child in m_Children)
				{
					yield return child.DoEnable() ?? Trigger.Successed;
				}
				if (HasModule(ModuleType.Enable)) yield return Modules.OnEnable(this) ?? Trigger.Successed;
			}
		}

		IEnumerator DoRun()
		{
			using (m_TransLock.Lock(TransLockFlag.RunOrSuspend))
			{
				if (m_Shutdown || Running) yield break;
				Running = true;
				if (HasModule(ModuleType.PreRun)) yield return Modules.OnPreRun(this) ?? Trigger.Successed;
				yield return DoEnable() ?? Trigger.Successed;
				yield return OnRun() ?? Trigger.Successed;
				if (HasModule(ModuleType.Run)) yield return Modules.OnRun(this) ?? Trigger.Successed;
				OnCompleteRun();
			}
		}

		IEnumerator DoDisable()
		{
			using (m_TransLock.Lock(TransLockFlag.EnableOrDisable))
			{
				if (Running) yield break;
				Call.Enabled = false;
				if (HasModule(ModuleType.PreDisable)) yield return Modules.OnPreDisable(this) ?? Trigger.Successed;
				foreach (var child in m_Children)
				{
					yield return child.DoDisable() ?? Trigger.Successed;
				}
				yield return OnDisable() ?? Trigger.Successed;
				if (HasModule(ModuleType.Disable)) yield return Modules.OnDisable(this) ?? Trigger.Successed;
			}
		}

		IEnumerator DoSuspend()
		{
			using (m_TransLock.Lock(TransLockFlag.RunOrSuspend))
			{
				if (m_Shutdown || !Running) yield break;
				Running = false;
				if (HasModule(ModuleType.PreSuspend)) yield return Modules.OnPreSuspend(this) ?? Trigger.Successed;
				yield return DoDisable() ?? Trigger.Successed;
				yield return OnSuspend() ?? Trigger.Successed;
				if (HasModule(ModuleType.Suspend)) yield return Modules.OnSuspend(this) ?? Trigger.Successed;
			}
		}

		internal IEnumerator DoShutdown()
		{
			m_Shutdown = true;
			Call.Dispose();
			m_Parent?.m_Children.Remove(this);
			//ブートシーケンスだけは待つ
			while (m_TransLock.IsLock(TransLockFlag.Boot))
			{
				yield return null;
			}
			using (m_TransLock.Lock(TransLockFlag.Shutdown))
			{
				OnPreShutdown();
				if (HasModule(ModuleType.PreShutdown)) yield return Modules.OnPreShutdown(this) ?? Trigger.Successed;
				foreach (var child in m_Children)
				{
					yield return child.DoShutdown() ?? Trigger.Successed;
				}
				yield return OnShutdown() ?? Trigger.Successed;
				if (HasModule(ModuleType.Shutdown)) yield return Modules.OnShutdown(this) ?? Trigger.Successed;
			}
		}

		IEnumerator DoSwitch(Type type, object prm)
		{
			var next = (Content)Activator.CreateInstance(type);
			next.PreBoot(Controller, m_Parent, prm);

			if (HasModule(ModuleType.PreSwitch)) yield return Modules.OnPreSwitch(this, next) ?? Trigger.Successed;

			yield return DoShutdown() ?? Trigger.Successed;

			if (next.HasModule(ModuleType.Switch)) yield return next.Modules.OnSwitch(this, next) ?? Trigger.Successed;

			using (next.m_TransLock.Lock(TransLockFlag.Boot))
			{
				if (next.HasModule(ModuleType.PreBoot)) yield return next.Modules.OnPreBoot(next) ?? Trigger.Successed;

				yield return next.OnBoot() ?? Trigger.Successed;

				if (next.HasModule(ModuleType.Boot)) yield return next.Modules.OnBoot(next) ?? Trigger.Successed;

				if (next.HasModule(ModuleType.EndSwitch)) yield return next.Modules.OnEndSwitch(this, next) ?? Trigger.Successed;

				yield return next.DoRun() ?? Trigger.Successed;
			}
			//遷移先を送信
			yield return Result<IContentRef>.Create(new Ref(next));
		}


		/// <summary>
		/// 子に登録されている指定のタイプのコンテンツを取得します。
		/// </summary>
		public IContentRef Get<T>(bool recursive = true)
		{
			foreach (var child in m_Children)
			{
				if (child is T) return new Ref(child);
			}
			if (recursive)
			{
				foreach (var child in m_Children)
				{
					var ret = child.Get<T>(recursive);
					if (ret != null)
					{
						return ret;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// 子に登録されている指定のタイプのコンテンツを取得します。
		/// </summary>
		public IContentRef Get(Type type, bool recursive = true)
		{
			foreach (var child in m_Children)
			{
				if (type.IsAssignableFrom(child.GetType())) return new Ref(child);
			}
			if (recursive)
			{
				foreach (var child in m_Children)
				{
					var ret = child.Get(type, recursive);
					if (ret != null)
					{
						return ret;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// 子に登録されている指定のタイプのコンテンツをすべて取得します。
		/// </summary>
		public IEnumerable<IContentRef> GetAll<T>(bool recursive = true)
		{
			foreach (var child in m_Children)
			{
				if (child is T) yield return new Ref(child);
				if (recursive)
				{
					foreach (var c in child.GetAll<T>())
					{
						yield return c;
					}
				}
			}
		}
		

		/// <summary>
		/// 例外をスローします。
		/// ハンドリングされない場合、親へと投げられます。
		/// </summary>
		/// <param name="exception"></param>
		protected void ThrowException(Exception exception)
		{
			bool ret = false;
			try
			{
				ret = HandleException(exception);
			}
			catch (Exception e)
			{
				if (m_Parent != null)
				{
					m_Parent.ThrowException(exception);
				}
				else
				{
					Controller.ThrowException(exception);
				}
				throw e;
			}
			if (!ret)
			{
				if (m_Parent != null)
				{
					m_Parent.ThrowException(exception);
				}
				else
				{
					Controller.ThrowException(exception);
				}
			}
		}

		protected virtual bool HandleException(Exception ex)
		{
			return false;
		}

	}
}
