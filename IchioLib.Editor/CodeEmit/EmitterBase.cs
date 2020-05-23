using System.Collections;
using System.Collections.Generic;

namespace ILib.CodeEmit
{

	public abstract class EmitterBase
	{
		public abstract void Emit(CodeWriter writer);
	}

}