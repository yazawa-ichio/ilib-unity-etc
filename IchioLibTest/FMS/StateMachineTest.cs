using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

using ILib.FSM;
using ILib.FSM.Provider;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

public class StateMachineTest
{

	public enum StateEvent
	{
		Event1,
		Event2,
		Event3,
		Event4,
		Event5,
	}

	class AutoStateBase : State
	{
		protected override void OnEnter()
		{
			Debug.Log(this);
		}
	}

	[DirectTransition(StateEvent.Event1)]
	class AutoState1 : AutoStateBase { public static int count = 0; protected override void OnEnter() { count++; base.OnEnter(); } }
	[DirectTransition(StateEvent.Event2, UseInit = true)]
	class AutoState2 : AutoStateBase { public static int count = 0; protected override void OnEnter() { count++; base.OnEnter(); } }
	[DirectTransition(StateEvent.Event3, ReTranstion = true)]
	class AutoState3 : AutoStateBase { public static int count = 0; protected override void OnEnter() { count++; base.OnEnter(); } }
	[DirectTransition(StateEvent.Event4)]
	class AutoState4 : AutoStateBase { public static int count = 0; protected override void OnEnter() { count++; base.OnEnter(); } }

	void Init()
	{
		AutoState1.count = 0;
		AutoState2.count = 0;
		AutoState3.count = 0;
		AutoState4.count = 0;
	}

	void Check(int state1, int state2, int state3, int state4)
	{
		Assert.AreEqual(state1, AutoState1.count);
		Assert.AreEqual(state2, AutoState2.count);
		Assert.AreEqual(state3, AutoState3.count);
		Assert.AreEqual(state4, AutoState4.count);
	}

	[Test]
	public void Test1()
	{
		Init();
		bool ret = false;
		var stateMachine = StateMachineProvider<AutoStateBase>.Create(null);
		//2に遷移
		Check(0, 1, 0, 0);
		ret = stateMachine.StrictTransition(StateEvent.Event2);
		//直接遷移は基本的に同じステートには遷移しない
		Check(0, 1, 0, 0);
		Assert.IsFalse(ret);
		ret = stateMachine.StrictTransition(StateEvent.Event3);
		//ステート3に遷移する
		Check(0, 1, 1, 0);
		Assert.IsTrue(ret);
		stateMachine.Transition(StateEvent.Event3);
		//再遷移フラグを付けた場合可能になる
		Check(0, 1, 2, 0);
		stateMachine.Transition(StateEvent.Event1);
		//ステート1に遷移する
		Check(1, 1, 2, 0);
		//存在しないイベントを発行
		ret = stateMachine.StrictTransition(StateEvent.Event5);
		Assert.IsFalse(ret);
	}

}
