using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Contents
{
	using Caller;
	using Routines;


	/// <summary>
	/// アプリケーションの全体の制御を行います。
	/// 個々の制御はContentを継承したクラスに記述します。
	/// </summary>
	public class ContentsController : MonoBehaviour, IHasDispatcher
	{
		Call m_Call = new Call();
		IDispatcher m_Dispatcher;
		Content m_Root;
		Content m_Current;
		public ModuleCollection Modules { get; } = new ModuleCollection();

		internal Call SubCall() => m_Call.SubCall();
		public IDispatcher Dispatcher => m_Dispatcher != null ? m_Dispatcher : m_Dispatcher = new Dispatcher(m_Call);
		public event Action<Exception> OnException;

		IRoutine<bool> Routine(IEnumerator enumerator, bool throwError)
		{
			var routine = this.Routine(enumerator);
			if (throwError) routine.AddFail(ThrowException);
			return routine;
		}

		public IRoutine<bool> Boot(RootParam param, bool throwError = true)
		{
			return Boot<RootContent>(param, throwError);
		}

		public IRoutine<bool> Boot<T>(object prm, bool throwError = true) where T : Content, new()
		{
			if (m_Root != null) throw new System.InvalidOperationException("already boot ContentsController");
			m_Root = new T();
			return this.Routine(m_Root.Boot(this, null, prm), throwError);
		}

		public IRoutine<bool> Shutdown()
		{
			return this.Routine(ShutdownImpl());
		}

		IEnumerator ShutdownImpl()
		{
			yield return m_Root.DoShutdown();
			m_Root = null;
		}


		public IContentRef Get<T>() where T : Content
		{
			return m_Root.Get<T>();
		}

		public IContentRef Get(Type type)
		{
			return m_Root.Get(type);
		}

		public IEnumerable<IContentRef> GetAll<T>() where T : Content
		{
			return m_Root.GetAll<T>();
		}

		void OnDestroy()
		{
			m_Call.Dispose();
			m_Call = null; ;
		}

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
					Debug.LogException(ex);
				}
			}
		}

		protected virtual bool HandleException(Exception ex) => false;

	}



}
