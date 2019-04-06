using System;
using System.Collections;
using System.Collections.Generic;

namespace ILib
{
	public class Trigger
	{
		public static IEnumerator Wait(params ITriggerAction[] actions)
		{
			foreach (var action in actions)
			{
				while (!action.Fired) yield return null;
			}
		}

		public static IEnumerator Wait(IEnumerable<ITriggerAction> actions)
		{
			foreach (var action in actions)
			{
				while (!action.Fired) yield return null;
			}
		}
	}

	public interface ITrigger<T>
	{
		ITriggerAction<T> Action { get; }
	}

	public class Trigger<T> : ITrigger<T>
	{
		TriggerAction<T> m_Action;
		public ITriggerAction<T> Action => m_Action;

		public bool Fired => m_Action.Fired;

		public Trigger(bool oneShot = true)
		{
			m_Action = new TriggerAction<T>(oneShot);
		}

		public void Fire(T item)
		{
			m_Action.Fire(item, null);
		}

		public void Error(Exception ex)
		{
			m_Action.Fire(default(T), ex);
		}

	}

}
