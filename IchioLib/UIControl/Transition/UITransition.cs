using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ILib.Routines;

namespace ILib.UI
{
	/// <summary>
	/// uGUIの機能を使った簡単なアニメーションの遷移の実装です。
	/// </summary>
	public class UITransition : MonoBehaviour, ITransition
	{
		public interface IAnim
		{
			void Init();
			IEnumerator Run(bool show);
		}

		[System.Serializable]
		public class Fade : IAnim
		{
			[SerializeField]
			float m_Time = 0.3f;
			[SerializeField]
			CanvasGroup[] m_Target = null;
			[SerializeField]
			float m_Show = 1f;
			[SerializeField]
			float m_Hide = 0f;
			[SerializeField]
			AnimationCurve m_Curve = AnimationCurve.Linear(0, 0, 1f, 1f);
			[SerializeField]
			bool m_Realtime = false;

			public void Init()
			{
				foreach (var target in m_Target)
				{
					target.alpha = m_Hide;
				}
			}

			public IEnumerator Run(bool show)
			{
				float cur = 0f;
				while (cur < m_Time)
				{
					float rate = (cur / m_Time);
					if (show)
					{
						rate = m_Curve.Evaluate(cur / m_Time);
					}
					else
					{
						rate = m_Curve.Evaluate(1f - cur / m_Time);
					}
					foreach (var target in m_Target)
					{
						target.alpha = m_Hide + (m_Show - m_Hide) * rate;
					}
					yield return null;
					cur += m_Realtime ? Time.unscaledDeltaTime : Time.deltaTime;
				}
				foreach (var target in m_Target)
				{
					target.alpha = show ? m_Show : m_Hide;
				}
			}

		}

		[System.Serializable]
		public class Move : IAnim
		{
			[SerializeField]
			float m_Time = 0.3f;
			[SerializeField]
			Transform m_Target = null;
			[SerializeField]
			Vector3 m_ShowPos = default;
			[SerializeField]
			Vector3 m_HidePos = default;
			[SerializeField]
			AnimationCurve m_Curve = AnimationCurve.Linear(0, 0, 1f, 1f);
			[SerializeField]
			bool m_Realtime = false;

			public void Init()
			{
				m_Target.localPosition = m_HidePos;
			}

			public IEnumerator Run(bool show)
			{
				float cur = 0f;
				while (cur < m_Time)
				{
					float rate = (cur / m_Time);
					if (show)
					{
						rate = m_Curve.Evaluate(cur / m_Time);
					}
					else
					{
						rate = m_Curve.Evaluate(1f - cur / m_Time);
					}
					m_Target.localPosition = m_HidePos + (m_ShowPos - m_HidePos) * rate;
					yield return null;
					cur += m_Realtime ? Time.unscaledDeltaTime : Time.deltaTime;
				}
				m_Target.localPosition = show ? m_ShowPos : m_HidePos;
			}

		}

		[SerializeField]
		Fade m_Fade = default;
		[SerializeField]
		Move m_Move = default;

		IAnim[] m_Anim;

		public void OnPreCreated()
		{
			m_Anim = new IAnim[] { m_Fade, m_Move }; 
			foreach (var anim in m_Anim)
			{
				anim.Init();
			}
		}

		public ITriggerAction Hide(bool close)
		{
			var hide = m_Anim.Select(x => this.Routine(x.Run(false)).Action);
			return Trigger.Combine(hide.ToArray());
		}

		public ITriggerAction Show(bool open)
		{
			var show = m_Anim.Select(x => this.Routine(x.Run(true)).Action);
			return Trigger.Combine(show.ToArray());
		}

	}

}
