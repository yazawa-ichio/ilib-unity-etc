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

	public abstract class Content : IHasDispatcher, IHasRoutineOwner
	{
		[Flags]
		internal enum TransLockFlag
		{
			Boot = 1,
			RunOrSuspend = 2,
			EnableOrDisable = 3,
			Shutdown = 4,
		}

		internal class Ref : IContentRef
		{
			Content m_content;
			public Ref(Content content) => m_content = content;
			public IRoutine<bool> Resume() => m_content.Resume();
			public IRoutine<bool> Suspend() => m_content.Suspend();
			public IRoutine<bool> Shutdown() => m_content.Shutdown();
			public IRoutine<IContentRef> Switch(IContentParam prm) => m_content.Switch(prm);
			public IRoutine<IContentRef> Switch<T>() => m_content.Switch<T>();
			public IRoutine<IContentRef> Switch(Type type, object prm = null) => m_content.Switch(type, prm);
			IDispatcher IHasDispatcher.Dispatcher => m_content.Dispatcher;
		}

		protected Content Parent { get; private set; }
		protected ContentsController Controller { get; private set; }
		LockCollection<Content> m_Children = new LockCollection<Content>();

		public bool HasChildren => m_Children.Count > 0;

		bool m_Shutdown;
		TransLock m_TransLock = new TransLock();
		protected object Param { get; private set; }
		protected bool Running { get; private set; }
		protected bool IsSelfThrowErrorIfNeeded { get; set; } = true;

		protected Call Call { get; private set; }
		public IDispatcher Dispatcher { get; private set; }
		public ModuleCollection Modules { get; private set; }

		UnityEngine.MonoBehaviour IHasRoutineOwner.RoutineOwner => Controller;

		protected virtual IEnumerator OnBoot() { yield break; }
		protected virtual IEnumerator OnEnable() { yield break; }
		protected virtual IEnumerator OnRun() { yield break; }
		protected virtual void OnCompleteRun() { }
		protected virtual IEnumerator OnSuspend() { yield break; }
		protected virtual IEnumerator OnDisable() { yield break; }
		protected virtual void OnPreShutdown() { }
		protected virtual IEnumerator OnShutdown() { yield break; }

		IRoutine<bool> _Routine(IEnumerator enumerator)
		{
			var routine = Controller.Routine(enumerator);
			if (IsSelfThrowErrorIfNeeded) routine.AddFail(ThrowException);
			return routine;
		}

		IRoutine<T> _Routine<T>(IEnumerator enumerator)
		{
			var routine = Controller.TaskRoutine<T>(enumerator);
			if (IsSelfThrowErrorIfNeeded) routine.AddFail(ThrowException);
			return routine;
		}

		protected IRoutine<IContentRef> Append(IContentParam prm)
		{
			return Append(prm.GetContentType(), prm);
		}

		protected IRoutine<IContentRef> Append<T>(object prm)
		{
			return Append(typeof(T), prm);
		}

		protected IRoutine<IContentRef> Append(Type type, object prm)
		{
			var content = (Content)Activator.CreateInstance(type);
			m_Children.Add(content);
			return _Routine<IContentRef>(content.Boot(Controller, this, prm));
		}

		protected IRoutine<bool> Resume() => _Routine(DoRun());

		protected IRoutine<bool> Suspend() => _Routine(DoSuspend());

		protected IRoutine<bool> Shutdown() => _Routine(DoShutdown());

		protected IRoutine<IContentRef> Switch(IContentParam prm) => _Routine<IContentRef>(DoSwitch(prm.GetContentType(), prm));

		protected IRoutine<IContentRef> Switch<T>(object prm = null) => _Routine<IContentRef>(DoSwitch(typeof(T), prm));

		protected IRoutine<IContentRef> Switch(Type type, object prm = null) => _Routine<IContentRef>(DoSwitch(type, prm));

		bool HasModule(ModuleType type) => (type & Modules.Type) == type;

		void PreBoot(ContentsController controller, Content parent, object prm)
		{
			Param = prm;
			Controller = controller;
			Parent = parent;
			Call = Parent != null ? Parent.Call.SubCall() : Controller.SubCall();
			Call.Bind(this);
			Dispatcher = new Dispatcher(Call);
			Modules = Parent != null ? new ModuleCollection(Parent.Modules) : new ModuleCollection(Controller.Modules);
		}

		internal IEnumerator Boot(ContentsController controller, Content parent, object prm)
		{
			PreBoot(controller, parent, prm);
			using (m_TransLock.Lock(TransLockFlag.Boot))
			{
				if (HasModule(ModuleType.PreBoot)) yield return Modules.OnPreBoot(this);
				yield return OnBoot();
				if (HasModule(ModuleType.Boot)) yield return Modules.OnBoot(this);
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
				if (HasModule(ModuleType.PreEnable)) yield return Modules.OnPreEnable(this);
				yield return OnEnable();
				foreach (var child in m_Children)
				{
					yield return child.DoEnable();
				}
				if (HasModule(ModuleType.Enable)) yield return Modules.OnEnable(this);
			}
		}

		IEnumerator DoRun()
		{
			using (m_TransLock.Lock(TransLockFlag.RunOrSuspend))
			{
				if (m_Shutdown || Running) yield break;
				Running = true;
				if (HasModule(ModuleType.PreRun)) yield return Modules.OnPreRun(this);
				yield return DoEnable();
				yield return OnRun();
				if (HasModule(ModuleType.Run)) yield return Modules.OnRun(this);
				OnCompleteRun();
			}
		}

		IEnumerator DoDisable()
		{
			using (m_TransLock.Lock(TransLockFlag.EnableOrDisable))
			{
				if (Running) yield break;
				Call.Enabled = false;
				if (HasModule(ModuleType.PreDisable)) yield return Modules.OnPreDisable(this);
				foreach (var child in m_Children)
				{
					yield return child.DoDisable();
				}
				yield return OnDisable();
				if (HasModule(ModuleType.Disable)) yield return Modules.OnDisable(this);
			}
		}

		IEnumerator DoSuspend()
		{
			using (m_TransLock.Lock(TransLockFlag.RunOrSuspend))
			{
				if (m_Shutdown || !Running) yield break;
				Running = false;
				if (HasModule(ModuleType.PreSuspend)) yield return Modules.OnPreSuspend(this);
				yield return DoDisable();
				yield return OnSuspend();
				if (HasModule(ModuleType.Suspend)) yield return Modules.OnSuspend(this);
			}
		}

		internal IEnumerator DoShutdown()
		{
			m_Shutdown = true;
			Call.Dispose();
			Parent?.m_Children.Remove(this);
			//ブートシーケンスだけは待つ
			while (m_TransLock.IsLock(TransLockFlag.Boot))
			{
				yield return null;
			}
			using (m_TransLock.Lock(TransLockFlag.Shutdown))
			{
				OnPreShutdown();
				if (HasModule(ModuleType.PreShutdown)) yield return Modules.OnPreShutdown(this);
				foreach (var child in m_Children)
				{
					yield return child.DoShutdown();
				}
				yield return OnShutdown();
				if (HasModule(ModuleType.Shutdown)) yield return Modules.OnShutdown(this);
			}
		}

		IEnumerator DoSwitch(Type type, object prm)
		{
			var next = (Content)Activator.CreateInstance(type);
			next.PreBoot(Controller, Parent, prm);

			if (HasModule(ModuleType.PreSwitch)) yield return Modules.OnPreSwitch(this, next);

			yield return DoShutdown();

			if (next.HasModule(ModuleType.Switch)) yield return next.Modules.OnSwitch(this, next);

			using (next.m_TransLock.Lock(TransLockFlag.Boot))
			{
				if (next.HasModule(ModuleType.PreBoot)) yield return next.Modules.OnPreBoot(next);

				yield return next.OnBoot();

				if (next.HasModule(ModuleType.Boot)) yield return next.Modules.OnBoot(next);

				if (next.HasModule(ModuleType.EndSwitch)) yield return next.Modules.OnEndSwitch(this, next);

				yield return next.DoRun();
			}
			//遷移先を送信
			yield return Result<IContentRef>.Create(new Ref(next));
		}


		public IContentRef Get<T>()
		{
			foreach (var child in m_Children)
			{
				if (child is T) return new Ref(child);
			}
			return null;
		}

		public IContentRef Get(Type type)
		{
			foreach (var child in m_Children)
			{
				if (type.IsAssignableFrom(child.GetType())) return new Ref(child);
			}
			return null;
		}

		public IEnumerable<IContentRef> GetAll<T>()
		{
			foreach (var child in m_Children)
			{
				if (child is T) yield return new Ref(child);
			}
		}
		
		protected void ThrowException(Exception exception)
		{
			bool ret = false;
			try
			{
				ret = HandleException(exception);
			}
			catch (Exception e)
			{
				Controller.ThrowException(exception);
				throw e;
			}
			
			if (!ret)
			{
				Controller.ThrowException(exception);
			}
		}

		protected virtual bool HandleException(Exception ex)
		{
			return false;
		}

	}
}
