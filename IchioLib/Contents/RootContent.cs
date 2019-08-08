using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ILib.Contents
{

	internal class RootContent : Content<BootParam>
	{
		Func<ITriggerAction> AppendFunc(IContentParam prm)
		{
			return () => Append(prm);
		}

		protected override ITriggerAction OnRun()
		{
			if (Param.ParallelBoot)
			{
				return Trigger.Combine(Param.BootContents.Select((x) => Append(x)).ToArray());
			}
			else
			{
				return Trigger.Sequential(Param.BootContents.Select(x => AppendFunc(x)));
			}
		}
	}

}
