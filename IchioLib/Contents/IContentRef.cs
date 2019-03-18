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
		IRoutine<bool> Resume();
		IRoutine<bool> Suspend();
		IRoutine<bool> Shutdown();
		IRoutine<IContentRef> Switch(IContentParam prm);
		IRoutine<IContentRef> Switch<T>();
		IRoutine<IContentRef> Switch(Type type, object prm = null);
	}

}
