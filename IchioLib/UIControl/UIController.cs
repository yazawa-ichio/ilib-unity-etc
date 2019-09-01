using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using ILib.Triggers;

namespace ILib.UI
{
	/// <summary>
	/// UIの表示制御の基底クラスです。
	/// Open・Change・Closeの操作を行えます。
	/// 実行中に非アクティブにするとコルーチンが停止するため正常に動かなくなります。
	/// </summary>
	public abstract class UIController<TParam, UControl> : MonoBehaviour, IController where UControl : class, IControl
	{

		ITriggerAction IController.Close(IControl control) => Close(control);

		protected abstract ITriggerAction Close(IControl control);

		/// <summary>
		/// アセットバンドル等から読む時は継承先で書き換えてください。
		/// </summary>
		protected virtual ITriggerAction<GameObject> Load<T>(string path, TParam prm)
		{
			var trigger = new Trigger<GameObject>();
			var loading = Resources.LoadAsync<GameObject>(path);
			loading.completed += (_) =>
			{
				var ret = loading.asset as GameObject;
				if (ret != null)
				{
					trigger.Fire(ret);
				}
				else
				{
					trigger.Error(new Exception($"not found {path}, {prm}, {typeof(T)}"));
				}
			};
			return trigger.Action;
		}

		protected abstract IEnumerable<T> GetActive<T>();

		/// <summary>
		/// 現在アクティブな指定型のUIに対して処理を行います。
		/// </summary>
		public bool Execute<T>(Action<T> action)
		{
			bool ret = false;
			foreach (var ui in GetActive<T>())
			{
				ret = true;
				action.Invoke(ui);
			}
			return ret;
		}

		/// <summary>
		/// 現在アクティブな指定型のUIに対して一つだけ処理を行います。
		/// </summary>
		public bool ExecuteAnyOne<T>(Action<T> action)
		{
			return ExecuteAnyOne<T>(x =>
			{
				action(x);
				return true;
			});
		}

		/// <summary>
		/// 現在アクティブな指定型のUIに対して一つだけ処理を行います。
		/// </summary>
		public bool ExecuteAnyOne<T>(Func<T,bool> action)
		{
			foreach (var ui in GetActive<T>())
			{
				var ret = action.Invoke(ui);
				if (ret) return true;
			}
			return false;
		}

		/// <summary>
		/// IExecuteBackを実装したUIに対してバック処理を行います。
		/// </summary>
		public bool ExecuteBack()
		{
			return ExecuteAnyOne<IExecuteBack>(x => x.TryBack());
		}

		Queue<Action> m_ProcessRequest = new Queue<Action>();

		/// <summary>
		/// 実行中のプロセスがある場合にtrueが返ります。
		/// UIQueueの場合、表示待ちのリクエストはプロセスに含まれないことに注意してください。
		/// </summary>
		public bool HasProcess => m_ProcessCount > 0;

		protected void EnqueueProcess(Action action)
		{
			if (m_ProcessCount > 0)
			{
				m_ProcessRequest.Enqueue(action);
			}
			else
			{
				action?.Invoke();
			}
		}

		int m_ProcessCount = 0;
		protected void StartProcess()
		{
			if (m_ProcessCount == 0) OnStartProcess();
			 m_ProcessCount++;
		}

		protected void EndProcess()
		{
			m_ProcessCount--;
			if (m_ProcessCount == 0)
			{
				OnEndProcess();
				while (m_ProcessRequest.Count > 0 && !HasProcess)
				{
					m_ProcessRequest.Dequeue().Invoke();
				}
			}
		}

		/// <summary>
		/// プロセスが開始された際に実行されます。
		/// ダブルタップが出来ないように入力制限を行う等に利用します。
		/// </summary>
		protected virtual void OnStartProcess() { }

		/// <summary>
		/// 全てのプロセスが終了した際に実行されます。
		/// ダブルタップが出来ないように入力制限を行う等に利用します。
		/// </summary>
		protected virtual void OnEndProcess() { }

		/// <summary>
		/// オープン処理が開始された際に実行されます。
		/// </summary>
		protected virtual ITriggerAction OnBeginOpen() => Trigger.Successed;

		/// <summary>
		/// UIが生成された直後に実行されます。
		/// </summary>
		protected virtual void OnOpen(UControl ui) { }

		/// <summary>
		/// オープン処理が完了した際に実行されます。
		/// </summary>
		protected virtual ITriggerAction OnEndOpen() => Trigger.Successed;

		/// <summary>
		/// 親のUIのBehind処理の実行が完了するまでOnFrontの処理を実行しません
		/// 標準は無効です。
		/// </summary>
		protected virtual bool IsWaitBehindBeforeOnFront<T>(string path, TParam prm) where T : UControl
		{
			return false;
		}

		protected ITriggerAction<UIInstance> Open<T>(string path, TParam prm, UIInstance parent = null) where T : UControl
		{
			StartProcess();
			var task = this.TaskRoutine<UIInstance>(OpenImpl<T>(path, prm, parent));
			task.Action.OnComplete += _ =>
			{
				EndProcess();
			};
			return task.Action;
		}

		private IEnumerator OpenImpl<T>(string path, TParam prm, UIInstance parent = null) where T : UControl
		{
			yield return OnBeginOpen();

			var loading = Load<T>(path, prm);

			yield return loading;

			if (loading.Error != null) throw loading.Error;

			var prefab = loading.Result;

			var obj = Instantiate(prefab, transform);
			var ui = obj.GetComponent<T>();

			ui.SetController(this);

			var behind = parent?.Control?.OnBehind() ?? null;

			yield return ui.OnCreated(prm);

			OnOpen(ui);

			if (behind != null && IsWaitBehindBeforeOnFront<T>(path, prm))
			{
				yield return behind;
				behind = null;
				if (parent.Control.IsDeactivateInBehind) parent.Object.SetActive(false);
			}

			yield return ui.OnFront(open: true);

			if (behind != null)
			{
				yield return behind;
				if (parent.Control.IsDeactivateInBehind) parent.Object.SetActive(false);
			}

			yield return OnEndOpen();

			var ret = new UIInstance();
			ret.Control = ui;
			ret.Object = obj;
			ret.Parent = parent;
			yield return Routines.Result<UIInstance>.Create(ret);

		}

		protected ITriggerAction CloseControls(UIInstance[] controls)
		{
			if (controls == null || controls.Length == 0)
			{
				return Trigger.Successed;
			}
			var _releases = controls.Select(x =>
			{
				var trigger = x.Control.OnClose();
				trigger.OnComplete += _ =>
				{
					OnClose(x.Control as UControl);
					Destroy(x.Object);
				};
				return trigger;
			});
			return Trigger.Combine(_releases.ToArray());
		}

		/// <summary>
		/// 遷移処理が開始された際に実行されます。
		/// </summary>
		protected virtual ITriggerAction OnBeginChange() => Trigger.Successed;

		/// <summary>
		/// 遷移処理が完了した際に実行されます。
		/// </summary>
		protected virtual ITriggerAction OnEndChange() => Trigger.Successed;

		/// <summary>
		/// 親のUIのClose処理の実行が完了するまでOnOpenの処理を実行しません
		/// 標準は無効です。
		/// </summary>
		protected virtual bool IsWaitCloseBeforeOnOpen<T>(string path, TParam prm) where T : UControl
		{
			return false;
		}

		protected ITriggerAction<UIInstance> Change<T>(string path, TParam prm, UIInstance parent, UIInstance[] releases) where T :UControl
		{
			StartProcess();
			var task = this.TaskRoutine<UIInstance>(ChangeImpl<T>(path, prm, parent, releases));
			task.Action.OnComplete += _ =>
			{
				EndProcess();
			};
			return task.Action;
		}

		private IEnumerator ChangeImpl<T>(string path, TParam prm, UIInstance parent, UIInstance[] releases) where T : UControl
		{
			yield return OnBeginChange();

			var loading = Load<T>(path, prm);

			var close = CloseControls(releases);

			yield return loading;

			if (IsWaitCloseBeforeOnOpen<T>(path, prm))
			{
				yield return close;
			}

			if (loading.Error != null) throw loading.Error;

			var prefab = loading.Result;

			var obj = Instantiate(prefab, transform);
			var ui = obj.GetComponent<T>();

			ui.SetController(this);

			yield return ui.OnCreated(prm);

			OnOpen(ui);

			yield return ui.OnFront(open: true);

			if (!close.Fired)
			{
				yield return close;
			}

			yield return OnEndChange();

			var ret = new UIInstance();
			ret.Control = ui;
			ret.Object = obj;
			ret.Parent = parent;
			yield return Routines.Result<UIInstance>.Create(ret);

		}

		/// <summary>
		/// 削除処理が開始された際に実行されます。
		/// </summary>
		protected virtual ITriggerAction OnBeginClose() => Trigger.Successed;

		/// <summary>
		/// インスタンスを削除する直前に実行されます。
		/// </summary>
		protected virtual void OnClose(UControl ui) { }

		/// <summary>
		/// 削除処理が完了した際に実行されます。
		/// </summary>
		protected virtual ITriggerAction OnEndClose() => Trigger.Successed;

		/// <summary>
		/// Close処理の実行が完了するまでOnFrontの処理を実行しません
		/// 標準は無効です。
		/// </summary>
		protected virtual bool IsWaitCloseBeforeOnFront(UControl font)
		{
			return false;
		}

		protected ITriggerAction<bool> Close(UIInstance[] releases, UIInstance front = null)
		{
			StartProcess();
			var task = this.Routine(CloseImpl(releases, front));
			task.Action.OnComplete += _ =>
			{
				EndProcess();
			};
			return task.Action;
		}

		protected IEnumerator CloseImpl(UIInstance[] releases, UIInstance front = null)
		{
			yield return OnBeginClose();

			var close = CloseControls(releases);

			if (front != null)
			{
				if (!front.Object.activeSelf) front.Object.SetActive(true);
				if (IsWaitCloseBeforeOnFront(front.Control as UControl))
				{
					yield return close;
				}
				yield return front.Control.OnFront(open: false);
			}

			if(!close.Fired)
			{
				yield return close;
			}

			yield return OnEndClose();

		}

	}

}
