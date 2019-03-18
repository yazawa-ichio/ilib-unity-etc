//#define ILIB_DISABLE_SAFE_TRIGGER
using System;
using System.Collections.Generic;

namespace ILib.Triggers
{
	internal class ActionHolder<T>
	{
		ITriggerAction m_Owner;
		Action<T> m_Action;
		List<Action<T>> m_List;

		public ActionHolder(ITriggerAction owner)
		{
			m_Owner = owner;
		}

		public void Invoke(T item, bool cache)
		{
#if ILIB_DISABLE_SAFE_TRIGGER
			m_action?.Invoke(item);
			if(cache) m_action = null;
#else
			try
			{
				m_Action?.Invoke(item);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
			}
			if (m_List != null)
			{
				foreach (var action in m_List)
				{
					try
					{
						action?.Invoke(item);
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogException(ex);
					}
				}
			}
			if (cache)
			{
				m_Action = null;
				m_List = null;
			}
#endif
		}

		public void Add(Action<T> action)
		{
#if ILIB_DISABLE_SAFE_TRIGGER
			m_action += action;
#else
			if (m_Action == null && m_List == null)
			{
				m_Action = action;
			}
			else
			{
				if (m_List == null)
				{
					m_List = new List<Action<T>>(4);
					m_List.Add(m_Action);
					m_Action = null;
				}
				m_List.Add(action);
			}
#endif
		}

		public void Remove(Action<T> action)
		{
#if ILIB_DISABLE_SAFE_TRIGGER
			m_action -= action;
#else
			if (m_List != null)
			{
				m_List.Remove(action);
			}
			else if (m_Action == action)
			{
				m_Action = null;
			}
#endif
		}

		public void Clear()
		{
			m_Owner = null;
			m_Action = null;
			m_List = null;
		}

	}

}
