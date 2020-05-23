using System;
using System.Collections;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

using ILib;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

	[Test]
	public void TriggerTest5()
	{
		//複数の結果をまとめる
		int[] input = new int[] { 1, 5, 7, 3, 5, 7 };
		var triggers = new List<Trigger<int>>();
		for (int i = 0; i < input.Length; i++)
		{
			triggers.Add(new Trigger<int>());
		}
		var combine = Trigger.Combine(triggers.Select(x => x.Action).ToArray());
		//まだ発火していない。
		Assert.IsFalse(combine.Fired);
		combine.Add(x =>
		{
			for (int i = 0; i < input.Length; i++)
			{
				Assert.AreEqual(x[i], input[i], "入力と同じ");
			}
		});
		for (int i = 0; i < input.Length; i++)
		{
			triggers[i].Fire(input[i]);
			if (i == input.Length - 1)
			{
				Assert.IsTrue(combine.Fired);
			}
			else
			{
				Assert.IsFalse(combine.Fired);
			}
		}
		//引数がなければすぐに発火される
		Assert.IsTrue(Trigger.Combine().Fired);
		Assert.IsTrue(Trigger.Combine<bool>().Fired);
	}

	[UnityTest]
	public IEnumerator TimeTest1()
	{
		var trigger = Trigger.Time(1);
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
		var trigger = Trigger.Time(1);
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

	[UnityTest]
	public IEnumerator AsyncTest1()
	{
		var action = Trigger.From(_AsyncTest1());
		yield return new WaitForSeconds(0.1f);
		Assert.IsFalse(action.Fired);
		yield return action;
		Assert.IsTrue(action.Fired);
	}

	async Task _AsyncTest1()
	{
		await Task.Delay(1000);
	}

	[UnityTest]
	public IEnumerator AsyncTest2()
	{
		bool complete = false;
		var action = Trigger.From(_AsyncTest2(() => complete = true));
		Assert.IsFalse(complete);
		yield return action;
		Assert.IsTrue(complete);
	}

#pragma warning disable 4014
	async Task _AsyncTest2(Action result)
	{
		//値ありのawait
		var ret = await Trigger.Time(1f);
		Assert.IsTrue(ret);

		var trigger = new Trigger();
		ITriggerAction action = trigger.Action;
		Trigger.Time(1f).Add(x => trigger.Fire());

		var tmp = Time.time;
		//値なしのITriggerActionでもawait出来る
		await action;
		Assert.IsTrue(tmp + 0.9f < Time.time);

		//発火済みでも正常に動作する
		tmp = Time.time;
		ret = false;
		ret = await Trigger.Successed;
		Assert.AreEqual(tmp, Time.time);
		Assert.IsTrue(ret);

		result();
	}
#pragma warning restore 4014

}