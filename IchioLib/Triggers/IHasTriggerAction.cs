namespace ILib
{
	/// <summary>
	/// トリガーに対するアクションを保持することを保証するクラスです。
	/// 拡張メソッドでアクションと同等の機能を提供します。
	/// </summary>
	public interface IHasTriggerAction<T> : System.IDisposable
	{
		ITriggerAction<T> Action { get; }
	}

}
