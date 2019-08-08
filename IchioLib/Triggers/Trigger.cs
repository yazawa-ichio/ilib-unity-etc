using ILib.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ILib
{
	public partial class Trigger : IHasTriggerAction<bool>
	{
		TriggerAction<bool> m_Action;
		public ITriggerAction<bool> Action => m_Action;

		public bool Fired => m_Action.Fired;

		public Trigger(bool oneShot = true)
		{
			m_Action = new TriggerAction<bool>(oneShot);
		}

		public void Fire()
		{
			m_Action.Fire(true, null);
		}

		public void Error(Exception ex)
		{
			m_Action.Fire(default, ex);
		}

		public void Dispose() => m_Action?.Dispose();

	}

	/// <summary>
	/// イベントの発火を制御するクラスです。
	/// イベントは一度のみ実行と、連続して実行のどちらかを選択して生成します。
	/// 発火時の処理はアクションを通して実装します。
	/// </summary>
	public class Trigger<T> : IHasTriggerAction<T>
	{

		TriggerAction<T> m_Action;
		public ITriggerAction<T> Action => m_Action;

		public bool Fired => m_Action.Fired;

		public Trigger(bool oneShot = true)
		{
			m_Action = new TriggerAction<T>(oneShot);
		}

		public void Fire(T item)
		{
			m_Action.Fire(item, null);
		}

		public void Error(Exception ex)
		{
			m_Action.Fire(default, ex);
		}

		public void Dispose() => m_Action?.Dispose();

	}

}
