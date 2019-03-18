using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib
{
	using System;
	using Routines;

	public interface IRoutine : IDisposable
	{
		bool IsRunning { get; }
		bool Restartable { get; }
		void Start();
		void Cancel();
	}

	public interface IRoutine<T> : ITrigger<T>, IRoutine
	{
	}

	/// <summary>
	/// Unityのコルーチンのラッパーです。
	/// 完了と例外を取得できます。
	/// </summary>
	public class Routine : RoutineBase, IRoutine<bool>
	{
		Trigger<bool> m_Trigger = new Trigger<bool>(oneShot: true);

		public ITriggerAction<bool> Action => m_Trigger.Action;

		public Routine(MonoBehaviour behaviour, IEnumerator routine) : base(behaviour, routine) { }

		protected override void Fail(Exception ex)
		{
			m_Trigger.Error(ex);
		}

		protected override void Success()
		{
			m_Trigger.Fire(true);
		}

		public static Routine Start(MonoBehaviour owner, IEnumerator routine) => new Routine(owner, routine);

		public static RepeatRoutine Repeat(MonoBehaviour owner, Func<IEnumerator> routine) => new RepeatRoutine(owner, routine);

		public static TaskRoutine<T> Task<T>(MonoBehaviour owner, IEnumerator routine) => new TaskRoutine<T>(owner, routine);

		public static IterationTaskRoutine IterationTask(MonoBehaviour owner, IEnumerator routine) => new IterationTaskRoutine(owner, routine);

		public static IterationTaskRoutine IterationTask(MonoBehaviour owner, Func<IEnumerator> routine) => new IterationTaskRoutine(owner, routine);

		public static IterationTaskRoutine<T> IterationTask<T>(MonoBehaviour owner, IEnumerator routine) => new IterationTaskRoutine<T>(owner, routine);

		public static IterationTaskRoutine<T> IterationTask<T>(MonoBehaviour owner, Func<IEnumerator> routine) => new IterationTaskRoutine<T>(owner, routine);

		public static RepeatTaskRoutine<T> RepeatTask<T>(MonoBehaviour owner, Func<IEnumerator> routine) => new RepeatTaskRoutine<T>(owner, routine);

	}

	/// <summary>
	/// Unityのコルーチンのラッパーです。
	/// 完了と例外を取得できます。
	/// 完了後に再度実行が可能です。
	/// </summary>
	public class RepeatRoutine : RoutineBase, IRoutine<bool>
	{
		Trigger<bool> m_Trigger = new Trigger<bool>(oneShot: false);

		public ITriggerAction<bool> Action => m_Trigger.Action;

		public RepeatRoutine(MonoBehaviour behaviour, Func<IEnumerator> routine) : base(behaviour, routine) { }

		protected override void Fail(Exception ex)
		{
			m_Trigger.Error(ex);
		}

		protected override void Success()
		{
			m_Trigger.Fire(true);
		}
	}

}
