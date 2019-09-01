using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ILib.UI
{

	/// <summary>
	/// UIの表示をスタック制御で行います。
	/// </summary>
	public class UIStack : UIStack<object, IControl> { }

	/// <summary>
	/// UIの表示をスタック制御で行います。
	/// </summary>
	public abstract class UIStack<TParam, UControl> : UIController<TParam, UControl> , IStackController where UControl : class, IControl
	{

		List<UIInstance> m_Stack = new List<UIInstance>();

		protected override IEnumerable<T> GetActive<T>()
		{
			for (int i = m_Stack.Count - 1; i >= 0; i--)
			{
				var ui = m_Stack[i].Control;
				if (ui.IsActive && ui is T target)
				{
					yield return target;
				}
			}
		}

		bool IStackController.IsFornt(StackEntry entry)
		{
			if (m_Stack.Count == 0 || entry.Instance == null) return false;
			var first = m_Stack[m_Stack.Count - 1];
			return entry.Instance == first;
		}

		public IStackEntry Push(string path, TParam prm)
		{
			var entry = new StackEntry(this);
			EnqueueProcess(() => PushImpl(path, prm, entry));
			return entry;
		}

		public IStackEntry Switch(string path, TParam prm)
		{
			var entry = new StackEntry(this);
			EnqueueProcess(() => ChangeImpl(path, prm, entry));
			return entry;
		}

		protected override ITriggerAction Close(IControl control)
		{
			var trigger = new Trigger();
			EnqueueProcess(() =>
			{
				var index = m_Stack.FindIndex(x => control == x.Control);
				if (index >= 0)
				{
					PopImpl(null, m_Stack.Count - index, trigger);
				}
			});
			return trigger.Action;
		}

		public ITriggerAction Pop(int count = 1)
		{
			var trigger = new Trigger();
			EnqueueProcess(() => PopImpl(null, count, trigger));
			return trigger.Action;
		}

		ITriggerAction IStackController.Pop(StackEntry entry)
		{
			var trigger = new Trigger();
			EnqueueProcess(() => PopImpl(entry.Instance, 0, trigger));
			return trigger.Action;
		}

		void PushImpl(string path, TParam prm, StackEntry entry) 
		{
			var parent = m_Stack.Count > 0 ? m_Stack[m_Stack.Count - 1] : null;
			var trigger = Open<UControl>(path, prm, parent);
			trigger.Add((x, ex) =>
			{
				if (ex == null)
				{
					m_Stack.Add(x);
					entry.Instance = x;
					entry.Fire(true, null);
				}
				else
				{
					entry.Fire(false, ex);
				}
			});
		}

		void ChangeImpl(string path, TParam prm, StackEntry entry)
		{
			var release = m_Stack.Count > 0 ? m_Stack[m_Stack.Count - 1] : null;
			var parent = default(UIInstance);
			UIInstance[] releases = Array.Empty<UIInstance>();
			if (release != null)
			{
				m_Stack.RemoveAt(m_Stack.Count - 1);
				releases = new UIInstance[] { release };
				parent = release.Parent;
			}
			var trigger = Change<UControl>(path, prm, parent, releases);
			trigger.Add((x, ex) =>
			{
				if (ex == null)
				{
					m_Stack.Add(x);
					entry.Instance = x;
					entry.Fire(true, null);
				}
				else
				{
					entry.Fire(false, ex);
				}
			});
		}

		void PopImpl(UIInstance instance, int count, Trigger trigger)
		{
			if (m_Stack.Count == 0)
			{
				Debug.Assert(false, "スタックが空です");
				trigger.Fire();
				return;
			}
			UIInstance front = null;
			UIInstance[] releases = null;
			int index = 0;
			if (instance != null)
			{
				index = m_Stack.IndexOf(instance);
				if (index < 0)
				{
					trigger.Error(new Exception("指定のUIが見つかりませんでした"));
					return;
				}
			}
			else
			{
				index = m_Stack.Count - count;
				if (index < 0) index = 0;
				instance = m_Stack[index];
			}
			releases = m_Stack.Skip(index).ToArray();
			m_Stack.RemoveRange(index, m_Stack.Count - index);
			
			front = instance.Parent;

			Close(releases, front).Add((x, ex) =>
			{
				if (ex != null)
				{
					trigger.Error(ex);
				}
				else
				{
					trigger.Fire();
				}
			});
		}

	}

}
