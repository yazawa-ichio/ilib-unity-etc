using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assert = UnityEngine.Assertions.Assert;
using ILib;
using ILib.Routines;
using UnityEngine.TestTools;
using System;

public class RoutineTest
{
	IEnumerator GetResults()
	{
		yield return new WaitForSeconds(0.2f);
		yield return Result<string>.Create("result1");
		yield return Result<string>.Create("result2");
		yield return Result<string>.Create("result3");
		yield return Result<string>.Create("result4");
		yield return Result<string>.Create("result5");
		yield return new WaitForSeconds(0.2f);
	}


	IEnumerator Error()
	{
		yield return new WaitForSeconds(0.5f);
		throw new System.Exception("error");
	}

	IEnumerator Nest(bool error)
	{
		yield return GetResults();
		if (error)
		{
			yield return Error();
		}
	}

	IEnumerator Nest2(bool error)
	{
		yield return null;
		yield return Nest(error);
	}

	IEnumerator Trigger(bool error, bool returnAction)
	{
		Trigger trigger = new Trigger();
		ILib.Trigger.Time(1).Add(() =>
		{
			if (!error)
			{
				trigger.Fire();
			}
			else
			{
				trigger.Error(new Exception("error"));
			}
		});
		if (returnAction)
		{
			yield return trigger.Action;
		}
		else
		{
			yield return trigger;
		}
	}

	class Tester : IDisposable, IHasRoutineOwner
	{
		class Behaviour : MonoBehaviour { }

		public MonoBehaviour RoutineOwner => m_behaviour;

		GameObject m_obj;
		Behaviour m_behaviour;

		public Tester()
		{
			m_obj = new GameObject("Tester");
			m_behaviour = m_obj.AddComponent<Behaviour>();
		}

		public void Dispose()
		{
			GameObject.Destroy(m_obj);
		}

	}

	[UnityTest]
	public IEnumerator Test1()
	{
		using (var tester = new Tester())
		{
			//コルーチンとして待てる
			yield return tester.Routine(GetResults());
		}
	}

	[UnityTest]
	public IEnumerator Test2()
	{
		using (var tester = new Tester())
		{
			//トリガーが実行される
			var routine = tester.Routine(GetResults());
			bool ret = false;
			routine.Action.Add(x => ret = x);
			yield return routine.Action;
			Assert.IsTrue(ret);
		}
	}

	[UnityTest]
	public IEnumerator Test3()
	{
		using (var tester = new Tester())
		{
			//エラーのハンドリング
			var routine = tester.Routine(Error());
			bool ret = false;
			Exception ex = null;
			routine.Action.Add((x, y) => { ret = x; ex = y; });
			yield return routine;
			Assert.IsFalse(ret);
			Assert.IsNotNull(ex);
			Assert.IsTrue(routine.Action.Fired);
		}
	}

	[UnityTest]
	public IEnumerator Test4()
	{
		using (var tester = new Tester())
		{
			//ネストしていても機能する
			var routine = tester.Routine(Nest2(false));
			bool ret = false;
			Exception ex = null;
			routine.Action.Add((x, y) => { ret = x; ex = y; });
			yield return routine;
			Assert.IsTrue(ret);
			Assert.IsNull(ex);
			Assert.IsTrue(routine.Action.Fired);
		}
	}


	[UnityTest]
	public IEnumerator Test5()
	{
		using (var tester = new Tester())
		{
			//ネストしていてもエラーを拾える
			var routine = tester.Routine(Nest2(true));
			bool ret = false;
			Exception ex = null;
			routine.Action.Add((x, y) => { ret = x; ex = y; });
			yield return routine;
			Assert.IsFalse(ret);
			Assert.IsNotNull(ex);
			Assert.IsTrue(routine.Action.Fired);
		}
	}

	[UnityTest]
	public IEnumerator Test6()
	{
		using (var tester = new Tester())
		{
			//リピートでは何度も実行できる
			var routine = tester.RepeatRoutine(GetResults);
			int count = 0;
			Exception ex = null;
			routine.Action.Add((x, y) => { count++; ex = y; });
			Assert.IsTrue(routine.Restartable);
			for (int i = 0; i < 5; i++)
			{
				if (!routine.IsRunning)
				{
					routine.Start();
				}
				yield return routine;
				Assert.AreEqual(count, i + 1);
				Assert.IsTrue(routine.Action.Fired);
				Assert.IsNull(ex);
			}
		}
	}

	[UnityTest]
	public IEnumerator Test7()
	{
		using (var tester = new Tester())
		{
			//結果を受け取れるコルーチン
			var routine = tester.TaskRoutine<string>(GetResults());
			string ret = "";
			Exception ex = null;
			routine.Action.Add((x, y) => { ret += x; ex = y; });
			yield return routine;
			Assert.AreEqual(ret, "result1");
			Assert.IsTrue(routine.Action.Fired);
			Assert.IsNull(ex);
		}
	}

	[UnityTest]
	public IEnumerator Test8()
	{
		using (var tester = new Tester())
		{
			//型が違うと受け取れない
			var routine = tester.TaskRoutine<int>(GetResults());
			string ret = "";
			Exception ex = null;
			routine.Action.Add((x, y) => { ret += x; ex = y; });
			yield return routine;
			Assert.AreNotEqual(ret, "result1");
			Assert.IsTrue(routine.Action.Fired);
			Assert.IsNotNull(ex);
		}
	}

	[UnityTest]
	public IEnumerator Test9()
	{
		using (var tester = new Tester())
		{
			//ネストしていても問題ない
			var routine = tester.TaskRoutine<string>(Nest2(false));
			string ret = "";
			Exception ex = null;
			routine.Action.Add((x, y) => { ret += x; ex = y; });
			yield return routine;
			Assert.AreEqual(ret, "result1");
			Assert.IsTrue(routine.Action.Fired);
			Assert.IsNull(ex);
		}
	}
	[UnityTest]
	public IEnumerator Test10()
	{
		using (var tester = new Tester())
		{
			//複数の結果が受け取れる
			var routine = tester.IterationTaskRoutine<string>(GetResults());
			string ret = "";
			Exception ex = null;
			routine.Action.Add((x, y) => { ret += x; ex = y; });
			yield return routine;
			Assert.AreEqual(ret, "result1result2result3result4result5");
			Assert.IsTrue(routine.Action.Fired);
			Assert.IsNull(ex);
		}
	}

	[UnityTest]
	public IEnumerator Test11()
	{
		using (var tester = new Tester())
		{
			//複数の結果を受け取る場合は、返り値がなくてもエラーにはならない
			var routine = tester.IterationTaskRoutine<int>(GetResults());
			string ret = "";
			Exception ex = null;
			routine.Action.Add((x, y) => { ret += x; ex = y; });
			yield return routine;
			Assert.AreEqual(ret, "");
			Assert.IsFalse(routine.Action.Fired);
			Assert.IsNull(ex);
		}
	}

	[UnityTest]
	public IEnumerator Test12()
	{
		using (var tester = new Tester())
		{
			//キャンセルした場合は発火しない
			var routine = tester.TaskRoutine<string>(GetResults());
			string ret = "";
			Exception ex = null;
			routine.Action.Add((x, y) => { ret += x; ex = y; });
			yield return null;
			routine.Cancel();
			yield return routine;
			Assert.AreEqual(ret, "");
			Assert.IsFalse(routine.Action.Fired);
			Assert.IsNull(ex);
		}
	}

	[UnityTest]
	public IEnumerator Test13()
	{
		using (var tester = new Tester())
		{
			var routine = tester.Routine(Trigger(false, false));
			yield return routine;
			Assert.IsNull(routine.Action.Error, routine.Action.Error?.Message ?? "test");
			Assert.IsTrue(routine.Action.Fired);
			routine = tester.Routine(Trigger(true, false));
			yield return routine;
			Assert.IsNotNull(routine.Action.Error);
			Assert.IsTrue(routine.Action.Fired);

			routine = tester.Routine(Trigger(false, true));
			yield return routine;
			Assert.IsNull(routine.Action.Error, routine.Action.Error?.Message ?? "test");
			Assert.IsTrue(routine.Action.Fired);
			routine = tester.Routine(Trigger(true, true));
			yield return routine;
			Assert.IsNotNull(routine.Action.Error);
			Assert.IsTrue(routine.Action.Fired);
		}
	}
}
