using System;
using System.Collections;

namespace ILib.Triggers
{
	public class SuccessedAction<T> : ITriggerAction<T>
	{
		public T Result { get; private set; }

		public bool Fired => true;

		public Exception Error => null;

		public bool Canceled => false;

		public bool OneShot => true;

		public event Action<T> OnFire
		{
			add { Add(value); }
			remove { }
		}

		public event Action<Exception> OnFail { add { } remove { } }

		public event Action OnCancel { add { } remove { } }

		public event Action<bool> OnComplete { add { AddComplete(value); } remove { } }

		public SuccessedAction(T val)
		{
			Result = val;
		}

		public ITriggerAction Add(Action action)
		{
			action?.Invoke();
			return this;
		}

		public ITriggerAction<T> Add(Action<T> action)
		{
			action?.Invoke(Result);
			return this;
		}

		public ITriggerAction<T> Add(Action<T, Exception> action)
		{
			action(Result, null);
			return this;
		}

		public ITriggerAction<T> AddComplete(Action<bool> action)
		{
			action?.Invoke(true);
			return this;
		}

		public ITriggerAction<T> AddFail(Action<Exception> action)
		{
			return this;
		}

		public void AddStrictCompleteObserve(Action action)
		{
			action?.Invoke();
		}

		public void Cancel() { }

		public void Clear() { }

		public void Dispose() { }

		public ITriggerAction<T> Remove(Action<T> action) { return this; }

		public ITriggerAction<T> RemoveComplete(Action<bool> action) { return this; }

		public ITriggerAction<T> RemoveFail(Action<Exception> action) { return this; }

		object IEnumerator.Current => null;

		bool IEnumerator.MoveNext() => false;

		void IEnumerator.Reset() { }

	}
}
