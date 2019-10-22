using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib
{
	internal static class TriggerBehaviour
	{
		class Behaviour : MonoBehaviour { }

		static Behaviour s_Behaviour;
		static Behaviour GetBehaviour()
		{
			if (s_Behaviour != null) return s_Behaviour;
			GameObject obj = new GameObject("TriggerBehaviour");
			GameObject.DontDestroyOnLoad(obj);
			return s_Behaviour = obj.AddComponent<Behaviour>();
		}

		static IEnumerator StartImpl(IEnumerator enumerable, Action action)
		{
			yield return enumerable;
			action?.Invoke();
		}

		static void Start(MonoBehaviour behaviour, IEnumerator enumerable, Action action)
		{
			behaviour.StartCoroutine(StartImpl(enumerable, action));
		}

		static IEnumerator TimeImpl(float time)
		{
			yield return new WaitForSeconds(time);
		}

		static IEnumerator RealtimeImpl(float time)
		{
			yield return new WaitForSecondsRealtime(time);
		}

		public static ITriggerAction<bool> Time(float time)
		{
			Trigger<bool> trigger = new Trigger<bool>();
			Start(GetBehaviour(), TimeImpl(time), () => trigger.Fire(true));
			return trigger.Action;
		}

		public static ITriggerAction<bool> Realtime(float time)
		{
			Trigger<bool> trigger = new Trigger<bool>();
			Start(GetBehaviour(), RealtimeImpl(time), () => trigger.Fire(true));
			return trigger.Action;
		}

	}

}
