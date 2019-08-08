using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ILib.Caller
{
	public class HandleAttribute : Attribute
	{
		public string Key { get; private set; }
		public HandleAttribute(object obj) { Key = Call.ToKey(obj); }
	}

	internal class HandleEntry
	{
		static Dictionary<Type, HandleEntry> s_Dic = new Dictionary<Type, HandleEntry>();

		public static HandleEntry Get(Type type)
		{
			HandleEntry entry = null;
			if (!s_Dic.TryGetValue(type, out entry))
			{
				s_Dic[type] = entry = new HandleEntry(type);
			}
			return entry;
		}

		public Dictionary<string, MethodInfo> Methods = new Dictionary<string, MethodInfo>();
		public Dictionary<string, ParameterInfo> Parameters = new Dictionary<string, ParameterInfo>();
		object[] m_Prm = new object[1];

		public HandleEntry(Type type)
		{
			var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			foreach (var method in methods)
			{
				foreach (var handle in method.GetCustomAttributes<HandleAttribute>(true))
				{
					this.Methods[handle.Key] = method;
					var prm = method.GetParameters();
					if (prm == null || prm.Length == 0)
					{
						Parameters[handle.Key] = null;
					}
					else if (prm.Length == 1)
					{
						Parameters[handle.Key] = prm[0];
					}
				}
			}
		}

		public bool Invoke(object instance, object key, object prm)
		{
			string name;
			if (key is string)
			{
				name = key.ToString();
			}
			else
			{
				name = key.GetType().FullName + "-" + key.ToString();
			}
			MethodInfo method = null;
			if (Methods.TryGetValue(name, out method))
			{
				if (Parameters[name] == null)
				{
					method.Invoke(instance, null);
				}
				else
				{
					m_Prm[0] = prm;
					method.Invoke(instance, m_Prm);
				}
				return true;
			}
			return false;
		}

	}

	/// <summary>
	/// Callに登録したオブジェクトのハンドルです。
	/// </summary>
	public class Handle : IDisposable
	{
		List<IPath> m_Paths = new List<IPath>();

		internal void Add(IPath path) => m_Paths.Add(path);

		public void Dispose()
		{
			foreach (var path in m_Paths)
			{
				path.Dispose();
			}
		}
	}

}
