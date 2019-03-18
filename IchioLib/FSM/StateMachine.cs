using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.FSM
{
	//イベントによる遷移を行うステートマシーンです。イベント時にパラメーターを渡す事も可能です。
	//遷移方法はステート間の遷移を明確に定義する方法とどのステートからの直接遷移の二つを利用できます。
	//複雑なステートマシンを実現する場合は遷移できるステートを明確に定義し、リセット処理などは直接遷移でどの状態からも出来るようになど併用も可能です。


	/// <summary>
	/// ステートの抽象クラスです
	/// 継承先で実装してください。
	/// </summary>
	public abstract class State
	{
		object m_Param;
		protected object Param { get { return m_Param; } }
		protected object Owner { get { return m_stateMachine.m_Owner; } }

		internal StateMachine m_stateMachine;
		protected virtual void OnEnter() { }
		protected virtual void OnUpdate() { }
		protected virtual void OnExit() { }

		internal void Init(StateMachine stateMachine) => m_stateMachine = stateMachine;
		internal void DoEnter(object prm) { m_Param = prm; OnEnter(); }
		internal void DoExit() { OnExit(); m_Param = null; }
		internal void DoUpdate() => OnUpdate();

		/// <summary>
		/// 遷移イベントを発行します
		/// 発火したステートと現在が違う場合無効になります。
		/// </summary>
		protected bool Transition(int eventId, object prm = null)
		{
			return m_stateMachine.Transition(this, eventId, prm);
		}

		/// <summary>
		/// 遷移イベントを発行します
		/// 発火したステートと現在が違う場合無効になります。
		/// Enum暗黙的にintに置き換えられます。
		/// </summary>
		protected bool Transition<TEnum>(TEnum eventId, object prm = null) where TEnum : struct, System.IConvertible
		{
			return m_stateMachine.Transition(this, eventId.ToInt32(null), prm);
		}
	}

	/// <summary>
	/// ステート管理するクラスです。
	/// 遷移や更新処理はInitを実行するまで行われません。
	/// </summary>
	public class StateMachine
	{

		class TranstionInfo
		{
			public int EventId;
			public State From;
			public State To;
			public bool ReTranstion;
		}

		public bool Initialized { get; private set; }

		State m_Current;
		List<TranstionInfo> m_Transtions = new List<TranstionInfo>();
		bool m_LockTransition;
		internal object m_Owner;

		public StateMachine(object owner)
		{
			m_Owner = owner;
		}

		/// <summary>
		/// ステートマシーンを起動します。
		/// 一番最初に登録されたステートを初期ステートにします。
		/// 指定したい場合は引数にステートを渡してください。
		/// </summary>
		/// <param name="state"></param>
		public void Init(object prm = null)
		{
			if (Initialized) throw new System.InvalidOperationException("initalized");
			Initialized = true;
			m_Current.DoEnter(prm);
		}

		/// <summary>
		/// 指定のステートでステートマシーンを起動します。
		/// </summary>
		public void Init(State state, object prm = null)
		{
			if (Initialized) throw new System.InvalidOperationException("initalized");
			m_Current = state;
			Init(prm);
		}

		/// <summary>
		/// ステートを追加します。
		/// </summary>
		public State AddState(State state)
		{
			if (!Initialized && m_Current == null) m_Current = state;
			state.Init(this);
			return state;
		}

		/// <summary>
		/// ステートを生成と追加を行います。
		/// </summary>
		public T AddState<T>() where T : State, new()
		{
			var state = new T();
			if (!Initialized && m_Current == null) m_Current = state;
			state.Init(this);
			return state;
		}

		/// <summary>
		/// イベントによるステート間の遷移を定義します。
		/// 列挙型は暗黙的にintに置き換えられます。
		/// </summary>
		public void AddTransition<TEnum>(TEnum eventId, State from, State to) where TEnum : struct, System.IConvertible
		{
			AddTransition(eventId.ToInt32(null), from, to);
		}

		/// <summary>
		/// イベントによるステート間の遷移を定義します。
		/// 同じイベントの場合は後で追加したイベントが優先されます。
		/// </summary>
		public void AddTransition(int eventId, State from, State to)
		{
			//遷移元を定義しないことは別の意味を持つので強めに制約を掛ける
			if (from == null) throw new System.ArgumentNullException(nameof(from));
			TranstionInfo info = new TranstionInfo
			{
				EventId = eventId,
				From = from,
				To = to,
				ReTranstion = true,
			};
			m_Transtions.Add(info);
		}

		/// <summary>
		/// イベントによる直接遷移を定義します。
		/// 直接遷移はどのステートからも実行が可能です。
		/// 同じイベントの場合は後で追加したイベントが優先されます。
		/// 列挙型は暗黙的にintに置き換えられます。
		/// </summary>
		public void AddDirectTransition<TEnum>(TEnum eventId, State to, bool reTranstion = false) where TEnum : struct, System.IConvertible
		{
			AddDirectTransition(eventId.ToInt32(null), to, reTranstion);
		}

		/// <summary>
		/// イベントによる直接遷移を定義します。
		/// 直接遷移はどのステートからも実行が可能です。
		/// 同じイベントの場合は後で追加したイベントが優先されます。
		/// </summary>
		public void AddDirectTransition(int eventId, State to, bool reTranstion = false)
		{
			TranstionInfo info = new TranstionInfo
			{
				EventId = eventId,
				From = null,
				To = to,
				ReTranstion = reTranstion,
			};
			m_Transtions.Add(info);
		}

		/// <summary>
		/// ステートからの遷移イベントです。
		/// 発火したステートと現在のステートが違う場合無効になります。
		/// </summary>
		internal bool Transition(State from, int eventId, object prm = null)
		{
			if (from != m_Current)
			{
				Debug.AssertFormat(false, "current state is {0}.", m_Current);
				return false;
			}
			return Transition(eventId, prm);
		}

		/// <summary>
		/// 遷移イベントを発行します
		/// 列挙型は暗黙的にintに置き換えられます。
		/// </summary>
		public bool Transition<TEnum>(TEnum eventId, object prm = null) where TEnum : struct, System.IConvertible
		{
			return Transition(eventId.ToInt32(null), prm);
		}

		/// <summary>
		/// 遷移イベントを発行します
		/// </summary>
		public bool Transition(int eventId, object prm = null)
		{
			if (!Initialized) throw new System.InvalidOperationException("use Init().");
			//遅延処理は面倒なのでロックを掛けてお茶を濁す
			if (m_LockTransition)
			{
				Debug.Assert(false);
				return false;
			}
			//遷移条件は後ろからなめる
			for (int i = m_Transtions.Count - 1; i >= 0; i--)
			{
				TranstionInfo info = m_Transtions[i];
				if (info.EventId == eventId && (info.From == null || info.From == m_Current) && (info.To != m_Current || info.ReTranstion))
				{
					try
					{
						m_LockTransition = true;
						var prev = m_Current;
						prev.DoExit();
						m_Current = info.To;
						m_Current.DoEnter(prm);
					}
					finally
					{
						m_LockTransition = false;
					}
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// ステートで更新処理が必要な場合に実行してください。
		/// </summary>
		public void Update()
		{
			if (Initialized && m_Current != null)
			{
				m_Current.DoUpdate();
			}
		}


	}

}
