namespace ILib
{
	public interface IHasTriggerAction : System.IDisposable
	{
		ITriggerAction Action { get; }
	}

	/// <summary>
	/// トリガーに対するアクションを保持することを保証するクラスです。
	/// 拡張メソッドでアクションと同等の機能を提供します。
	/// </summary>
	public interface IHasTriggerAction<T> : IHasTriggerAction
	{
		new ITriggerAction<T> Action { get; }
	}

}