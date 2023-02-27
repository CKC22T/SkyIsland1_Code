using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Global.SceneChange
{
    public class SceneChangeAni_Animator : SceneChangeAni
    {
        #region Inspector
        [SerializeField, TabGroup("Component"), LabelText("Animator")] private Animator m_Animator;
        [SerializeField, TabGroup("Option"), LabelText("Animator")] private string m_SceneChangeStartAni;
        [SerializeField, TabGroup("Option"), LabelText("Animator")] private string m_SceneChangeEndAni;
        #endregion
        #region Value
        private Coroutine m_EndAniTimeScaleCor;
        #endregion

        #region Event
        //SceneChangeAni Event
        protected override void OnAniStart()
        {
            m_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            m_Animator.Play(m_SceneChangeStartAni);
        }
        protected override void OnAniEnd()
        {
            m_Animator.updateMode = AnimatorUpdateMode.Normal;
            m_EndAniTimeScaleCor = StartCoroutine(EndAniTimeScaleCor());
            m_Animator.Play(m_SceneChangeEndAni);
        }

        //AnimationEvent
        public void OnStartAniEnd()
        {
            PostChange();
        }
        public void OnEndAniEnd()
        {
            if (m_EndAniTimeScaleCor != null)
                StopCoroutine(m_EndAniTimeScaleCor);
            PostEnd();
        }
        #endregion
        #region Function
        private IEnumerator EndAniTimeScaleCor()
        {
            while(true)
            {
                yield return new WaitForSeconds(0.1f);
                if (UnityEngine.Time.timeScale <= 0)
                {
                    m_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                    break;
                }
            }
            m_EndAniTimeScaleCor = null;
        }
        #endregion
    }
}