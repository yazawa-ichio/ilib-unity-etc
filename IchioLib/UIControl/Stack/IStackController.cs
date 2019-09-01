namespace ILib.UI
{
	internal interface IStackController
	{
		bool IsFornt(StackEntry entry);
		ITriggerAction Pop(StackEntry entry);
	}
}
