﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.UI
{
	/// <summary>
	/// アニメーションの遷移を行います。
	/// </summary>
	public interface ITransition
	{
		/// <summary>
		/// UIのOnCreated実行前に実行されます。
		/// 初期表示をHide状態にするなどに利用します。
		/// </summary>
		void OnPreCreated();
		/// <summary>
		/// UIの表示アニメーションです。
		/// </summary>
		ITriggerAction Show(bool open);
		/// <summary>
		/// UIの非表示アニメーションです。
		/// </summary>
		ITriggerAction Hide(bool close);
	}

}
