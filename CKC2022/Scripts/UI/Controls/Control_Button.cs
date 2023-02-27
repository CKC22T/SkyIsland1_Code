using CulterLib.Types;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using CulterLib.Presets;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace CulterLib.UI.Controls
{
    [RequireComponent(typeof(Button))]
    public class Control_Button : Mono_UI
    {
        #region Type
        public enum EBtnType
        {
            /// <summary>
            /// 기본 작동 없음
            /// </summary>
            None,
            /// <summary>
            /// 인터넷 링크
            /// </summary>
            URL,
            /// <summary>
            /// 해당 ID의 팝업 열기
            /// </summary>
            Popup,
            /// <summary>
            /// 해당 링크의 씬으로 이동
            /// </summary>
            Scene,
            /// <summary>
            /// 팝업 끄기
            /// </summary>
            Close,
        }
        #endregion

        #region Inspector
        [Title("Control_Button")]
        [TabGroup("Component"), SerializeField, LabelText("Control_Button - 버튼")] private Button m_Button;

        [Title("Control_Button")]
        [TabGroup("Option"), SerializeField, LabelText("버튼타입")] private EBtnType m_Type;
        [TabGroup("Option"), SerializeField, LabelText("버튼링크"), ShowIf("IsShowLink")] private string m_Link;
        [TabGroup("Option"), SerializeField, LabelText("버튼옵션"), ShowIf("IsShowOption")] private string m_Option;
        [TabGroup("Option"), SerializeField, LabelText("클릭음")] private AudioClip m_ClickSE;
        [TabGroup("Option"), SerializeField, LabelText("로그 사용")] private bool m_IsLog;
        #endregion
        #region Get,Set
        /// <summary>
        /// 버튼
        /// </summary>
        public Button Tar { get => m_Button; }
        /// <summary>
        /// 버튼 링크
        /// </summary>
        public string Link { get => m_Link; set => m_Link = value; }
        /// <summary>
        /// 버튼 옵션
        /// </summary>
        public string Option { get => m_Option; set => m_Option = value; }
        /// <summary>
        /// 버튼 클릭시 이벤트
        /// </summary>
        public Action<Control_Button> OnBtnClickFunc { get; set; }
        /// <summary>
        /// 해당 버튼이 상호작용 가능한 상태인지
        /// </summary>
        public bool interactable { get => m_Button.interactable; set => m_Button.interactable = value; }

        //Inspector
        public bool IsShowLink { get => m_Type != EBtnType.None && m_Type != EBtnType.Close; }
        public bool IsShowOption { get => m_Type == EBtnType.Popup; }
        #endregion

        #region Event
        //UI Event
        public virtual void OnBtnClick()
        {
            OnBtnClickFunc?.Invoke(this);
            GlobalManager.Instance.SoundMgr.PlayUI(transform.position, m_ClickSE);

            //기본 작동
            switch(m_Type)
            {
                case EBtnType.URL:
                    Application.OpenURL(m_Link);
                    break;
                case EBtnType.Popup:
                    if (UIManager.Instance.PopMgr.Popup.TryGetValue(m_Link, out var pop))
                        pop.Open(m_Option);
                    break;
                case EBtnType.Scene:
                    GlobalManager.Instance.SceneChangeMgr.SceneChange(m_Link);
                    break;
                case EBtnType.Close:
                    ParPopup?.Close();
                    break;
            }
           
        }
        #endregion
        #region Function - Editor
#if UNITY_EDITOR
        public override void Setup()
        {
            base.Setup();

            m_Button = GetComponent<Button>();
            if (m_Button)
            {
                int persistentCount = m_Button.onClick.GetPersistentEventCount();
                for (int i = 0; i < persistentCount; ++i)
                    UnityEventTools.RemovePersistentListener(m_Button.onClick, 0);
                UnityEventTools.AddPersistentListener(m_Button.onClick, new UnityAction(OnBtnClick));
            }
        }
#endif
        #endregion
    }
}