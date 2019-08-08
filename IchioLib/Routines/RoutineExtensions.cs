using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib
{
	public interface IHasRoutineOwner
	{
		MonoBehaviour RoutineOwner { get; }
	}

	public static class RoutineExtensions
	{
		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// 完了と例外を取得できます。
		/// </summary>
		public static ILib.Routine Routine(this MonoBehaviour behaviour, IEnumerator routine)
		{
			return ILib.Routine.Start(behaviour, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// 完了と例外を取得できます。
		/// 完了後に再度実行が可能です。
		/// </summary>
		public static ILib.RepeatRoutine RepeatRoutine(this MonoBehaviour behaviour, Func<IEnumerator> routine)
		{
			return ILib.Routine.Repeat(behaviour, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// IHasResult[T]型を返すイテレータをコンストラクタで指定してください。
		/// 値が返された時点で完了扱いとなります。
		/// </summary>
		public static ILib.TaskRoutine<T> TaskRoutine<T>(this MonoBehaviour behaviour, IEnumerator routine)
		{
			return ILib.Routine.Task<T>(behaviour, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// IHasResult型を返すイテレータをコンストラクタで指定してください。
		/// 値が返されるたびにトリガーが実行されます。
		/// </summary>
		public static ILib.IterationTaskRoutine IterationTaskRoutine(this MonoBehaviour behaviour, IEnumerator routine)
		{
			return ILib.Routine.IterationTask(behaviour, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// IHasResult型を返すイテレータをコンストラクタで指定してください。
		/// 値が返されるたびにトリガーが実行されます。
		/// </summary>
		public static ILib.IterationTaskRoutine IterationTaskRoutine(this MonoBehaviour behaviour, Func<IEnumerator> routine)
		{
			return ILib.Routine.IterationTask(behaviour, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// IHasResult[T]型を返すイテレータをコンストラクタで指定してください。
		/// 値が返されるたびにトリガーが実行されます。
		/// </summary>
		public static ILib.IterationTaskRoutine<T> IterationTaskRoutine<T>(this MonoBehaviour behaviour, IEnumerator routine)
		{
			return ILib.Routine.IterationTask<T>(behaviour, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// IHasResult[T]型を返すイテレータをコンストラクタで指定してください。
		/// 値が返されるたびにトリガーが実行されます。
		/// </summary>
		public static ILib.IterationTaskRoutine<T> IterationTaskRoutine<T>(this MonoBehaviour behaviour, Func<IEnumerator> routine)
		{
			return ILib.Routine.IterationTask<T>(behaviour, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// IHasResult[T]型を返すイテレータをコンストラクタで指定してください。
		/// 値が返されるたびにトリガーが実行されます。
		/// 完了後に再度実行が可能です。
		/// </summary>
		public static ILib.RepeatTaskRoutine<T> RepeatTaskRoutine<T>(this MonoBehaviour behaviour, Func<IEnumerator> routine)
		{
			return ILib.Routine.RepeatTask<T>(behaviour, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// 完了と例外を取得できます。
		/// </summary>
		public static ILib.Routine Routine(this IHasRoutineOwner self, IEnumerator routine)
		{
			return ILib.Routine.Start(self.RoutineOwner, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// 完了と例外を取得できます。
		/// 完了後に再度実行が可能です。
		/// </summary>
		public static ILib.RepeatRoutine RepeatRoutine(this IHasRoutineOwner self, Func<IEnumerator> routine)
		{
			return ILib.Routine.Repeat(self.RoutineOwner, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// IHasResult[T]型を返すイテレータをコンストラクタで指定してください。
		/// 値が返された時点で完了扱いとなります。
		/// </summary>
		public static ILib.TaskRoutine<T> TaskRoutine<T>(this IHasRoutineOwner self, IEnumerator routine)
		{
			return ILib.Routine.Task<T>(self.RoutineOwner, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// IHasResult型を返すイテレータをコンストラクタで指定してください。
		/// 値が返されるたびにトリガーが実行されます。
		/// </summary>
		public static ILib.IterationTaskRoutine IterationTaskRoutine(this IHasRoutineOwner self, IEnumerator routine)
		{
			return ILib.Routine.IterationTask(self.RoutineOwner, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// IHasResult型を返すイテレータをコンストラクタで指定してください。
		/// 値が返されるたびにトリガーが実行されます。
		/// </summary>
		public static ILib.IterationTaskRoutine IterationTaskRoutine(this IHasRoutineOwner self, Func<IEnumerator> routine)
		{
			return ILib.Routine.IterationTask(self.RoutineOwner, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// IHasResult[T]型を返すイテレータをコンストラクタで指定してください。
		/// 値が返されるたびにトリガーが実行されます。
		/// </summary>
		public static ILib.IterationTaskRoutine<T> IterationTaskRoutine<T>(this IHasRoutineOwner self, IEnumerator routine)
		{
			return ILib.Routine.IterationTask<T>(self.RoutineOwner, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// IHasResult[T]型を返すイテレータをコンストラクタで指定してください。
		/// 値が返されるたびにトリガーが実行されます。
		/// </summary>
		public static ILib.IterationTaskRoutine<T> IterationTaskRoutine<T>(this IHasRoutineOwner self, Func<IEnumerator> routine)
		{
			return ILib.Routine.IterationTask<T>(self.RoutineOwner, routine);
		}

		/// <summary>
		/// Unityのコルーチンのラッパーです。
		/// IHasResult[T]型を返すイテレータをコンストラクタで指定してください。
		/// 値が返されるたびにトリガーが実行されます。
		/// 完了後に再度実行が可能です。
		/// </summary>
		public static ILib.RepeatTaskRoutine<T> RepeatTaskRoutine<T>(this IHasRoutineOwner self, Func<IEnumerator> routine)
		{
			return ILib.Routine.RepeatTask<T>(self.RoutineOwner, routine);
		}

	}
}
