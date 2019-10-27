using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;


namespace ILib
{

	public static class TriggerExtensions
	{
		public static ITriggerAction<T> Add<T>(this IHasTriggerAction<T> self, Action<T> action)
		{
			return self.Action.Add(action);
		}

		public static ITriggerAction<T> Add<T>(this IHasTriggerAction<T> self, Action<T, Exception> action)
		{
			return self.Action.Add(action);
		}

		public static ITriggerAction<T> Add<T>(this IHasTriggerAction<T> self, Trigger<T> trigger) => self.Action.Add(trigger);

		public static ITriggerAction<T> Add<T>(this ITriggerAction<T> self, Trigger<T> trigger)
		{
			self.Add(trigger.Fire);
			self.AddFail(trigger.Error);
			return self;
		}

		public static ITriggerAction<T> AddFail<T>(this IHasTriggerAction<T> self, Action<Exception> action)
		{
			return self.Action.AddFail(action);
		}

		public static IEnumerator Wait<T>(this IHasTriggerAction<T> self)
		{
			return self.Action;
		}

		public static ITriggerAction<T> Select<T>(this ITriggerAction self, Func<T> func, bool oneShot = true)
		{
			Trigger<T> trigger = new Trigger<T>(oneShot: self.OneShot);
			self.OnFail += trigger.Error;
			self.Add(() => trigger.Fire(func()));
			return trigger.Action;
		}

		public static ITriggerAction<U> Select<T, U>(this IHasTriggerAction<T> self, Func<T, U> func, bool oneShot = true)
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

		public static ITriggerAction<T> Where<T>(this IHasTriggerAction<T> self, Func<T, bool> func)
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

		public static ITriggerAction<bool> Any<T>(this IHasTriggerAction<T> self)
		{
			return self.Action.Any();
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

		public static ITriggerAction<T> Then<T>(this IHasTriggerAction<ITriggerAction<T>> self)
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

		public static ITriggerAction Then(this IHasTriggerAction<ITriggerAction> self)
		{
			return self.Action.Then();
		}

		public static ITriggerAction Then(this ITriggerAction<ITriggerAction> self)
		{
			Trigger trigger = new Trigger(oneShot: self.OneShot);
			self.Add((item1, ex1) =>
			{
				if (ex1 != null)
				{
					trigger.Error(ex1);
				}
				else
				{
					item1.Add(trigger.Fire);
					item1.OnFail += trigger.Error;
				}
			});
			return trigger.Action;
		}

		public static ITriggerAction<U> Then<T, U>(this IHasTriggerAction<T> self, Func<T, ITriggerAction<U>> func, bool oneShot = true)
		{
			return self.Select(func, oneShot: oneShot).Then();
		}

		public static ITriggerAction<U> Then<T, U>(this ITriggerAction<T> self, Func<T, ITriggerAction<U>> func, bool oneShot = true)
		{
			return self.Select(func, oneShot: oneShot).Then();
		}

		public static ITriggerAction Then<T>(this IHasTriggerAction<T> self, Func<T, ITriggerAction> func, bool oneShot = true)
		{
			return self.Select(func, oneShot: oneShot).Then();
		}

		public static ITriggerAction Then<T>(this ITriggerAction<T> self, Func<T, ITriggerAction> func, bool oneShot = true)
		{
			return self.Select(func, oneShot: oneShot).Then();
		}

		public static ITriggerAction<T> Time<T>(this IHasTriggerAction<T> self, float time)
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
					TriggerBehaviour.Time(time)
					.Add(x => trigger.Fire(item))
					.AddFail(trigger.Error);
				}
			});
			return trigger.Action;
		}

		public static ITriggerAction<T> Realtime<T>(this IHasTriggerAction<T> self, float time)
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
					TriggerBehaviour.Realtime(time)
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
			var timeout = realtime ? TriggerBehaviour.Realtime(time) : TriggerBehaviour.Time(time);
			timeout.Add(ret =>
			{
				if (!trigger.Action.Fired)
				{
					trigger.Error(new TimeoutException("trigger timeout"));
				}
			});
			return trigger.Action;
		}

		public static TriggerActionAwaiter GetAwaiter(this IHasTriggerAction self)
		{
			return new TriggerActionAwaiter(self);
		}

		public static TriggerActionAwaiter GetAwaiter(this ITriggerAction self)
		{
			return new TriggerActionAwaiter(self);
		}

		public static TriggerActionAwaiter<T> GetAwaiter<T>(this IHasTriggerAction<T> self)
		{
			return self.Action.GetAwaiter();
		}

		public static TriggerActionAwaiter<T> GetAwaiter<T>(this IHasTriggerAction<T> self, CancellationToken cancellation)
		{
			cancellation.Register(() => { self.Action.Cancel(); });
			return self.Action.GetAwaiter();
		}

		public static TriggerActionAwaiter<T> GetAwaiter<T>(this ITriggerAction<T> self, CancellationToken cancellation)
		{
			cancellation.Register(() => { self.Cancel(); });
			return self.GetAwaiter();
		}

		public static ITriggerAction ToTrigger(this Task self)
		{
			return Trigger.From(self);
		}

		public static ITriggerAction<T> ToTrigger<T>(this Task<T> self)
		{
			return Trigger.From(self);
		}


	}

}
