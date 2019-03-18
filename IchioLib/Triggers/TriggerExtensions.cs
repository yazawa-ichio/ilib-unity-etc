using System;
using System.Collections;
using System.Collections.Generic;

namespace ILib
{

	public static class TriggerExtensions
	{
		public static ITriggerAction<T> Add<T>(this ITrigger<T> self, Action<T> action)
		{
			return self.Action.Add(action);
		}

		public static ITriggerAction<T> Add<T>(this ITrigger<T> self, Action<T, Exception> action)
		{
			return self.Action.Add(action);
		}

		public static ITriggerAction<T> Add<T>(this ITrigger<T> self, Trigger<T> trigger) => self.Action.Add(trigger);

		public static ITriggerAction<T> Add<T>(this ITriggerAction<T> self, Trigger<T> trigger)
		{
			self.Add(trigger.Fire);
			self.AddFail(trigger.Error);
			return self;
		}

		public static ITriggerAction<T> AddFail<T>(this ITrigger<T> self, Action<Exception> action)
		{
			return self.Action.AddFail(action);
		}

		public static IEnumerator Wait<T>(this ITrigger<T> self)
		{
			return self.Action.Wait();
		}

		public static ITriggerAction<U> Select<T, U>(this ITrigger<T> self, Func<T, U> func, bool oneShot = true)
		{
			return self.Action.Select(func, oneShot);
		}

		public static ITriggerAction<U> Select<T, U>(this ITriggerAction<T> self, Func<T, U> func, bool oneShot = true)
		{
			Trigger<U> trigger = new Trigger<U>(oneShot: self.OneShot);
			self.Add((item, ex) =>
			{
				if (ex != null)
				{
					trigger.Error(ex);
				}
				else
				{
					trigger.Fire(func(item));
				}
			});
			return trigger.Action;
		}

		public static ITriggerAction<T> Where<T>(this ITrigger<T> self, Func<T, bool> func)
		{
			return self.Action.Where(func);
		}

		public static ITriggerAction<T> Where<T>(this ITriggerAction<T> self, Func<T, bool> func)
		{
			Trigger<T> trigger = new Trigger<T>(oneShot: false);
			self.Add((item, ex) =>
			{
				if (ex != null)
				{
					trigger.Error(ex);
				}
				else if (func(item))
				{
					trigger.Fire(item);
				}
			});
			return trigger.Action;
		}

		public static ITriggerAction<bool> Any<T>(this ITrigger<T> self)
		{
			return self.Any();
		}

		public static ITriggerAction<bool> Any<T>(this ITriggerAction<T> self)
		{
			Trigger<bool> trigger = new Trigger<bool>(true);
			self.Add((item, ex) =>
			{
				if (ex != null)
				{
					trigger.Fire(true);
				}
			});
			return trigger.Action;
		}

		public static ITriggerAction<T> Then<T>(this ITrigger<ITriggerAction<T>> self)
		{
			return self.Action.Then();
		}

		public static ITriggerAction<T> Then<T>(this ITriggerAction<ITriggerAction<T>> self)
		{
			Trigger<T> trigger = new Trigger<T>(oneShot: self.OneShot);
			self.Add((item1, ex1) =>
			{
				if (ex1 != null)
				{
					trigger.Error(ex1);
				}
				else
				{
					item1.Add((item2, ex2) =>
					{
						if (ex2 != null)
						{
							trigger.Error(ex2);
						}
						else
						{
							trigger.Fire(item2);
						}
					});
				}
			});
			return trigger.Action;
		}

		public static ITriggerAction<T> Time<T>(this ITrigger<T> self, float time)
		{
			return self.Action.Time(time);
		}

		public static ITriggerAction<T> Time<T>(this ITriggerAction<T> self, float time)
		{
			Trigger<T> trigger = new Trigger<T>(oneShot: self.OneShot);
			self.Add((item, ex) =>
			{
				if (ex != null)
				{
					trigger.Error(ex);
				}
				else
				{
					AsyncTrigger.Time(time)
					.Add(x => trigger.Fire(item))
					.AddFail(trigger.Error);
				}
			});
			return trigger.Action;
		}

		public static ITriggerAction<T> Realtime<T>(this ITrigger<T> self, float time)
		{
			return self.Action.Realtime(time);
		}

		public static ITriggerAction<T> Realtime<T>(this ITriggerAction<T> self, float time)
		{
			Trigger<T> trigger = new Trigger<T>(oneShot: self.OneShot);
			self.Add((item, ex) =>
			{
				if (ex != null)
				{
					trigger.Error(ex);
				}
				else
				{
					AsyncTrigger.Realtime(time)
					.Add(x => trigger.Fire(item))
					.AddFail(trigger.Error);
				}
			});
			return trigger.Action;
		}

		public static ITriggerAction<T> Timeout<T>(this ITriggerAction<T> self, float time, bool realtime = false)
		{
			Trigger<T> trigger = new Trigger<T>(oneShot: self.OneShot);
			self.Add(trigger);
			var timeout = realtime ? AsyncTrigger.Realtime(time) : AsyncTrigger.Time(time);
			timeout.Add(ret =>
			{
				if (!trigger.Action.Fired)
				{
					trigger.Error(new TimeoutException("trigger timeout"));
				}
			});
			return trigger.Action;
		}


	}

}
