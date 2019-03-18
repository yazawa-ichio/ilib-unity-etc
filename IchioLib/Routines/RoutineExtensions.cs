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
		public static ILib.Routine Routine(this MonoBehaviour behaviour, IEnumerator routine)
		{
			return ILib.Routine.Start(behaviour, routine);
		}

		public static ILib.RepeatRoutine RepeatRoutine(this MonoBehaviour behaviour, Func<IEnumerator> routine)
		{
			return ILib.Routine.Repeat(behaviour, routine);
		}

		public static ILib.TaskRoutine<T> TaskRoutine<T>(this MonoBehaviour behaviour, IEnumerator routine)
		{
			return ILib.Routine.Task<T>(behaviour, routine);
		}

		public static ILib.IterationTaskRoutine IterationTaskRoutine(this MonoBehaviour behaviour, IEnumerator routine)
		{
			return ILib.Routine.IterationTask(behaviour, routine);
		}

		public static ILib.IterationTaskRoutine IterationTaskRoutine(this MonoBehaviour behaviour, Func<IEnumerator> routine)
		{
			return ILib.Routine.IterationTask(behaviour, routine);
		}

		public static ILib.IterationTaskRoutine<T> IterationTaskRoutine<T>(this MonoBehaviour behaviour, IEnumerator routine)
		{
			return ILib.Routine.IterationTask<T>(behaviour, routine);
		}

		public static ILib.IterationTaskRoutine<T> IterationTaskRoutine<T>(this MonoBehaviour behaviour, Func<IEnumerator> routine)
		{
			return ILib.Routine.IterationTask<T>(behaviour, routine);
		}

		public static ILib.RepeatTaskRoutine<T> RepeatTaskRoutine<T>(this MonoBehaviour behaviour, Func<IEnumerator> routine)
		{
			return ILib.Routine.RepeatTask<T>(behaviour, routine);
		}

		public static ILib.Routine Routine(this IHasRoutineOwner self, IEnumerator routine)
		{
			return ILib.Routine.Start(self.RoutineOwner, routine);
		}

		public static ILib.RepeatRoutine RepeatRoutine(this IHasRoutineOwner self, Func<IEnumerator> routine)
		{
			return ILib.Routine.Repeat(self.RoutineOwner, routine);
		}

		public static ILib.TaskRoutine<T> TaskRoutine<T>(this IHasRoutineOwner self, IEnumerator routine)
		{
			return ILib.Routine.Task<T>(self.RoutineOwner, routine);
		}

		public static ILib.IterationTaskRoutine IterationTaskRoutine(this IHasRoutineOwner self, IEnumerator routine)
		{
			return ILib.Routine.IterationTask(self.RoutineOwner, routine);
		}

		public static ILib.IterationTaskRoutine IterationTaskRoutine(this IHasRoutineOwner self, Func<IEnumerator> routine)
		{
			return ILib.Routine.IterationTask(self.RoutineOwner, routine);
		}

		public static ILib.IterationTaskRoutine<T> IterationTaskRoutine<T>(this IHasRoutineOwner self, IEnumerator routine)
		{
			return ILib.Routine.IterationTask<T>(self.RoutineOwner, routine);
		}

		public static ILib.IterationTaskRoutine<T> IterationTaskRoutine<T>(this IHasRoutineOwner self, Func<IEnumerator> routine)
		{
			return ILib.Routine.IterationTask<T>(self.RoutineOwner, routine);
		}

		public static ILib.RepeatTaskRoutine<T> RepeatTaskRoutine<T>(this IHasRoutineOwner self, Func<IEnumerator> routine)
		{
			return ILib.Routine.RepeatTask<T>(self.RoutineOwner, routine);
		}

	}
}
