using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using CulterLib.Types;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.Collections.Generic;
using CulterLib.UI.Controls;
using System;
using CulterLib.Presets;

namespace CulterLib.UI.Popups
{
    /// <summary>
    /// 팝업 기본 클래스입니다.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class PopupWindow : Mono_UI
    {
        #region Type
        protected delegate IEnumerator ChangeAniCoroutine();
        #endregion

        #region Inspector
        [Title("PopupWindow")]
        [SerializeField, TabGroup("Component")] protected Canvas m_Canvas;                          //Canvas
        [SerializeField, TabGroup("Component")] protected Animator m_Animator;

        [Title("PopupWindow")]
        [SerializeField, TabGroup("Option"), LabelText("일반UI취급")] private bool m_IsBasePopup;                         //해당 팝업이 기본팝업인지
        [SerializeField, TabGroup("Option"), LabelText("Esc로 닫기")] private bool m_IsCloseByEscape;                     //Escape버튼을 사용해 해당 팝업을 종료 할 수 있는지
        [SerializeField, TabGroup("Option"), LabelText("Esc 넘기기")] private bool m_IsPassEscape;                        //Escape버튼 입력을 아래 팝업에 넘겨버릴지
        [SerializeField, TabGroup("Option"), LabelText("블로킹Popup 포커스")] private bool m_IsFocusBlockingPop = true;
        #endregion
        #region Get, Set
        /// <summary>
        /// Escape버튼을 사용해 해당 팝업을 종료 할 수 있는지 가져옵니다.
        /// </summary>
#if UNITY_EDITOR
        public bool IsCloseByEscape { get => (UiTestManager.Instance && !m_IsBasePopup) ? true : m_IsCloseByEscape; set => m_IsCloseByEscape = value; }

#else
        public bool IsCloseByEscape { get => m_IsCloseByEscape; protected set => m_IsCloseByEscape = value; }
#endif
        /// <summary>
        /// Escape 버튼 입력을 아래 팝업에 넘겨버릴지 가져옵니다.
        /// </summary>
        public bool IsPassEscape { get => m_IsPassEscape; protected set => m_IsPassEscape = value; }
        /// <summary>
        /// 해당 팝업이 기본팝업인지 (사실상 팝업이 아니라 일반UI처럼 취급됨)
        /// </summary>
        public bool IsBasePopup { get => m_IsBasePopup; protected set => m_IsBasePopup = value; }

        /// <summary>
        /// 해당 팝업이 켜지기 시작하면 호출되는 이벤트
        /// </summary>
        public Action<PopupWindow> OnOpenStartFunc { get; set; }
        /// <summary>
        /// 해당 팝업이 완전히 켜지면 호출되는 이벤트
        /// </summary>
        public Action<PopupWindow> OnOpenEndFunc { get; set; }
        /// <summary>
        /// 해당 팝업이 꺼지기 시작하면 호출되는 이벤트
        /// </summary>
        public Action<PopupWindow> OnCloseStartFunc { get; set; }
        /// <summary>
        /// 해당 팝업이 완전히 꺼지면 호출되는 이벤트
        /// </summary>
        public Action<PopupWindow> OnCloseEndFunc { get; set; }
        #endregion

        #region Event
        /// <summary>
        /// 싱글톤을 초기화합니다.
        /// </summary>
        public void InitSingleton()
        {
            OnInitSingleton();
        }
        /// <summary>
        /// 팝업을 초기화합니다.
        /// </summary>
        /// <param name="_mgr">해당 팝업을 포함하는 매니저입니다.</param>
        /// <param name="_uiCam">해당 팝업을 렌더링할 카메라입니다.</param>
        public void Init()
        {
            try
            {
                //구성요소 초기화
                if (m_Canvas.worldCamera == null)
                    m_Canvas.worldCamera = UIManager.Instance.UICam;
                gameObject.SetActive(false);

                //기본 초기화
                base.Init();

                //이벤트 초기화
                AddDataChangeEvent(GlobalManager.Instance.LanguageMgr.Now, (dummy) => UpdateLanguage());
            }
            catch (Exception e)
            {
                Debug.LogError($"PopupManager.InitPopup Error (popup name : {gameObject.name}) {e.Message}\n{e.StackTrace}");
            }
        }

        //Popup Event
        /// <summary>
        /// 싱글톤을 설정할 때 호출되는 이벤트입니다.
        /// </summary>
        protected virtual void OnInitSingleton() { }
        /// <summary>
        /// 팝업이 열리기 시작 할 때 호출되는 이벤트입니다.
        /// </summary>
        protected virtual void OnStartOpen(string _opt) { }
        /// <summary>
        /// 팝업이 열린 직후 호출되는 이벤트입니다.
        /// </summary>
        protected virtual void OnEndOpen() { }
        /// <summary>
        /// 팝업이 닫히기 시작 할 때 호출되는 이벤트입니다.
        /// </summary>
        protected virtual void OnStartClose() { }
        /// <summary>
        /// 팝업이 종료 된 직후 호출되는 이벤트입니다.
        /// </summary>
        protected virtual void OnEndClose() { }

        //Animation Event
        public void OnOpenAniEnd()
        {
            EndOpen();
        }
        public void OnCloseAniEnd()
        {
            EndClose();
        }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// 팝업 열기
        /// </summary>
        /// <param name="option">텍스트 형태의 추가로 넘길 데이터</param>
        public void Open(string option = null)
        {
            //열기
            UIManager.Instance.PopMgr.PushPopup(this);
            gameObject.SetActive(true);

            PostParentOpen();
            OnStartOpen(option);
            OnOpenStartFunc?.Invoke(this);

            if (m_Animator)
                m_Animator.Play("OpenAni");
            else
                EndOpen();

            if (m_IsFocusBlockingPop && BlockingPopup.Instance && BlockingPopup.Instance.gameObject.activeSelf)
                BlockingPopup.Instance.Focus();
        }
        /// <summary>
        /// 팝업 닫기
        /// </summary>
        public void Close()
        {
            //이미 닫혀있으면 무시
            if (!gameObject.activeSelf)
                return;

            OnStartClose();
            OnCloseStartFunc?.Invoke(this);

            if (m_Animator)
                m_Animator.Play("CloseAni");
            else
                EndClose();
        }
        /// <summary>
        /// 해당 팝업을 맨 앞으로 보냅니다.
        /// </summary>
        public void Focus()
        {
            UIManager.Instance.PopMgr.FocusPopup(this);
        }

        //Internal
        /// <summary>
        /// 캔버스의 sortingOrder, planeDistance 값 설정
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="order"></param>
        internal void SetSort(float distance, int order)
        {
            m_Canvas.planeDistance = distance;
            m_Canvas.sortingOrder = order;
        }

        //Private
        /// <summary>
        /// 팝업 열기를 마무리합니다.
        /// </summary>
        private void EndOpen()
        {
            OnEndOpen();
            OnOpenEndFunc?.Invoke(this);
        }
        /// <summary>
        /// 팝업 닫기를 마무리합니다.
        /// </summary>
        private void EndClose()
        {
            UIManager.Instance.PopMgr.PopPopup(this);
            gameObject.SetActive(false);

            OnEndClose();
            OnCloseEndFunc?.Invoke(this);
        }
        #endregion
        #region Function - Editor
#if UNITY_EDITOR
        public override void Setup()
        {
            base.Setup();

            //자식 오브젝트 설정
            var childList = new List<Mono_UI>();
            void FindDeep(Transform tr)
            {
                var deepList = new List<Mono_UI>();
                FindChildUIMono(deepList, tr);
                foreach (var v in deepList)
                {
                    if (v.GetComponent<Control_RootFrame>())
                        FindDeep(v.transform);
                    childList.Add(v);
                }
            }
            FindDeep(transform);
            SetChildUIMono(childList);

            //변수 설정
            if (!m_Canvas)
                m_Canvas = GetComponent<Canvas>();
            if (!m_Animator)
                m_Animator = GetComponent<Animator>();

            m_Canvas.planeDistance = 10;
        }
#endif
        #endregion
    }
}