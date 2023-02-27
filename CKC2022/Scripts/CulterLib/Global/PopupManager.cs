using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using CulterLib.Types;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace CulterLib.UI.Popups
{
    /// <summary>
    /// 팝업 관리기능이 포함된 클래스입니다.
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        #region Inspector
        [SerializeField, TabGroup("Component"), LabelText("기본 팝업")] private PopupWindow[] m_InitPopup;    //해당 매니저가 기본적으로 포함할 팝업 (미리 생성되어있는 팝업)

        [SerializeField, TabGroup("Option"), LabelText("UI 시작거리")] private float m_PopupDistanceStart = 10;     //팝업과 UI 거리 시작지점
        [SerializeField, TabGroup("Option"), LabelText("UI 추가거리")] private float m_PopupDistance = -1.0f;       //팝업간에 거리
        [SerializeField, TabGroup("Option"), LabelText("UI 시작오더")] private int m_PopupOrder = 1;                //팝업 시작 order
        #endregion
        #region Get,Set
        /// <summary>
        /// 초기화용으로 등록된 팝업들
        /// </summary>
        public IReadOnlyCollection<PopupWindow> InitPopup { get => m_InitPopup; }
        /// <summary>
        /// 매니저에 존재하는 팝업들
        /// </summary>
        public IReadOnlyDictionary<string, PopupWindow> Popup { get => m_Popup; }
        /// <summary>
        /// 현재 열려있는 팝업
        /// </summary>
        public IReadOnlyList<PopupWindow> OpenedPopup { get => m_OpenPopup; }
        #endregion
        #region Value
        private Dictionary<string, PopupWindow> m_Popup = new Dictionary<string, PopupWindow>();
        private List<PopupWindow> m_OpenPopup = new List<PopupWindow>();
        #endregion

        #region Event
        /// <summary>
        /// 팝업 싱글톤들을 초기화합니다.
        /// </summary>
        public void InitSingleton()
        {
            foreach (var v in m_InitPopup)
                v.InitSingleton();
        }
        /// <summary>
        /// 팝업을 초기화합니다.
        /// </summary>
        public void Init()
        {
            foreach (var v in m_InitPopup)
            {
                v.Init();
                m_Popup.Add(v.ID.Value, v);
            }
        }

        //UnityEvent
        private void Update()
        {
            //Esc 버튼을 누르면 가장 최근에 켠 팝업 종료
            if (0 < m_OpenPopup.Count && Input.GetKeyDown(KeyCode.Escape))
            {
                for (int i = m_OpenPopup.Count - 1; 0 <= i; --i)
                {
                    PopupWindow nowPopup = m_OpenPopup[i];
                    if (nowPopup.IsCloseByEscape)
                        nowPopup.Close();

                    if (!nowPopup.IsPassEscape)
                        break;
                }
            }
        }
        #endregion
        #region Function
        //Internal
        /// <summary>
        /// 해당 팝업을 넣습니다.
        /// </summary>
        /// <param name="popup">넣을 팝업</param>
        internal void PushPopup(PopupWindow popup)
        {
            //이미 열려있는 팝업이면 일단 없앤다.
            if (m_OpenPopup.Contains(popup))
                PopPopup(popup);

            //맨앞에 팝업 추가
            popup.SetSort(m_PopupDistanceStart + m_PopupDistance * m_OpenPopup.Count, m_PopupOrder + m_OpenPopup.Count);
            m_OpenPopup.Add(popup);
        }
        /// <summary>
        /// 해당 팝업을 없앱니다.
        /// </summary>
        /// <param name="popup">뺄 팝업</param>
        internal void PopPopup(PopupWindow popup)
        {
            for (int i = 0; i < m_OpenPopup.Count; ++i)
                if (m_OpenPopup[i] == popup)
                {
                    m_OpenPopup.RemoveAt(i);
                    UpdateSort();
                    return;
                }
        }
        /// <summary>
        /// 해당 팝업을 맨 앞으로 보냅니다.
        /// </summary>
        /// <param name="_pop"></param>
        internal void FocusPopup(PopupWindow _pop)
        {
            m_OpenPopup.Remove(_pop);
            m_OpenPopup.Add(_pop);
            UpdateSort();
        }
        /// <summary>
        /// 팝업 sort를 한번 업데이트해줍니다.
        /// </summary>
        internal void UpdateSort()
        {
            for (int i = 0; i < m_OpenPopup.Count; ++i)
                m_OpenPopup[i].SetSort(m_PopupDistanceStart + m_PopupDistance * i, m_PopupOrder + i);
        }
        #endregion
        #region Function - Editor
#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button("Setup")]
        public virtual void Setup()
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.SetDirty(this);

            //하위에 있는 팝업들 m_InitPopup에 추가
            m_InitPopup = GetComponentsInChildren<PopupWindow>();
        }
#endif
        #endregion
    }
}