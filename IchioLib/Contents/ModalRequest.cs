using System;

namespace ILib
{
	using Logger;

	internal class ModalRequest
	{
		Type m_Type;
		Action<object, Exception> m_Action;
		object m_Result = null;
		Exception m_Error = null;

		public bool HasResult => m_Result != null || m_Error != null;

		public ModalRequest(Type type)
		{
			m_Type = type;
		}

		public ITriggerAction<T> CreateResultAction<T>()
		{
			Trigger<T> trigger = new Trigger<T>();
			m_Action = (obj, ex) =>
			{
				if (ex != null)
				{
					trigger.Error(ex);
				}
				else
				{
					trigger.Fire((T)obj);
				}
			};
			return trigger.Action;
		}

		public void SetResult(object obj, Exception ex)
		{
			if (ex != null)
			{
				m_Error = ex;
				Log.Debug("[ilib-content]set modal error {0}", ex);
				return;
			}
			if (!m_Type.IsAssignableFrom(obj.GetType()))
			{
				throw new InvalidCastException(string.Format("Modal result is invalid cast {0} <= {1}", m_Type, obj));
			}
			Log.Debug("[ilib-content]set modal result {0}", obj);
			m_Result = obj;
		}

		public void Dispatch()
		{
			Log.Trace("[ilib-content]dispatch modal result.");
			m_Action(m_Result, m_Error);
		}

	}

}
