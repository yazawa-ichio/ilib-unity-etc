﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using ILib.Triggers;

namespace ILib
{
	using Logger;
	using System.Threading.Tasks;

	public partial class Trigger
	{
		public static readonly ITriggerAction<bool> Successed = new SuccessedAction<bool>(true);

		public static ITriggerAction<T> Success<T>(T val) => new SuccessedAction<T>(val);

		public static IEnumerator Wait(params ITriggerAction[] actions)
		{
			foreach (var action in actions)
			{
				while (action.Fired) yield return null;
			}
		}

		public static IEnumerator Wait(IEnumerable<ITriggerAction> actions)
		{
			foreach (var action in actions)
			{
				while (action.Fired) yield return null;
			}
		}

		public static ITriggerAction Combine(params ITriggerAction[] actions)
		{
			Trigger trigger = new Trigger();
			if (actions == null || actions.Length == 0)
			{
				trigger.Fire();
				return trigger.Action;
			}
			int count = 0;
			Action onSuccess = () =>
			{
				count++;
				if (count == actions.Length)
				{
					trigger.Fire();
				}
			};
			foreach (var action in actions)
			{
				action.Add(onSuccess);
				action.OnFail += trigger.Error;
			}
			return trigger.Action;
		}


		public static ITriggerAction<T[]> Combine<T>(params ITriggerAction<T>[] actions)
		{
			Trigger<T[]> trigger = new Trigger<T[]>();
			T[] ret = new T[actions.Length];
			if (actions == null || actions.Length == 0)
			{
				trigger.Fire(ret);
				return trigger.Action;
			}
			int count = 0;
			for (int i = 0; i < actions.Length; i++)
			{
				int index = i;
				ITriggerAction<T> action = actions[i];
				action.Add((x) =>
				{
					ret[index] = x;
					count++;
					if (count == ret.Length)
					{
						trigger.Fire(ret);
					}
				});
				action.OnFail += trigger.Error;
			}
			return trigger.Action;
		}

		public static ITriggerAction<T[]> Parallel<T>(IEnumerable<Func<ITriggerAction<T>>> actions)
		{
			Trigger<T[]> trigger = new Trigger<T[]>();
			var _actions = actions.Select(x => x()).ToArray();
			var ret = new T[_actions.Length];
			var count = 0;
			for (int i = 0; i < _actions.Length; i++)
			{
				var index = i;
				_actions[i]
					.AddFail(x => trigger.Error(x))
					.Add(x =>
					{
						ret[index] = x;
						count++;
						if (count == _actions.Length)
						{
							trigger.Fire(ret);
						}
					});
			}
			return trigger.Action;
		}

		public static ITriggerAction<bool> Parallel(IEnumerable<Func<ITriggerAction>> actions)
		{
			Trigger<bool> trigger = new Trigger<bool>();
			var _actions = actions.Select(x => x()).ToArray();
			var count = 0;
			for (int i = 0; i < _actions.Length; i++)
			{
				var index = i;
				_actions[i].OnFail += trigger.Error;
				_actions[i].Add(() =>
				{
					count++;
					if (count == _actions.Length)
					{
						trigger.Fire(true);
					}
				});
			}
			return trigger.Action;
		}

		public static ITriggerAction<T[]> Sequential<T>(IEnumerable<Func<ITriggerAction<T>>> actions)
		{
			Trigger<T[]> trigger = new Trigger<T[]>();
			var _actions = actions.ToArray();
			T[] ret = new T[_actions.Length];
			int index = 0;
			Action<T> onSuccess = null;
			onSuccess = (x) =>
			{
				ret[index++] = x;
				if (index == _actions.Length)
				{
					trigger.Fire(ret);
					return;
				}
				var action = _actions[index]();
				action.Add(onSuccess);
				action.AddFail(trigger.Error);
			};
			{
				var action = _actions[0]();
				action.Add(onSuccess);
				action.AddFail(trigger.Error);
			}
			return trigger.Action;
		}

		public static ITriggerAction<bool> Sequential(IEnumerable<Func<ITriggerAction>> actions)
		{
			Trigger<bool> trigger = new Trigger<bool>();
			var _actions = actions.ToArray();
			int index = 0;
			Action onSuccess = null;
			onSuccess = () =>
			{
				index++;
				if (index == _actions.Length)
				{
					trigger.Fire(true);
					return;
				}
				var action = _actions[index]();
				action.Add(onSuccess);
				action.OnFail += trigger.Error;
			};
			{
				var action = _actions[0]();
				action.Add(onSuccess);
				action.OnFail += trigger.Error;
			}
			return trigger.Action;
		}

		public static ITriggerAction<bool> Time(float time)
		{
			return TriggerBehaviour.Time(time);
		}

		public static ITriggerAction<bool> Realtime(float time)
		{
			return TriggerBehaviour.Realtime(time);
		}

		public static ITriggerAction From(Task task)
		{
			var trigger = new Trigger();
			FromImpl(trigger, task);
			return trigger.Action;
		}

		public static ITriggerAction<T> From<T>(Task<T> task)
		{
			var trigger = new Trigger<T>();
			FromImpl(trigger, task);
			return trigger.Action;
		}

		internal static async void FromImpl(Trigger trigger, Task task)
		{
			try
			{
				if (task != null)
				{
					await task;
				}
				trigger.Fire();
			}
			catch (Exception ex)
			{
				Log.Warning("[ilib-trigger] from task error:{0}", ex);
				trigger.Error(ex);
			}
		}

		internal static async void FromImpl<T>(Trigger<T> trigger, Task<T> task)
		{
			try
			{
				trigger.Fire(await task);
			}
			catch (Exception ex)
			{
				Log.Warning("[ilib-trigger] from task({0}) error:{1}", typeof(T), ex);
				trigger.Error(ex);
			}
		}

	}
}