using System;
using System.Collections;
using System.Collections.Generic;

namespace ILib
{
	using Triggers;

	public interface ITriggerAction : IDisposable
	{
		event Action<Exception> OnFail;
		event Action OnCancel;
		event Action<bool> OnComplete;
		bool Fired { get; }
		Exception Error { get; }
		bool Canceled { get; }
		bool OneShot { get; }
		void Clear();
		void Cancel();
		IEnumerator Wait();
	}

	public interface ITriggerAction<T> : ITriggerAction
	{
		event Action<T> OnFire;
		ITriggerAction<T> Add(Action<T> action);
		ITriggerAction<T> Remove(Action<T> action);
		ITriggerAction<T> Add(Action<T, Exception> action);
		ITriggerAction<T> AddFail(Action<Exception> action);
		ITriggerAction<T> RemoveFail(Action<Exception> action);
		ITriggerAction<T> AddComplete(Action<bool> action);
		ITriggerAction<T> RemoveComplete(Action<bool> action);
	}

	public class TriggerAction<T> : ITriggerAction<T>
	{
		public static readonly TriggerAction<T> Empty;
		static TriggerAction()
		{
			Empty = new TriggerAction<T>(true);
			Empty.Cancel();
		}

		public event Action<T> OnFire { add { Add(value); } remove { Remove(value); } }
		public event Action<Exception> OnFail { add { AddFail(value); } remove { RemoveFail(value); } }
		public event Action<bool> OnComplete { add { AddComplete(value); } remove { RemoveComplete(value); } }
		public event Action OnCancel
		{
			add { if (Canceled) value?.Invoke(); else m_OnCancel += value; }
			remove { m_OnCancel -= value; }
		}

		public bool Canceled { get; private set; }
		public bool OneShot
		{
			get { return m_OneShot; }
			set { if (Fired) throw new InvalidOperationException(); m_OneShot = value; }
		}

		ActionHolder<T> m_Action;
		ActionHolder<Exception> m_Fail;
		ActionHolder<bool> m_Complete;

		Action m_OnCancel = null;
		bool m_OneShot = true;
		public bool Fired { get; private set; }
		public T Result { get; private set; }
		public Exception Error { get; private set; }

		public TriggerAction(bool oneshot = true) => m_OneShot = oneshot;

		internal protected void Fire(T ret, Exception ex)
		{
			if (Fired && m_OneShot)
			{
				return;
			}
			Fired = true;
			Result = ret;
			Error = ex;
			if (ex != null)
			{
				m_Fail?.Invoke(ex, m_OneShot);
			}
			else
			{
				m_Action?.Invoke(ret, m_OneShot);
			}
			m_Complete?.Invoke(true, m_OneShot);
		}

		public ITriggerAction<T> Add(Action<T> action)
		{
			if (Canceled || action == null) return this;
			if (Fired && m_OneShot && Error == null)
			{
				action.Invoke(Result);
			}
			else
			{
				if (m_Action == null) m_Action = new ActionHolder<T>(this);
				m_Action.Add(action);
			}
			return this;
		}

		public ITriggerAction<T> Remove(Action<T> action)
		{
			if (Canceled || action == null || m_Action == null) return this;
			m_Action.Remove(action);
			return this;
		}

		public ITriggerAction<T> AddFail(Action<Exception> action)
		{
			if (Canceled || action == null) return this;
			if (Fired && m_OneShot && Error != null)
			{
				action(Error);
			}
			else
			{
				if (m_Fail == null) m_Fail = new ActionHolder<Exception>(this);
				m_Fail.Add(action);
			}
			return this;
		}

		public ITriggerAction<T> RemoveFail(Action<Exception> action)
		{
			if (Canceled || action == null || m_Fail == null) return this;
			m_Fail.Remove(action);
			return this;
		}

		public ITriggerAction<T> Add(Action<T, Exception> onAction)
		{
			if (Canceled) return this;
			Add((T item) => onAction?.Invoke(item, null));
			AddFail((Exception item) => onAction?.Invoke(default(T), item));
			return this;
		}

		public ITriggerAction<T> AddComplete(Action<bool> action)
		{
			if (action == null) return this;
			if (Canceled)
			{
				action.Invoke(false);
			}
			else if (Fired)
			{
				action.Invoke(true);
			}
			else
			{
				if (m_Complete == null) m_Complete = new ActionHolder<bool>(this);
				m_Complete.Add(action);
			}
			return this;
		}

		public ITriggerAction<T> RemoveComplete(Action<bool> action)
		{
			if (Canceled || action == null || m_Complete == null) return this;
			m_Complete.Remove(action);
			return this;
		}

		public IEnumerator Wait()
		{
			while (!Fired) yield return null;
		}

		public void Clear()
		{
			if (Fired && m_OneShot) throw new InvalidOperationException("clear is before Fired");
			m_Complete?.Clear();
			m_Action?.Clear();
			m_Fail?.Clear();
			m_OnCancel = null;
		}

		public void Cancel()
		{
			Canceled = true;
			m_Complete?.Invoke(false, true);
			m_OnCancel?.Invoke();
			m_OnCancel = null;

			m_Complete?.Clear();
			m_Action?.Clear();
			m_Fail?.Clear();
		}

		public void Dispose()
		{
			Cancel();
		}

	}
}
