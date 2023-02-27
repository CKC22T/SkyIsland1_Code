using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Global.SceneChange
{
    public abstract class SceneChangeAni : MonoBehaviour
    {
        #region Get,Set
        protected SceneChangeManager ParentMgr { get; private set; }
        #endregion

        #region Event
        public void Init(SceneChangeManager _parentMgr)
        {
            //변수 초기화
            ParentMgr = _parentMgr;

            //구성요소 초기화
            gameObject.SetActive(false);
        }

        //SceneChangeAni Event
        /// <summary>
        /// 씬 변경 시작 애니메이션을 출력해야 할 때 호출되는 이벤트입니다.
        /// </summary>
        protected abstract void OnAniStart();
        /// <summary>
        /// 씬변경 완료 애니메이션을 출력해야할 때 호출되는 이벤트입니다.
        /// </summary>
        protected abstract void OnAniEnd();
        #endregion
        #region Function
        //Public
        /// <summary>
        /// 씬변경 애니 시작!
        /// </summary>
        public void StartAni()
        {
            gameObject.SetActive(true);
            OnAniStart();
        }
        /// <summary>
        /// 씬변경 애니 끝!
        /// </summary>
        public void EndAni()
        {
            OnAniEnd();
        }

        //Protected
        /// <summary>
        /// 씬을 변경하라고 보낸다.
        /// </summary>
        protected void PostChange()
        {
            ParentMgr.OnChange();
        }
        /// <summary>
        /// 애니가 완전히 끝났다고 보낸다.
        /// </summary>
        protected void PostEnd()
        {
            gameObject.SetActive(false);
            ParentMgr.OnEnd();
        }
        #endregion
    }
}