
namespace ILib.Caller
{

	public interface IDispatcher
	{
		bool Message(object key);
		bool Message<T>(object key, T prm);
		bool Broadcast(object key);
		bool Broadcast<T>(object key, T prm);
	}

	public class Dispatcher : IDispatcher
	{
		Call m_Call;
		public Dispatcher(Call call) => m_Call = call;
		public bool Broadcast(object key) => m_Call.Broadcast(key);
		public bool Broadcast<T>(object key, T prm) => m_Call.Broadcast(key, prm);
		public bool Message(object key) => m_Call.Message(key);
		public bool Message<T>(object key, T prm) => m_Call.Message(key, prm);
	}

	public interface IHasDispatcher
	{
		IDispatcher Dispatcher { get; }
	}

	public static class DispatcherExtensions
	{
		public static bool Message(this IHasDispatcher self, object key) => self.Dispatcher.Message(key);
		public static bool Message<T>(this IHasDispatcher self, object key, T prm) => self.Dispatcher.Message(key, prm);
		public static bool Broadcast(this IHasDispatcher self, object key) => self.Dispatcher.Broadcast(key);
		public static bool Broadcast<T>(this IHasDispatcher self, object key, T prm) => self.Dispatcher.Broadcast(key, prm);
	}

}
