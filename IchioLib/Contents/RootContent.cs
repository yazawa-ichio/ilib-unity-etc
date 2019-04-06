using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ILib.Contents
{
	public class RootParam : ContentParam<RootContent>
	{
		public bool ParallelBoot { get; set; }
		public List<IContentParam> BootContents { get; set; } = new List<IContentParam>();

		public void Add<T>(object prm = null) where T : Content
		{
			BootContents.Add(SimpleParam.Create<T>(prm));
		}

	}

	public class RootContent : Content<RootParam>
	{
		IEnumerable<ITriggerAction> BootContents()
		{
			foreach (var ctx in Param.BootContents)
			{
				yield return Append(ctx).Action;
			}
		}

		protected override IEnumerator OnRun()
		{
			if (Param.ParallelBoot)
			{
				var contents = BootContents().ToArray();
				yield return Trigger.Wait(contents);
				foreach (var error in contents.Select(x => x.Error).Where(x => x != null)) {
					ThrowException(error);
				}
			}
			else
			{
				foreach (var ctx in BootContents())
				{
					yield return ctx.Wait();
				}
			}
		}
	}

}
