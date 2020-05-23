using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLogInit
{
	[RuntimeInitializeOnLoadMethod]
	static void Init()
	{
		ILib.Logger.Log.Init();
		//ILib.Logger.Log.Level = ILib.Logger.Log.LogLevel.Trace;
	}
}