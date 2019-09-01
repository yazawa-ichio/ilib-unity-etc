using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.UI
{
	/// <summary>
	/// Animatorを利用した遷移処理です。
	/// SHOW・HIDEステートの完了を検知して遷移します。
	/// </summary>
	public class AnimatorTransition : MonoBehaviour, ITransition
	{

		Animator m_Animator;
		string m_WaitState;
		Trigger m_Wait;

		public void OnPreCreated()
		{
			m_Animator = GetComponent<Animator>();
		}

		public ITriggerAction Hide(bool close)
		{
			m_Wait?.Dispose();
			StopAllCoroutines();

			m_Animator.SetBool("OPEN", false);
			m_Animator.SetBool("SHOW", false);
			m_Animator.SetBool("CLOSE", close);
			m_Animator.SetBool("HIDE", true);

			m_WaitState = "HIDE";
			m_Wait = new Trigger();
			StartCoroutine(Wait());
			return m_Wait.Action;
		}

		public ITriggerAction Show(bool open)
		{
			m_Wait?.Dispose();
			StopAllCoroutines();
			m_Animator.SetBool("OPEN", open);
			m_Animator.SetBool("SHOW", true);
			m_Animator.SetBool("CLOSE", false);
			m_Animator.SetBool("HIDE", false);
			m_WaitState = "SHOW";
			m_Wait = new Trigger();
			StartCoroutine(Wait());
			return m_Wait.Action;
		}

		IEnumerator Wait()
		{
			while (true)
			{
				var info = m_Animator.GetCurrentAnimatorStateInfo(0);
				if (info.IsName(m_WaitState) && info.normalizedTime >= 1f)
				{
					m_Wait.Fire();
					m_Wait = null;
					yield break;
				}
				yield return null;
			}
		}

	}

}
