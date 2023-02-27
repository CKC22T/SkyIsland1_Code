using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using CulterLib.Utils;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace CulterLib.Types
{
    public class Mono_UI : Mono_Data
    {
        #region Inspector
        [Title("UIMono")]
        [TabGroup("AutoInit", "Component"), SerializeField, LabelText("자식 오브젝트"), ReadOnly] private Mono_UI[] m_ChildUIMono = new Mono_UI[0];

        [Title("UIMono")]
        [TabGroup("Option"), SerializeField, LabelText("추가 ID")] string m_AddID = "";
        [TabGroup("Option"), SerializeField, LabelText("부모에 의한 초기화 사용")] bool m_IsInitByPar = true;
        #endregion
        #region Get,Set
        /// <summary>
        /// 해당 UIMono 상위에 있는 Popup
        /// </summary>
        public UI.Popups.PopupWindow ParPopup
        {
            get
            {
                Mono_UI ui = this;
                while (ui != null)
                {
                    if (ui is UI.Popups.PopupWindow)
                        return ui as UI.Popups.PopupWindow;
                    ui = ui.ParUIMono;
                }
                return null;
            }
        }
        /// <summary>
        /// 해당 UIMono 상위에 있는 UIMono
        /// </summary>
        public Mono_UI ParUIMono { get; private set; }
        /// <summary>
        /// 해당 UIMono 하위에 있는 UIMono
        /// 아무런 UI가 없을 경우 null
        /// </summary>
        public IReadOnlyList<Mono_UI> ChildUIMono { get => m_ChildUIMonoList; }
        #endregion
        #region Value
        private List<Mono_UI> m_ChildUIMonoList;                         //현재 해당 UIMono 하위에 있는 UIMono들
        #endregion

        #region Event
        public void Init(Mono_UI _par, string _id = null)
        {
            ParUIMono = _par;

            //Mono_Data 초기화
            base.Init(_id);
        }

        //Mono_Data Event
        protected override string OnInitID(string _id)
        {
            return base.OnInitID(_id) + m_AddID;
        }
        protected override void OnInitData()
        {
            base.OnInitData();

            //변수 초기화
            if (m_ChildUIMono != null && 0 < m_ChildUIMono.Length)
                m_ChildUIMonoList = new List<Mono_UI>();

            //구성요소 초기화
            foreach (var v in m_ChildUIMono)
                AddChildUI(v, v.m_IsInitByPar);
        }

        //UIMono Event
        /// <summary>
        /// 최상위 부모가 열리면 호출됩니다.
        /// </summary>
        protected virtual void OnParentOpen() { }
        /// <summary>
        /// 언어 변경시 호출되는 이벤트
        /// </summary>
        /// <param name="_dummy"></param>
        protected virtual void OnUpdateLanguage() { }
        /// <summary>
        /// 자식 UIMono가 추가되었을 때 호출됩니다.
        /// </summary>
        /// <param name="_prefab"></param>
        /// <returns></returns>
        protected virtual void OnAddChild(Mono_UI _child) { }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// 자식오브젝트를 추가합니다.
        /// </summary>
        /// <param name="_child"></param>
        public void AddChildUI(Mono_UI _child, bool _isInit = true)
        {
            if (m_ChildUIMonoList == null)
                m_ChildUIMonoList = new List<Mono_UI>();

            if (_isInit)
                _child.Init(this, ID.Value);
            m_ChildUIMonoList.Add(_child);
            OnAddChild(_child);
        }
        /// <summary>
        /// 자식오브젝트를 제거합니다.
        /// </summary>
        /// <param name="_child"></param>
        public void RemoveChildUI(Mono_UI _child)
        {
            if (m_ChildUIMonoList != null)
                m_ChildUIMonoList.Remove(_child);
        }
        /// <summary>
        /// 자식 오브젝트를 생성합니다.  
        /// </summary>
        public Mono_UI SpawnChildUI_AddID(GameObject _prefab, string _addID)
        {
            var child = GameObjectUtil.Instantiate(_prefab, transform).GetComponent<Mono_UI>();
            child.m_AddID = _addID;
            SpawnChildUI(child);

            return child;
        }
        /// <summary>
        /// 자식 오브젝트를 생성합니다.  
        /// </summary>
        public Mono_UI SpawnChildUI_FullID(GameObject _prefab, string _fullID)
        {
            var child = GameObjectUtil.Instantiate(_prefab, transform).GetComponent<Mono_UI>();
            child.ID.Set(_fullID, false);
            SpawnChildUI(child);

            return child;
        }
      
        //Internal
        /// <summary>
        /// 언어를 업데이트합니다.
        /// </summary>
        /// <param name="_dummy"></param>
        internal void UpdateLanguage()
        {
            OnUpdateLanguage();

            if (ChildUIMono != null)
                foreach (var v in ChildUIMono)
                    v.UpdateLanguage();
        }
        /// <summary>
        /// 해당 UI모노의 최상위 부모가 열렸음을 알립니다.
        /// </summary>
        internal void PostParentOpen()
        {
            OnParentOpen();
            if (ChildUIMono != null)
                foreach (var v in ChildUIMono)
                    v.PostParentOpen();
        }

        //Private
        /// <summary>
        /// 자식 오브젝트 신규 생성시의 공동작업
        /// </summary>
        /// <param name="_child"></param>
        /// <param name="_isInit"></param>
        private void SpawnChildUI(Mono_UI _child, bool _isInit = true)
        {
            if (m_ChildUIMonoList == null)
                m_ChildUIMonoList = new List<Mono_UI>();

            if (_isInit)
                _child.Init(this, ID.Value);
            m_ChildUIMonoList.Add(_child);
            OnAddChild(_child);
        }
        #endregion
        #region Function - Editor
#if UNITY_EDITOR
        //Public
        [Sirenix.OdinInspector.Button("Setup")]
        public virtual void Setup()
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.SetDirty(this);

            //자식 오브젝트 설정
            var childList = new List<Mono_UI>();
            FindChildUIMono(childList, transform);
            SetChildUIMono(childList);
        }

        //Protected
        /// <summary>
        /// 해당 transform 하위의 자식 오브젝트를 찾습니다.
        /// </summary>
        /// <param name="childList"></param>
        /// <param name="tr"></param>
        protected void FindChildUIMono(List<Mono_UI> childList, Transform tr)
        {
            for (int i = 0; i < tr.childCount; ++i)
            {
                Transform child = tr.GetChild(i);
                var comp = child.GetComponents<Mono_UI>();
                if (0 < comp.Length)
                {
                    foreach (var v in comp)
                        if (v.m_IsInitByPar)
                            childList.Add(v);
                }
                else
                    FindChildUIMono(childList, child);
            }
        }
        /// <summary>
        /// 자식 오브젝트를 설정합니다.
        /// </summary>
        /// <param name="childList"></param>
        protected void SetChildUIMono(List<Mono_UI> childList)
        {
            m_ChildUIMono = childList.ToArray();
        }
#endif
        #endregion
    }
}