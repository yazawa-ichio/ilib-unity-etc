using System;
using System.Collections;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

using ILib;
using UnityEngine;
using UnityEngine.TestTools;

public class TriggerTest
{


	[Test]
	public void TriggerTest1()
	{
		var trigger = new Trigger<string>(oneShot: true);
		trigger.Fire("fire");
		string ret = "";
		Assert.IsTrue(trigger.Fired);
		//実行済みであればすぐに発火する
		trigger.Action.Add(x => ret = x);
		Assert.AreEqual(ret, "fire");

		//OneShotの場合は後で実行しても意味がない。
		trigger.Fire("test");
		Assert.AreNotEqual(ret, "test");
	}

	[Test]
	public void TriggerTest2()
	{
		var trigger = new Trigger<string>(oneShot: true);
		string ret = "";
		trigger.Action.Add(x => ret += x);
		trigger.Action.Add(x => ret += x);
		trigger.Action.Add(x => ret += x);
		trigger.Fire("fire");
		//複数を連結する
		Assert.AreEqual(ret, "firefirefire");
	}

	[Test]
	public void TriggerTest3()
	{
		var trigger = new Trigger<string>(oneShot: false);
		string ret = "";
		Action<string> onAction = x => ret += x;
		trigger.Action.Add(x => ret += x);
		trigger.Action.Add(x => ret += x);
		trigger.Action.Add(onAction);
		//連結されている
		trigger.Fire("fire");
		Assert.AreEqual(ret, "firefirefire");
		//解除したので連結数が減る。
		ret = "";
		trigger.Action.Remove(onAction);
		trigger.Fire("test");
		Assert.AreEqual(ret, "testtest");

		//コールバックの全て解除
		trigger.Action.Clear();
		ret = "";
		trigger.Fire("fire");
		Assert.AreEqual(ret, "");

		//キャンセルした場合は発火されない
		trigger.Action.Add(x => ret += x);
		trigger.Action.Cancel();
		ret = "";
		trigger.Fire("fire");
		Assert.AreEqual(ret, "");

	}

	[Test]
	public void TriggerTest4()
	{
		//トリガーの結果を変換して受け取る
		var trigger = new Trigger<string>(oneShot: false);
		var action = trigger.Select(x => int.Parse(x)).Where(x => x % 2 == 0);
		int ret = 0;
		action.Add(x => ret += x);
		int sum = 0;
		for (int i = 0; i < 25; i++)
		{
			if (i % 2 == 0) sum += i;
			trigger.Fire(i.ToString());
		}
		Assert.AreEqual(ret, sum);
	}

	[UnityTest]
	public IEnumerator TimeTest1()
	{
		var trigger = AsyncTrigger.Time(1);
		var val = "";
		trigger.Add(x => val = "fire");
		yield return new WaitForSeconds(0.5f);
		Assert.IsFalse(trigger.Fired);
		Assert.AreEqual(val, "");
		yield return new WaitForSeconds(1.5f);
		Assert.IsTrue(trigger.Fired);
		Assert.AreEqual(val, "fire");
	}

	[UnityTest]
	public IEnumerator TimeTest2()
	{
		var trigger = AsyncTrigger.Time(1);
		var val = "";
		trigger.Add(x => val = "fire");
		yield return new WaitForSeconds(0.5f);
		Assert.IsFalse(trigger.Fired);
		Assert.AreEqual(val, "");
		yield return new WaitForSeconds(1.5f);
		Assert.IsTrue(trigger.Fired);
		Assert.AreEqual(val, "fire");
	}


	[UnityTest]
	public IEnumerator TimeTest3()
	{
		Trigger<string> trigger = new Trigger<string>();
		var action = trigger.Time(2f);
		var val = "";
		action.Add(x => val = x);
		trigger.Fire("fire");
		yield return new WaitForSeconds(0.5f);
		//まだ発火されない
		Assert.IsFalse(action.Fired);
		//トリガーを待つ
		yield return action;
		Assert.IsTrue(trigger.Fired);
		Assert.AreEqual(val, "fire");
	}


	[UnityTest]
	public IEnumerator TimeTest4()
	{
		Trigger<string> trigger = new Trigger<string>();
		var action = trigger.Time(2f);
		var val = "";
		Exception ex = null;
		action.Add(x => val = x);
		action.AddFail(x => ex = x);
		yield return new WaitForSeconds(0.5f);
		//エラーはすぐに発火される。
		trigger.Error(new Exception("error"));
		Assert.IsTrue(action.Fired);
		Assert.AreEqual(val, "");
		Assert.IsNotNull(ex);
	}

}
