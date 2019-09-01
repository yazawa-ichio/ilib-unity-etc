namespace ILib.UI
{
	public interface IController
	{
		/// <summary>
		/// UIを閉じます。
		/// 閉じる方法に関しては各々のコントローラーによって挙動が異なります。
		/// </summary>
		ITriggerAction Close(IControl control);
	}
}
