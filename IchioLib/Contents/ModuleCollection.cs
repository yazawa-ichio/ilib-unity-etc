using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace ILib.Contents
{
	using Logger;

	/// <summary>
	/// モジュールのコレクションです。
	/// 親が指定されている場合、親のモジュールも実行されます。
	/// </summary>
	public class ModuleCollection : LockCollection<Module>
	{
		ModuleCollection m_Parent;
		ModuleType m_CollectionType = ModuleType.None;

		public ModuleType Type
		{
			get
			{
				if (m_Parent == null) return m_CollectionType;
				return m_Parent.Type | m_CollectionType;
			}
		}

		public ModuleCollection() { }

		public ModuleCollection(ModuleCollection parent)
		{
			m_Parent = parent;
		}

		void UpdateType()
		{
			ModuleType type = ModuleType.None;
			foreach (var module in this)
			{
				type |= module.Type;
			}
			m_CollectionType = type;
		}

		public void Add<T>() where T : Module, new()
		{
			Add(new T());
		}

		public void Remove<T>() where T : Module
		{
			var module = Get<T>();
			if (module != null) Remove(module);
		}

		public T Get<T>() where T : Module
		{
			return this.FirstOrDefault(x => x is T) as T;
		}

		public IEnumerable<T> GetModules<T>() where T : Module
		{
			return this.Select(x => x as T).Where(x => x != null);
		}

		public override void Add(Module child)
		{
			Log.Debug("[ilib-content]add module {0}", child);
			base.Add(child);
			UpdateType();
		}

		public override void Remove(Module child)
		{
			Log.Debug("[ilib-content]remove module {0}", child);
			base.Remove(child);
			UpdateType();
		}

		IEnumerator Iterate(ModuleType type, Func<Module, IEnumerator> func)
		{
			if (m_Parent != null)
			{
				var iterate = m_Parent.Iterate(type, func);
				while (iterate.MoveNext())
				{
					yield return iterate.Current;
				}
			}
			foreach (var module in this)
			{
				if (module.Type.HasFlag(type))
				{
					yield return func(module);
				}
			}
		}

		public IEnumerator OnPreBoot(Content content) => Iterate(ModuleType.PreBoot, x => x.OnPreBoot(content));

		public IEnumerator OnBoot(Content content) => Iterate(ModuleType.Boot, x => x.OnBoot(content));

		public IEnumerator OnPreShutdown(Content content) => Iterate(ModuleType.PreShutdown, x => x.OnPreShutdown(content));

		public IEnumerator OnShutdown(Content content) => Iterate(ModuleType.Shutdown, x => x.OnShutdown(content));

		public IEnumerator OnPreRun(Content content) => Iterate(ModuleType.PreRun, x => x.OnPreRun(content));

		public IEnumerator OnRun(Content content) => Iterate(ModuleType.Run, x => x.OnRun(content));

		public IEnumerator OnPreSuspend(Content content) => Iterate(ModuleType.PreSuspend, x => x.OnPreSuspend(content));

		public IEnumerator OnSuspend(Content content) => Iterate(ModuleType.Suspend, x => x.OnSuspend(content));

		public IEnumerator OnPreEnable(Content content) => Iterate(ModuleType.PreEnable, x => x.OnPreEnable(content));

		public IEnumerator OnEnable(Content content) => Iterate(ModuleType.Enable, x => x.OnEnable(content));

		public IEnumerator OnPreDisable(Content content) => Iterate(ModuleType.PreDisable, x => x.OnPreDisable(content));

		public IEnumerator OnDisable(Content content) => Iterate(ModuleType.Disable, x => x.OnDisable(content));

		public IEnumerator OnPreSwitch(Content prev, Content next) => Iterate(ModuleType.PreSwitch, x => x.OnPreSwitch(prev, next));

		public IEnumerator OnSwitch(Content prev, Content next) => Iterate(ModuleType.Switch, x => x.OnSwitch(prev, next));

		public IEnumerator OnEndSwitch(Content prev, Content next) => Iterate(ModuleType.EndSwitch, x => x.OnEndSwitch(prev, next));

	}

}
