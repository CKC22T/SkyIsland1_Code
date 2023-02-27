using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using CulterLib.Types;
using CulterLib.UI.Popups;
#if UNITY_EDITOR
using UnityEditor.Events;
using UnityEditor.SceneManagement;
#endif

namespace CulterLib.UI.Controls
{
    public class Control_RootFrame : Mono_UI
    {
        #region Inspector
        [SerializeField, TabGroup("Component"), LabelText("RootFrame - 종료 버튼")] private Control_Button m_CloseButton;
        #endregion
        #region Get,Set
        /// <summary>
        /// 해당 프레임에 달려있는 닫기버튼
        /// </summary>
        public Control_Button CloseBtn { get => m_CloseButton; }

        //Protected
        /// <summary>
        /// 해당 프레임의 대상 팝업입니다.
        /// </summary>
        protected PopupWindow ParPop { get => ParUIMono as PopupWindow; }
        #endregion

        #region Event
        protected override void OnInitData()
        {
            base.OnInitData();

            //이벤트 초기화
            if (m_CloseButton)
                m_CloseButton.OnBtnClickFunc += (btn) =>
                {   //종료 버튼을 누르면 해당 프레임을 갖고있는 팝업이 종료된다.
                    if (ParPop)
                        ParPop.Close();
                };
        }
        #endregion
        #region Function - Editor
#if UNITY_EDITOR
        public override void Setup()
        {
            //루트는 하위에 오브젝트가 많으므로 문제생기는걸 막기위해서 셋업이 없다. 
        }
#endif
        #endregion
    }
}