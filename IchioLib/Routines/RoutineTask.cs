using System;
using System.Collections;
using System.Collections.Generic;

namespace ILib
{
	using Routines;
	using UnityEngine;

	public class NoResultTaskRoutineException : Exception
	{
		public NoResultTaskRoutineException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// Unityのコルーチンのラッパーです。
	/// IHasResult[T]型を返すイテレータをコンストラクタで指定してください。
	/// 値が返された時点で完了扱いとなります。
	/// </summary>
	public class TaskRoutine<T> : RoutineBase, IRoutine<T>
	{
		Trigger<T> m_Trigger = new Trigger<T>(oneShot: true);
		public ITriggerAction<T> Action => m_Trigger.Action;

		public TaskRoutine(MonoBehaviour behaviour, IEnumerator routine) : base(behaviour, routine) { }

		protected override bool Result(IHasResult result)
		{
			var ret = result as IHasResult<T>;
			if (ret != null)
			{
				m_Trigger.Fire(ret.Value);
				return true;
			}
			return false;
		}

		protected override void Fail(Exception ex)
		{
			m_Trigger.Error(ex);
		}

		protected override void Success()
		{
			Fail(new NoResultTaskRoutineException("not result task."));
		}
	}

	/// <summary>
	/// Unityのコルーチンのラッパーです。
	/// IHasResult[T]型を返すイテレータをコンストラクタで指定してください。
	/// 値が返されるたびにトリガーが実行されます。
	/// </summary>
	public class IterationTaskRoutine<T> : RoutineBase, IRoutine<T>
	{
		Trigger<T> m_Trigger = new Trigger<T>(oneShot: false);
		Trigger<bool> m_CompleteTrigger = new Trigger<bool>(oneShot: false);

		public ITriggerAction<T> Action => m_Trigger.Action;
		public ITriggerAction<bool> Complete => m_CompleteTrigger.Action;

		public IterationTaskRoutine(MonoBehaviour behaviour, IEnumerator routine) : base(behaviour, routine) { }
		public IterationTaskRoutine(MonoBehaviour behaviour, Func<IEnumerator> routine) : base(behaviour, routine) { }

		protected override bool Result(IHasResult result)
		{
			var ret = result as IHasResult<T>;
			if (ret != null)
			{
				m_Trigger.Fire(ret.Value);
			}
			return false;
		}

		protected override void Fail(Exception ex)
		{
			m_Trigger.Error(ex);
		}

		protected override void Success()
		{
			m_CompleteTrigger.Fire(m_Trigger.Fired);
		}
	}

	/// <summary>
	/// Unityのコルーチンのラッパーです。
	/// IHasResult型を返すイテレータをコンストラクタで指定してください。
	/// 値が返されるたびにトリガーが実行されます。
	/// </summary>
	public class IterationTaskRoutine : RoutineBase, IRoutine<object>
	{
		Trigger<object> m_Trigger = new Trigger<object>(oneShot: false);
		Trigger<bool> m_CompleteTrigger = new Trigger<bool>(oneShot: true);

		public ITriggerAction<object> Action => m_Trigger.Action;
		public ITriggerAction<bool> Complete => m_CompleteTrigger.Action;

		public IterationTaskRoutine(MonoBehaviour behaviour, IEnumerator routine) : base(behaviour, routine) { }

		public IterationTaskRoutine(MonoBehaviour behaviour, Func<IEnumerator> routine) : base(behaviour, routine) { }

		protected override bool Result(IHasResult result)
		{
			if (result != null)
			{
				m_Trigger.Fire(result.Value);
			}
			return false;
		}

		protected override void Fail(Exception ex)
		{
			m_Trigger.Error(ex);
		}

		protected override void Success()
		{
			m_CompleteTrigger.Fire(m_Trigger.Fired);
		}
	}

	/// <summary>
	/// Unityのコルーチンのラッパーです。
	/// IHasResult[T]型を返すイテレータをコンストラクタで指定してください。
	/// 値が返されるたびにトリガーが実行されます。
	/// 完了後に再度実行が可能です。
	/// </summary>
	public class RepeatTaskRoutine<T> : RoutineBase, IRoutine<T>
	{
		Trigger<T> m_Trigger = new Trigger<T>(oneShot: false);
		Trigger<bool> m_CompleteTrigger = new Trigger<bool>(oneShot: false);

		public ITriggerAction<T> Action => m_Trigger.Action;
		public ITriggerAction<bool> Complete => m_CompleteTrigger.Action;

		public RepeatTaskRoutine(MonoBehaviour behaviour, Func<IEnumerator> routine) : base(behaviour, routine) { }

		protected override bool Result(IHasResult result)
		{
			var ret = result as IHasResult<T>;
			if (ret != null)
			{
				m_Trigger.Fire(ret.Value);
			}
			return false;
		}

		protected override void Fail(Exception ex)
		{
			m_Trigger.Error(ex);
			m_CompleteTrigger.Error(ex);
		}

		protected override void Success()
		{
			m_CompleteTrigger.Fire(m_Trigger.Fired);
		}

	}
}
