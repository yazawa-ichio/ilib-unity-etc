namespace ILib.UI
{
	/// <summary>
	/// UIStackのリクエストです。
	/// Pushの完了を受け取れます。
	/// </summary>
	public interface IStackEntry : ITriggerAction
	{
		bool IsActive { get; }
		bool IsFornt { get; }
		ITriggerAction Pop();
		void Execute<T>(System.Action<T> action, bool immediate = false);
	}
}
