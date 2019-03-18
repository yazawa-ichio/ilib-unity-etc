using System;
using System.Collections;
using System.Collections.Generic;

namespace ILib.Caller
{

	public class Call : IDisposable, IDispatcher
	{
		public static string ToKey(object key)
		{
			if (key is string)
			{
				return key.ToString();
			}
			else
			{
				return key.GetType().FullName + "-" + key.ToString();
			}
		}

		Call m_Parent;
		List<Call> m_Calls;
		List<PathBase> m_Paths = new List<PathBase>();
		bool m_Getting = false;
		List<PathBase> m_Removes = new List<PathBase>();
		bool m_Enabled = true;
		int m_Priority = 0;
		object m_Locker = new object();

		public int Priority { get { return m_Priority; } set { m_Priority = value; m_Parent?.SortCall(); } }
		public bool Enabled { get { return m_Enabled && !Disposed; } set { m_Enabled = value; } }
		public bool Disposed { get; private set; }

		~Call()
		{
			Dispose();
		}

		public Call SubCall()
		{
			Call call = new Call();
			lock (m_Locker)
			{
				call.m_Parent = this;
				if (m_Calls == null)
				{
					//優先度を処理するため自身をリストに含める
					m_Calls = new List<Call>(4);
					m_Calls.Add(this);
				}
				m_Calls.Add(call);
			}
			SortCall();
			return call;
		}

		void Remove(Call call)
		{
			lock (m_Locker)
			{
				m_Calls.Remove(call);
			}
		}

		void SortCall()
		{
			m_Calls.Sort((x1, x2) => x2.Priority - x1.Priority);
		}

		public void Dispose()
		{
			m_Parent?.Remove(this);
			m_Paths?.Clear();
			if (m_Calls == null) return;
			lock (m_Locker)
			{
				foreach (var call in m_Calls.ToArray())
				{
					if (call != this) call.Dispose();
				}
				m_Calls.Clear();
			}
		}

		public IPath Path(object key, Action action)
		{
			var path = new Path(this, ToKey(key), () =>
			{
				action();
				return true;
			});
			m_Paths.Add(path);
			return path;
		}

		public IPath Path<T>(object key, Action<T> action)
		{
			var path = new Path<T>(this, ToKey(key), (item) =>
			{
				action(item);
				return true;
			});
			m_Paths.Add(path);
			return path;
		}

		public ITriggerAction<T> Trigger<T>(object key)
		{
			if (Disposed) return TriggerAction<T>.Empty;
			Trigger<T> trigger = new Trigger<T>(oneShot: false);
			var path = new Path<T>(this, ToKey(key), (item) =>
			{
				trigger.Fire(item);
				return true;
			});
			m_Paths.Add(path);
			trigger.Action.OnCancel += path.Dispose;
			return trigger.Action;
		}

		public ITriggerAction<bool> Trigger(object key)
		{
			if (Disposed) return TriggerAction<bool>.Empty;
			Trigger<bool> trigger = new Trigger<bool>(oneShot: false);
			var path = new Path(this, ToKey(key), () =>
			{
				trigger.Fire(true);
				return true;
			});
			m_Paths.Add(path);
			trigger.Action.OnCancel += path.Dispose;
			return trigger.Action;
		}

		public Handle Bind(object handler)
		{
			if (Disposed) return new Handle();
			Handle handle = new Handle();
			var entry = HandleEntry.Get(handler.GetType());
			foreach (var kvp in entry.Methods)
			{
				var key = kvp.Key;
				var method = kvp.Value;
				var parameter = entry.Parameters[key];
				PathBase path = null;
				if (parameter == null)
				{
					path = new Path(this, key, () => entry.Invoke(handler, key, null));
				}
				else
				{
					path = new HandlePath(this, key, (obj) => entry.Invoke(handler, key, obj), parameter.ParameterType);
				}
				handle.Add(path);
				m_Paths.Add(path);
			}
			return handle;
		}

		public bool Message(object key) => Disposed ? false : Message(ToKey(key));

		public bool Message(string key)
		{
			if (Disposed) return false;
			if (m_Calls != null)
			{
				foreach (var call in m_Calls)
				{
					bool ret = (call == this) ? MessageImpl(key) : call.Message(key);
					if (ret) return true;
				}
				return false;
			}
			else {
				return MessageImpl(key);
			}
		}

		bool MessageImpl(string key)
		{
			if (!Enabled) return false;
			foreach (var path in Get(key, null))
			{
				if (path.Invoke(null))
				{
					return true;
				}
			}
			return false;
		}

		public bool Message<T>(object key, T prm) => Disposed ? false : Message(ToKey(key), prm);

		public bool Message<T>(string key, T prm)
		{
			if (Disposed) return false;
			if (m_Calls != null)
			{
				foreach (var call in m_Calls)
				{
					bool ret = (call == this) ? MessageImpl(key, prm) : call.Message(key, prm);
					if (ret) return true;
				}
				return false;
			}
			else
			{
				return MessageImpl(key, prm);
			}
		}

		bool MessageImpl<T>(string key, T prm)
		{
			if (!Enabled) return false;
			foreach (var path in Get(key, typeof(T)))
			{
				if (path.Invoke(prm))
				{
					return true;
				}
			}
			return false;
		}

		public bool Broadcast(object key) => Disposed ? false : Broadcast(ToKey(key));

		public bool Broadcast(string key)
		{
			if (Disposed) return false;
			if (m_Calls != null)
			{
				bool ret = false;
				foreach (var call in m_Calls)
				{
					ret |= (call == this) ? BroadcastImpl(key) : call.Broadcast(key);
				}
				return ret;
			}
			else
			{
				return BroadcastImpl(key);
			}
		}

		bool BroadcastImpl(string key)
		{
			if (!Enabled) return false;
			bool ret = false;
			foreach (var path in Get(key, null))
			{
				ret |= path.Invoke(null);
			}
			return ret;
		}

		public bool Broadcast<T>(object key, T prm) => Disposed ? false : Broadcast(ToKey(key), prm);

		public bool Broadcast<T>(string key, T prm)
		{
			if (Disposed) return false;
			if (m_Calls != null)
			{
				bool ret = false;
				foreach (var call in m_Calls)
				{
					ret |= (call == this) ? BroadcastImpl(key, prm) : call.Broadcast(key, prm);
				}
				return ret;
			}
			else
			{
				return BroadcastImpl(key, prm);
			}
		}

		bool BroadcastImpl<T>(string key, T prm)
		{
			if (!Enabled) return false;
			bool ret = false;
			foreach (var path in Get(key, typeof(T)))
			{
				ret |= path.Invoke(prm);
			}
			return ret;
		}


		IEnumerable<PathBase> Get(string key, Type type)
		{
			m_Getting = true;
			for (int i = 0; i < m_Paths.Count; i++)
			{
				PathBase path = m_Paths[i];
				if (m_Removes.Contains(path)) continue;
				if (path.Key == key && path.Type == type)
				{
					yield return path;
				}
			}
			if (m_Removes.Count > 0)
			{
				foreach (var remove in m_Removes)
				{
					m_Paths.Remove(remove);
				}
				m_Removes.Clear();
			}
			m_Getting = false;
		}

		internal void Remove(PathBase path)
		{
			if (m_Getting)
			{
				m_Removes.Add(path);
			}
			else
			{
				m_Paths.Remove(path);
			}
		}

	}

}
