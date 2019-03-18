using System;
using System.Collections;

namespace ILib.Contents
{

	[Flags]
	public enum ModuleType
	{
		None = 0,
		PreBoot = 1,
		Boot = 2,
		PreShutdown = 3,
		Shutdown = 4,
		PreRun = 5,
		Run = 6,
		PreSuspend = 7,
		Suspend = 8,
		PreEnable = 9,
		Enable = 10,
		PreDisable = 11,
		Disable = 12,
		PreSwitch = 13,
		Switch = 14,
		EndSwitch = 15,
	}

	/// <summary>
	/// アプリケーションの共通で行いたい処理を実装します。
	/// Typeで指定した関数しか実行されません。
	/// </summary>
	public abstract class Module
	{
		public int Priority { get; set; }
		public abstract ModuleType Type { get; }

		public virtual IEnumerator OnPreBoot(Content content) { yield break; }
		public virtual IEnumerator OnBoot(Content content) { yield break; }
		public virtual IEnumerator OnPreShutdown(Content content) { yield break; }
		public virtual IEnumerator OnShutdown(Content content) { yield break; }
		public virtual IEnumerator OnPreRun(Content content) { yield break; }
		public virtual IEnumerator OnRun(Content content) { yield break; }
		public virtual IEnumerator OnPreSuspend(Content content) { yield break; }
		public virtual IEnumerator OnSuspend(Content content) { yield break; }
		public virtual IEnumerator OnPreEnable(Content content) { yield break; }
		public virtual IEnumerator OnEnable(Content content) { yield break; }
		public virtual IEnumerator OnPreDisable(Content content) { yield break; }
		public virtual IEnumerator OnDisable(Content content) { yield break; }
		public virtual IEnumerator OnPreSwitch(Content prev, Content next) { yield break; }
		public virtual IEnumerator OnSwitch(Content prev, Content next) { yield break; }
		public virtual IEnumerator OnEndSwitch(Content prev, Content next) { yield break; }

	}

}
