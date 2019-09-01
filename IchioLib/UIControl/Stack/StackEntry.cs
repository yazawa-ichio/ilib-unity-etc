namespace ILib.UI
{
	internal class StackEntry : TriggerAction<bool>, IStackEntry
	{
		public UIInstance Instance;
		IStackController m_Parent;
		ITriggerAction m_Pop;

		public StackEntry(IStackController parent) => m_Parent = parent;

		public bool IsActive => Instance?.Control.IsActive ?? false;

		public bool IsFornt => m_Parent.IsFornt(this);

		public ITriggerAction Pop()
		{
			if (m_Pop != null) return m_Pop;
			if (Fired)
			{
				return m_Pop = m_Parent.Pop(this);
			}
			else
			{
				return m_Pop = this.Then(x => m_Parent.Pop(this));
			}
		}

		public void Execute<T>(System.Action<T> action, bool immediate = false)
		{
			if (immediate && Instance == null) return;
			if (Instance != null)
			{
				if (Instance.Control is T target)
				{
					action(target);
				}
			}
			else
			{
				Add(() =>
				{
					if (Instance != null && Instance.Control is T target)
					{
						action(target);
					}
				});
			}
		}
	}

}
