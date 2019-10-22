using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Contents
{
	using Caller;
	using Routines;
	using Logger;


	/// <summary>
	/// アプリケーションの全体の制御を行います。
	/// 個々の制御はContentを継承したクラスに記述します。
	/// </summary>
	public class ContentsController : MonoBehaviour, IHasDispatcher
	{
		EventCall m_Call = new EventCall();
		IDispatcher m_Dispatcher;
		Content m_Root;

		/// <summary>
		/// コンテンツを跨ぐ機能を提供するモジュールを管理します
		/// </summary>
		public ModuleCollection Modules { get; } = new ModuleCollection();

		/// <summary>
		/// ルートコンテンツ
		/// </summary>
		public Content Root => m_Root;

		/// <summary>
		/// イベントの
		/// </summary>
		public EventCall Call => m_Call;

		/// <summary>
		/// コンテンツに実行されるイベントの発火装置
		/// </summary>
		public IDispatcher Dispatcher => m_Dispatcher != null ? m_Dispatcher : m_Dispatcher = new Dispatcher(m_Call);

		/// <summary>
		/// コンテンツの例外をハンドリングします
		/// </summary>
		public event Action<Exception> OnException;

		ITriggerAction<bool> Routine(IEnumerator enumerator, bool throwError)
		{
			var routine = this.Routine(enumerator);
			if (throwError) routine.AddFail(ThrowException);
			return routine.Action;
		}

		/// <summary>
		/// コントローラーを起動します。
		/// BootParamで指定したコンテンツが起動します。
		/// </summary>
		public ITriggerAction<bool> Boot(BootParam param, bool throwError = true)
		{
			return Boot<RootContent>(param, throwError);
		}

		/// <summary>
		/// 指定したコンテンツでコントローラーを起動します。
		/// </summary>
		public ITriggerAction<bool> Boot<T>(object prm, bool throwError = true) where T : Content, new()
		{
			if (m_Root != null) throw new InvalidOperationException("already boot ContentsController");
			m_Root = new T();
			return Routine(m_Root.Boot(this, null, prm, null), throwError);
		}

		/// <summary>
		/// コントローラーを終了します。
		/// </summary>
		public ITriggerAction<bool> Shutdown()
		{
			return this.Routine(ShutdownImpl()).Action;
		}

		IEnumerator ShutdownImpl()
		{
			yield return m_Root.DoShutdown();
			m_Root = null;
		}

		/// <summary>
		/// 指定のタイプのコンテンツを取得します。
		/// </summary>
		public T Get<T>() where T : Content
		{
			return m_Root.Get<T>();
		}

		/// <summary>
		/// 指定のタイプのコンテンツをすべて取得します。
		/// </summary>
		public IEnumerable<T> GetAll<T>() where T : Content
		{
			return m_Root.GetAll<T>();
		}

		void OnDestroy()
		{
			m_Call.Dispose();
			m_Call = null; ;
			OnDestroyEvent();
		}

		protected virtual void OnDestroyEvent() { }

		/// <summary>
		/// コントローラーに例外をスローします
		/// </summary>
		public void ThrowException(Exception ex)
		{
			if (!HandleException(ex))
			{
				if (OnException != null)
				{
					OnException(ex);
				}
				else
				{
					Log.Exception(ex);
				}
			}
		}

		protected virtual bool HandleException(Exception ex) => false;

	}



}
