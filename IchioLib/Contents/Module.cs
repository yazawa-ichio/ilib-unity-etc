﻿using System.Collections;

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
		public virtual ITriggerAction OnPreBoot(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの初期化直後のイベントです。
		/// コンテンツ毎に一度しか実行されません。
		/// </summary>
		public virtual ITriggerAction OnBoot(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの終了直前のイベントです。
		/// コンテンツ毎に一度しか実行されません。
		/// </summary>
		public virtual ITriggerAction OnPreShutdown(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの終了直後のイベントです。
		/// コンテンツ毎に一度しか実行されません。
		/// </summary>
		public virtual ITriggerAction OnShutdown(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの実行直前のイベントです。
		/// サスペンドから復帰する際にも呼ばれます。
		/// </summary>
		public virtual ITriggerAction OnPreRun(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの実行時のイベントです。
		/// サスペンドから復帰する際にも呼ばれます。
		/// </summary>
		public virtual ITriggerAction OnRun(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの停止直前のイベントです。
		/// </summary>
		public virtual ITriggerAction OnPreSuspend(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの停止時のイベントです。
		/// </summary>
		public virtual ITriggerAction OnSuspend(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの有効化直前のイベントです。
		/// 親のコンテンツが有効になった際も実行されます。
		/// </summary>
		public virtual ITriggerAction OnPreEnable(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの有効化直後のイベントです。
		/// 親のコンテンツが有効になった際も実行されます。
		/// </summary>
		public virtual ITriggerAction OnEnable(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの無効化直前のイベントです。
		/// 親のコンテンツが無効になった際も実行されます。
		/// </summary>
		public virtual ITriggerAction OnPreDisable(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツの無効化直後のイベントです。
		/// 親のコンテンツが無効になった際も実行されます。
		/// </summary>
		public virtual ITriggerAction OnDisable(Content content) => Trigger.Successed;
		/// <summary>
		/// コンテンツを遷移直前のイベントです。
		/// </summary>
		public virtual ITriggerAction OnPreSwitch(Content prev, Content next) => Trigger.Successed;
		/// <summary>
		/// コンテンツを遷移イベントです。
		/// </summary>
		public virtual ITriggerAction OnSwitch(Content prev, Content next) => Trigger.Successed;
		/// <summary>
		/// コンテンツを遷移完了イベントです。
		/// </summary>
		public virtual ITriggerAction OnEndSwitch(Content prev, Content next) => Trigger.Successed;

	}

}
