using System;

namespace ILib.UI
{
	internal class QueueEntry : IQueueEntry
	{
		public bool IsClosed
		{
			get
			{
				if (m_Cancel) return true;
				if (Instance != null)
				{
					return Instance.Object == null;
				}
				return false;
			}
		}

		Action m_OnClosed;
		public ITriggerAction ObserveClosed
		{
			get
			{
				if (IsClosed)
				{
					return Trigger.Successed;
				}
				Trigger trigger = new Trigger();
				m_OnClosed += trigger.Fire;
				return trigger.Action;
			}
		}

		public UIInstance Instance;
		IQueueController m_Parent;
		Func<ITriggerAction<UIInstance>> m_Open;
		ITriggerAction<UIInstance> m_Opening;
		bool m_Close;
		bool m_Cancel;

		public QueueEntry(IQueueController parent)
		{
			m_Parent = parent;
		}

		public void SetOpenAction(Func<ITriggerAction<UIInstance>> open)
		{
			m_Open = open;
		}

		public ITriggerAction<bool> Open()
		{
			if (m_Open == null) return Trigger.Successed;
			m_Opening = m_Open().Add((x, ex) =>
			{
				Instance = x;
			});
			m_Open = null;
			return m_Opening.Select(x => true);
		}

		public ITriggerAction<bool> Close()
		{
			if (m_Close) return Trigger.Successed;
			m_Close = true;
			if (m_Opening == null)
			{
				m_Cancel = true;
				m_Parent.Cancel(this);
				m_Open = null;
				m_OnClosed?.Invoke();
				m_OnClosed = null;
				return Trigger.Successed;
			}
			else
			{
				return m_Opening.Then(x => m_Parent.Close(this)).Add((x, ex) =>
				{
					m_OnClosed?.Invoke();
					m_OnClosed = null;
				});
			}
		}

		public void Dispose()
		{
			Close();
		}
	}

}
