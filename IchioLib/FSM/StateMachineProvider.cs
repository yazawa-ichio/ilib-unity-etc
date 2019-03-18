using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ILib.FSM.Provider
{

	/// <summary>
	/// 初期ステートに設定します。
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class InitStateAttribute : Attribute
	{
	}

	/// <summary>
	/// 自身からの遷移イベントを登録します。
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class TransitionToAttribute : Attribute
	{
		public int EventId { get; private set; }
		public Type ToState { get; private set; }
		public TransitionToAttribute(int eventId, Type toState)
		{
			EventId = eventId;
			ToState = ToState;
		}
		public TransitionToAttribute(object eventId, Type toState)
		{
			EventId = (eventId as System.IConvertible).ToInt32(null);
			ToState = ToState;
		}
		public bool UseInit { get; set; }
	}

	/// <summary>
	/// 自身への直接遷移イベントを登録します。
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class DirectTransitionAttribute : Attribute
	{
		public int EventId { get; private set; }
		public DirectTransitionAttribute(int eventId)
		{
			EventId = eventId;
		}
		public DirectTransitionAttribute(object eventId)
		{
			EventId = (eventId as IConvertible).ToInt32(null);
		}
		public bool UseInit { get; set; }
		public bool ReTranstion { get; set; }
	}

	/// <summary>
	/// T型を継承するステートをノードとするステートマシーンを自動生成します。
	/// 初期ステートと遷移条件は属性によって決定されます。
	/// 小規模な重要ではないステートマシーンに利用してください。
	/// </summary>
	public static class StateMachineProvider<T> where T : State
	{
		public static int MaxStateNum = 16;

		class StateInfo
		{
			public Type Type;
			public TransitionToAttribute[] Transitions;
			public DirectTransitionAttribute[] DirectTransitions;
			public bool UseInit;
			//一時用のキャッシュ
			State m_state;
			public State GetState() => m_state = m_state ?? (State)Activator.CreateInstance(Type);
			public void ClearState() => m_state = null;
		}

		static StateInfo[] s_StateInfos;


		/// <summary>
		/// ステート情報を初期化します。実行されていない場合、ステートマシーン作成時に自動で実行されます。
		/// 属性情報を取得するため、事前に実行しておくことでスパイクの原因を減らせます。
		/// </summary>
		public static void Init()
		{
			if (s_StateInfos != null) return;
			var stateInfos = new List<StateInfo>();
			foreach (var type in typeof(T).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract))
			{
				StateInfo info = new StateInfo();
				info.Type = type;
				info.Transitions = (TransitionToAttribute[])Attribute.GetCustomAttributes(type, typeof(TransitionToAttribute), false);
				info.DirectTransitions = (DirectTransitionAttribute[])Attribute.GetCustomAttributes(type, typeof(DirectTransitionAttribute), false);
				info.UseInit = Attribute.GetCustomAttribute(type, typeof(InitStateAttribute), false) != null;
				stateInfos.Add(info);
			}
			if (stateInfos.Count > MaxStateNum) throw new Exception($"auto provide state count is less than {MaxStateNum}.");
			s_StateInfos = stateInfos.ToArray();
		}

		/// <summary>
		/// ステートマシーンを作成します。
		/// 初期ステートを指定する場合はInitStateAttribute属性を使用してください。
		/// </summary>
		public static StateMachine Create(object owner, object prm = null)
		{
			//初期化
			if (s_StateInfos == null) Init();

			StateMachine stateMachine = new StateMachine(owner);
			State init = null;
			//ステートと遷移条件を登録
			foreach (var info in s_StateInfos)
			{
				var state = info.GetState();
				stateMachine.AddState(state);
				bool useInit = info.UseInit;
				foreach (var transiton in info.DirectTransitions)
				{
					stateMachine.AddDirectTransition(transiton.EventId, state, transiton.ReTranstion);
					useInit |= transiton.UseInit;
				}
				foreach (var transiton in info.Transitions)
				{
					var to = s_StateInfos.First((x) => x.Type == transiton.ToState).GetState();
					stateMachine.AddTransition(transiton.EventId, state, to);
					useInit |= transiton.UseInit;
				}
				if (useInit)
				{
					init = state;
				}
			}
			//キャッシュを破棄
			foreach (var info in s_StateInfos)
			{
				info.ClearState();
			}
			//初期化
			if (init != null)
			{
				stateMachine.Init(init, prm);
			}
			else
			{
				stateMachine.Init(prm);
			}
			return stateMachine;
		}


	}




}
