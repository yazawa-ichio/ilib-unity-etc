﻿using System.Collections;
using System.Collections.Generic;

namespace ILib.UI
{
	/// <summary>
	/// UIの表示制御
	/// </summary>
	public interface IControl
	{
		/// <summary>
		/// 有効化？
		/// </summary>
		bool IsActive { get; }
		/// <summary>
		/// Behind時に非アクティブにするか？
		/// </summary>
		bool IsDeactivateInBehind { get; }
		void SetController(IController controller);
		/// <summary>
		/// UI作成直後に実行されます
		/// </summary>
		ITriggerAction OnCreated(object prm);
		/// <summary>
		/// UIが最前面に来た際に実行されます。
		/// </summary>
		ITriggerAction OnFront(bool open);
		/// <summary>
		/// UIが最前面から後ろに回った際に実行されます。
		/// </summary>
		ITriggerAction OnBehind();
		/// <summary>
		/// UIを閉じる際に実行されます。
		/// </summary>
		ITriggerAction OnClose();
	}

	public interface IControl<TParam> : IControl
	{
		//ITriggerAction OnCreated(TParam prm);
	}
}
