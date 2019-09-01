namespace ILib.UI
{
	internal interface IQueueController
	{
		void Cancel(QueueEntry entry);
		ITriggerAction<bool> Close(IQueueEntry entry);
	}
}
