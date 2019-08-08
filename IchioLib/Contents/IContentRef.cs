using System.Collections;
using System.Collections.Generic;

namespace ILib.Contents
{
	using Caller;
	using System;

	/// <summary>
	/// コンテンツの公開メソッド
	/// </summary>
	public interface IContentRef : IHasDispatcher
	{
		ITriggerAction<bool> Resume();
		ITriggerAction<bool> Suspend();
		ITriggerAction<bool> Shutdown();
		ITriggerAction<IContentRef> Switch(IContentParam prm);
		ITriggerAction<IContentRef> Switch<T>();
		ITriggerAction<IContentRef> Switch(Type type, object prm = null);
	}

}
