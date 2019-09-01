namespace ILib.UI
{
	/// <summary>
	/// UIQueueのリクエストです。
	/// </summary>
	public interface IQueueEntry : System.IDisposable
	{
		bool IsClosed { get; }
		ITriggerAction ObserveClosed { get; }
		ITriggerAction<bool> Close();
	}
}
