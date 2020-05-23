using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace ILib
{
	public readonly struct TriggerActionAwaiter<T> : ICriticalNotifyCompletion
	{
		readonly ITriggerAction<T> m_Action;

		public bool IsCompleted => m_Action.Fired;

		public TriggerActionAwaiter(ITriggerAction<T> action)
		{
			m_Action = action;
		}

		public TriggerActionAwaiter(IHasTriggerAction<T> trigger)
		{
			m_Action = trigger.Action;
		}

		public void OnCompleted(Action continuation)
		{
			if (m_Action.Fired)
			{
				continuation();
				return;
			}
			m_Action.OnComplete += x => continuation();
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			if (m_Action.Fired)
			{
				continuation();
				return;
			}
			m_Action.OnComplete += x => continuation();
		}

		public T GetResult()
		{
			if (m_Action.Canceled)
			{
				throw new TaskCanceledException("trigger is canceled.");
			}
			if (m_Action.Error != null)
			{
				throw m_Action.Error;
			}
			return m_Action.Result;
		}
	}

	public readonly struct TriggerActionAwaiter : ICriticalNotifyCompletion
	{
		readonly ITriggerAction m_Action;

		public bool IsCompleted => m_Action.Fired;

		public TriggerActionAwaiter(ITriggerAction action)
		{
			m_Action = action;
		}

		public TriggerActionAwaiter(IHasTriggerAction trigger)
		{
			m_Action = trigger.Action;
		}

		public void OnCompleted(Action continuation)
		{
			if (m_Action.Fired)
			{
				continuation();
				return;
			}
			m_Action.OnComplete += x => continuation();
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			if (m_Action.Fired)
			{
				continuation();
				return;
			}
			m_Action.OnComplete += x => continuation();
		}

		public void GetResult()
		{
			if (m_Action.Canceled)
			{
				throw new TaskCanceledException("trigger is canceled.");
			}
			if (m_Action.Error != null)
			{
				throw m_Action.Error;
			}
		}
	}
}