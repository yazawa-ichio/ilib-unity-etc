using System;
using System.Collections;

namespace ILib
{

	public interface ITriggerAction :  IDisposable, IEnumerator
	{
		/// <summary>
		/// 失敗時のコールバックです。
		/// 内部的にAddFail・RemoveFail関数が実行されます
		/// </summary>
		event Action<Exception> OnFail;
		/// <summary>
		/// キャンセル時のコールバックです
		/// </summary>
		event Action OnCancel;
		/// <summary>
		/// 完了時のコールバックです。
		/// 発火時はtrue、キャンセル時はfalseが返ります。
		/// </summary>
		event Action<bool> OnComplete;
		/// <summary>
		/// 発火済みか？
		/// </summary>
		bool Fired { get; }
		/// <summary>
		/// 失敗時の値です。
		/// </summary>
		Exception Error { get; }
		/// <summary>
		/// キャンセル済みか？
		/// </summary>
		bool Canceled { get; }
		/// <summary>
		/// 一度だけ発火するか？
		/// </summary>
		bool OneShot { get; }
		/// <summary>
		/// 成功時のコールバックを追加します。
		/// </summary>
		ITriggerAction Add(Action action);
		/// <summary>
		/// イベントをすべて解除します。
		/// 発火前のみ有効です。
		/// </summary>
		void Clear();
		/// <summary>
		/// キャンセルを行います。
		/// 以後、イベントの発火は出来なくなります。
		/// </summary>
		void Cancel();
	}

	public interface ITriggerAction<T> : ITriggerAction
	{
		T Result { get; }
		/// <summary>
		/// 発火時のコールバックです。
		/// 内部的にAddとRemoveが実行されます。
		/// </summary>
		event Action<T> OnFire;
		/// <summary>
		/// 成功時のコールバックを追加します。
		/// </summary>
		ITriggerAction<T> Add(Action<T> action);
		/// <summary>
		/// 成功時のコールバックを解除します。
		/// </summary>
		ITriggerAction<T> Remove(Action<T> action);
		/// <summary>
		/// 成功と失敗のコールバックを実装します。
		/// Clear関数以外で解除はできません。
		/// </summary>
		ITriggerAction<T> Add(Action<T, Exception> action);
		/// <summary>
		/// 失敗時のコールバックを追加します。
		/// </summary>
		ITriggerAction<T> AddFail(Action<Exception> action);
		/// <summary>
		/// 失敗時のコールバックを解除します。
		/// </summary>
		ITriggerAction<T> RemoveFail(Action<Exception> action);
		/// <summary>
		/// 完了時のコールバックを追加します。
		/// キャンセル時はfalseが返ります。
		/// </summary>
		ITriggerAction<T> AddComplete(Action<bool> action);
		/// <summary>
		/// 完了時のコールバックを解除します。
		/// </summary>
		ITriggerAction<T> RemoveComplete(Action<bool> action);
	}
}
