using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CulterLib.Global.SceneChange
{
    public class SceneChangeManager : MonoBehaviour
    {
        #region Inspector
        [SerializeField, TabGroup("Component"), LabelText("씬변경 애니메이션")] private SceneChangeAni[] m_SceneChangeAni;
        #endregion
        #region Value
        private SceneChangeAni m_CurAni;
        private string m_TargetSceneID;         //변경할 씬
        private string m_NextTargetSceneID;     //예약된 변경할 씬
        private int m_NextAniIndex;             //예약된 애니메이션
        #endregion

        #region Event
        //UnityEvent
        public void Init()
        {
            //컴포넌트 초기화
            foreach (var ani in m_SceneChangeAni)
                ani.Init(this);
        }

        //SceneChangeAni Event
        internal void OnChange()
        {
            //예약된 씬변경이 있으면 해당씬으로 변경
            if (!string.IsNullOrEmpty(m_NextTargetSceneID))
            {
                m_TargetSceneID = m_NextTargetSceneID;
                m_NextTargetSceneID = null;
            }

            //씬변경
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => m_CurAni?.EndAni();
            SceneManager.LoadScene(m_TargetSceneID, LoadSceneMode.Single);
        }
        internal void OnEnd()
        {
            //씬변경 완료
            m_CurAni = null;

            //예약된 씬변경이 있으면 바로 실행
            if (!string.IsNullOrEmpty(m_NextTargetSceneID))
            {
                SceneChange(m_NextTargetSceneID, m_NextAniIndex);
                m_NextTargetSceneID = null;
            }
        }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// 씬을 변경합니다.
        /// </summary>
        /// <param name="_nextScene">넘어갈 씬의 이름</param>
        /// <param name="_aniIndex">애니메이션 인덱스</param>
        public void SceneChange(string _nextScene, int _aniIndex = 0)
        {
            //씬변경 도중인 경우 예약만 한다.
            if (m_CurAni)
            {
                m_NextTargetSceneID = _nextScene;
                m_NextAniIndex = _aniIndex;
                return;
            }

            //변수 설정
            m_TargetSceneID = _nextScene;
            m_CurAni = m_SceneChangeAni[_aniIndex];
            //씬변경 시작
            m_CurAni.StartAni();
        }
        #endregion
    }
}