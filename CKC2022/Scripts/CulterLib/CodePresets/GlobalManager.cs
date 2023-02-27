using CulterLib.Global.Data;
using CulterLib.Global.SceneChange;
using CulterLib.Global.Sound;
using Sirenix.OdinInspector;
using UnityEngine;
using CulterLib.Global.Language;
using CulterLib.Global.Load;
using Utils;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace CulterLib.Presets
{
    public class InitializedDataContrainer : Singleton<InitializedDataContrainer>
    {
        /// <summary>
        /// 1(griffin) ~ 4(Derin). 처음 시작시 선택한 캐릭터 ID. 변경되면 안됨
        /// </summary>
        public readonly Notifier<int> SelectedCharacterID = new();
    }


    public class GlobalManager : MonoSingleton<GlobalManager>
    {
        #region Inspector
        [Title("필수 기능")]
        [TabGroup("Manager"), SerializeField] private SoundManager m_SoundMgr;                  //사운드
        [TabGroup("Manager"), SerializeField] private LanguageManager m_LangMgr;                //언어
        [TabGroup("Manager"), SerializeField] private SceneChangeManager m_SceneChangeMgr;      //씬변경
        [TabGroup("Manager"), SerializeField] private WebManager m_WebManager;                  //웹
        [TabGroup("Manager"), SerializeField] private WebSockManager m_WebSockManager;                  //웹소켓
        [TabGroup("Manager"), SerializeField] private UserManager m_UserManager;                //유저정보
        [TabGroup("Manager"), SerializeField] private MatchingManager m_MatchingManager;        //로그인
        [TabGroup("Manager"), SerializeField] private RoomManager m_RoomManager;        //대기실

        [Title("커스텀 시스템")]
        [TabGroup("Manager"), SerializeField] private GameDataManager m_DataMgr;                    //테이블 및 공용 세이브 데이터
        [TabGroup("Manager"), SerializeField] private GameLoadManager m_LoadMgr;                    //게임 시작 전 데이터 로드 처리

#if UNITY_EDITOR
        [TabGroup("Debug"), SerializeField] private bool m_IsGameScene;
#endif
        #endregion
        #region Get,Set
        public SoundManager SoundMgr { get => m_SoundMgr; }
        public LanguageManager LanguageMgr { get => m_LangMgr; }
        public SceneChangeManager SceneChangeMgr { get => m_SceneChangeMgr; }
        public WebManager WebMgr { get => m_WebManager; }
        public WebSockManager WebSockMgr { get => m_WebSockManager; }
        public UserManager UserMgr { get => m_UserManager; }
        public MatchingManager MatchingMgr { get => m_MatchingManager; }
        public RoomManager RoomMgr { get => m_RoomManager; }

        public GameDataManager DataMgr { get => m_DataMgr; }
        public GameLoadManager LoadMgr { get => m_LoadMgr; }
        #endregion

        #region Event
        protected override void Initialize()
        {
            base.Initialize();

            if (m_SceneChangeMgr)
                m_SceneChangeMgr.Init();

            if (m_DataMgr)
                m_DataMgr.Init();
            if (m_LoadMgr)
                m_LoadMgr.Init();

            if (m_SoundMgr)
                m_SoundMgr.Init();
            if (m_LangMgr)
                m_LangMgr.Init();

#if UNITY_EDITOR
            if (m_IsGameScene && m_LoadMgr)
                m_LoadMgr.Load_InitOnly();
#endif
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
            m_SoundMgr = GetComponentInChildren<SoundManager>();
            m_LangMgr = GetComponentInChildren<LanguageManager>();
            m_SceneChangeMgr = GetComponentInChildren<SceneChangeManager>();

            m_DataMgr = GetComponentInChildren<GameDataManager>();
            m_LoadMgr = GetComponentInChildren<GameLoadManager>();
        }
#endif
        #endregion
    }
}