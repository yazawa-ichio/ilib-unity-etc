using System.Collections;

namespace ILib.Contents
{

	/// <summary>
	/// アプリケーションの共通で行いたい処理を実装します。
	/// Typeで指定した関数しか実行されません。
	/// </summary>
	public abstract class Module
	{

		public int Priority { get; set; }

		/// <summary>
		/// Typeに登録したイベントのみ実行されます。
		/// </summary>
		public abstract ModuleType Type { get; }

		/// <summary>
		/// コンテンツの初期化直前のイベントです。
		/// コンテンツ毎に一度しか実行されません。
		/// </summary>
		public virtual IEnumerator OnPreBoot(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの初期化直後のイベントです。
		/// コンテンツ毎に一度しか実行されません。
		/// </summary>
		public virtual IEnumerator OnBoot(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの終了直前のイベントです。
		/// コンテンツ毎に一度しか実行されません。
		/// </summary>
		public virtual IEnumerator OnPreShutdown(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの終了直後のイベントです。
		/// コンテンツ毎に一度しか実行されません。
		/// </summary>
		public virtual IEnumerator OnShutdown(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの実行直前のイベントです。
		/// サスペンドから復帰する際にも呼ばれます。
		/// </summary>
		public virtual IEnumerator OnPreRun(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの実行時のイベントです。
		/// サスペンドから復帰する際にも呼ばれます。
		/// </summary>
		public virtual IEnumerator OnRun(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの停止直前のイベントです。
		/// </summary>
		public virtual IEnumerator OnPreSuspend(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの停止時のイベントです。
		/// </summary>
		public virtual IEnumerator OnSuspend(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの有効化直前のイベントです。
		/// 親のコンテンツが有効になった際も実行されます。
		/// </summary>
		public virtual IEnumerator OnPreEnable(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの有効化直後のイベントです。
		/// 親のコンテンツが有効になった際も実行されます。
		/// </summary>
		public virtual IEnumerator OnEnable(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの無効化直前のイベントです。
		/// 親のコンテンツが無効になった際も実行されます。
		/// </summary>
		public virtual IEnumerator OnPreDisable(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの無効化直後のイベントです。
		/// 親のコンテンツが無効になった際も実行されます。
		/// </summary>
		public virtual IEnumerator OnDisable(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツを遷移直前のイベントです。
		/// </summary>
		public virtual IEnumerator OnPreSwitch(Content prev, Content next) => Trigger.Successed;
		/// <summary>
		/// コンテンツを遷移イベントです。
		/// </summary>
		public virtual IEnumerator OnSwitch(Content prev, Content next) => Trigger.Successed;
		/// <summary>
		/// コンテンツを遷移完了イベントです。
		/// </summary>
		public virtual IEnumerator OnEndSwitch(Content prev, Content next) => Trigger.Successed;

	}

}
