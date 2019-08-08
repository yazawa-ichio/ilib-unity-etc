namespace ILib.Contents
{
	using Caller;
	using System;


	public abstract partial class Content
	{
		internal class Ref : IContentRef
		{
			Content m_content;
			public Ref(Content content) => m_content = content;
			public ITriggerAction<bool> Resume() => m_content.Resume();
			public ITriggerAction<bool> Suspend() => m_content.Suspend();
			public ITriggerAction<bool> Shutdown() => m_content.Shutdown();
			public ITriggerAction<IContentRef> Switch(IContentParam prm) => m_content.Switch(prm);
			public ITriggerAction<IContentRef> Switch<T>() => m_content.Switch<T>();
			public ITriggerAction<IContentRef> Switch(Type type, object prm = null) => m_content.Switch(type, prm);
			IDispatcher IHasDispatcher.Dispatcher => m_content.Dispatcher;
		}

	}
}
