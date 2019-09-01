using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.UI
{
	/// <summary>
	/// UIの表示をキュー制御で行います。現在のUIが閉じられるまで、次の表示リクエストは実行されません。
	/// 表示前に不要になったリクエストはキャンセルできます。
	/// </summary>
	public class UIQueue : UIQueue<object, IControl> { }

	/// <summary>
	/// UIの表示をキュー制御で行います。現在のUIが閉じられるまで、次の表示リクエストは実行されません。
	/// 表示前に不要になったリクエストはキャンセルできます。
	/// </summary>
	public abstract class UIQueue<TParam, UControl> : UIController<TParam, UControl>, IQueueController where UControl : class, IControl
	{
		bool m_IsRun;
		QueueEntry m_Current;
		List<QueueEntry> m_Queue = new List<QueueEntry>();

		protected override IEnumerable<T> GetActive<T>()
		{
			var ui = m_Current?.Instance?.Control ?? null;
			if (ui != null && ui.IsActive && ui is T target)
			{
				yield return target;
			}
		}

		public IQueueEntry Enqueue(string path, TParam prm)
		{
			var entry = new QueueEntry(this);
			entry.SetOpenAction(() => EnqueueImpl(entry, path, prm));
			m_Queue.Add(entry);
			TryRun();
			return entry;
		}

		ITriggerAction<UIInstance> EnqueueImpl(QueueEntry entry, string path, TParam prm)
		{
			var prev = m_Current;
			m_Current = entry;
			TryRun();
			if (prev != null)
			{
				return Change<UControl>(path, prm, null, new UIInstance[] { prev.Instance });
			}
			else
			{
				return Open<UControl>(path, prm);
			}
		}

		protected override ITriggerAction Close(IControl control)
		{
			var cur = m_Current?.Instance?.Control ?? null;
			if (cur == control && control != null)
			{
				return Close(m_Current);
			}
			return Trigger.Successed;
		}

		void IQueueController.Cancel(QueueEntry entry)
		{
			m_Queue.Remove(entry);
		}

		public ITriggerAction<bool> Close(IQueueEntry entry)
		{
			var _entry = (entry as QueueEntry);
			m_Queue.Remove(_entry);
			if (m_Current == entry)
			{
				if (m_Queue.Count == 0)
				{
					return Close(new UIInstance[] { _entry.Instance });
				}
				else
				{
					var next = m_Queue[0];
					m_Queue.RemoveAt(0);
					return next.Open();
				}
			}
			return Trigger.Successed;
		}

		void TryRun()
		{
			if (!m_IsRun)
			{
				this.Routine(Run()).AddFail(x =>
				{
					Debug.LogException(x);
					m_IsRun = false;
				});
			}
		}

		IEnumerator Run()
		{
			if (m_IsRun) yield break;
			m_IsRun = true;
			while (m_Queue.Count > 0 || m_Current != null)
			{
				if (m_Current != null)
				{
					while (!m_Current.IsClosed)
					{
						yield return null;
					}
					m_Current = null;
				}
				if (m_Queue.Count > 0)
				{
					var next = m_Queue[0];
					m_Queue.RemoveAt(0);
					next.Open();
				}
			}
			m_IsRun = false;
		}


	}

}
