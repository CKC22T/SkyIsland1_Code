using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace CulterLib.Presets
{
    public class UIManager : LocalSingleton<UIManager>
    {
        #region Inspector
        [SerializeField, TabGroup("Component")] private Camera m_UICam;
        [SerializeField, TabGroup("Manager")] private PopupManager m_PopMgr;
        [SerializeField, TabGroup("Option")] private bool m_IsAutoInit = true;
        #endregion
        #region Get,Set
        public PopupManager PopMgr { get => m_PopMgr; }
        /// <summary>
        /// UI 카메라
        /// </summary>
        public Camera UICam { get => m_UICam; }
        #endregion

        #region Event
        public void Init()
        {   //UIManager 초기화
            OnInitMgr();

            //BasePopup 자동으로 열기
            foreach (var v in m_PopMgr.Popup)
                if (v.Value.IsBasePopup)
                    v.Value.Open();
        }

        //Unity Event
        private void Start()
        {
            if (m_IsAutoInit)
                Init();
        }

        //MonoSingleton Event
        protected override void Initialize()
        {
            base.Initialize();

            m_PopMgr.InitSingleton();
        }

        //UIManager Event
        /// <summary>
        /// 매니저들을 초기화합니다.
        /// </summary>
        protected virtual void OnInitMgr()
        {
            if (m_PopMgr)
                m_PopMgr.Init();
        }
        #endregion
        #region Function - Editor
#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button("Setup")]
        public virtual void Setup()
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.SetDirty(this);

            //매니저 및 컴포넌트 추가
            m_UICam = GetComponentInChildren<Camera>();
            m_PopMgr = GetComponentInChildren<PopupManager>();

            //캔버스에 카메라 추가
            foreach (var v in GetComponentsInChildren<Canvas>())
                if (!v.worldCamera)
                    v.worldCamera = m_UICam;
        }
#endif
        #endregion
    }
}