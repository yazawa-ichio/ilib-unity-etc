namespace ILib.CodeEmit
{
	public class AppendEmitter : EmitterBase
	{
		public string Message;
		public AppendEmitter(string message) => Message = message;
		public override void Emit(CodeWriter writer)
		{
			writer.Append(Message);
		}
	}

}
