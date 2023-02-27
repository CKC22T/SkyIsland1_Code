using Network.Server;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Game.Chr
{
    public class CharacterActionManager : MonoBehaviour
    {
        #region Inspector
        [SerializeField, TabGroup("Component"), LabelText("캐릭터 액션")] private CharacterAction[] m_Actions;
        [SerializeField, TabGroup("Component"), LabelText("기본 액션")] private CharacterAction[] m_DefaultActions;
        [SerializeField, TabGroup("Component"), LabelText("범위")] private CharacterActionDetector m_Detector;

        [SerializeField, TabGroup("Option"), LabelText("자동 초기화")] private bool m_IsAutoInit = true;
        #endregion
        #region Get,Set
        /// <summary>
        /// 초기화 여부
        /// </summary>
        public bool IsInited { get; private set; }
        /// <summary>
        /// 해당 캐릭터 Entity
        /// </summary>
        public MasterEntityData ParentEntity { get; private set; }
        /// <summary>
        /// 해당 캐릭터의 감지범위
        /// </summary>
        public CharacterActionDetector Detector { get => m_Detector; }
        /// <summary>
        /// 해당 캐릭터의 액션들중 ID값으로 접근 가능한 액션을 가져옵니다.
        /// </summary>
        public IReadOnlyDictionary<string, CharacterAction> IdActions { get => mIdActions; }
        /// <summary>
        /// 해당 캐릭터가 사용할 수 있는 액션을 가져옵니다.
        /// </summary>
        public IReadOnlyList<CharacterAction> ChildActions { get => m_ChildActions; }
        /// <summary>
        /// 현재 사용중인 해당 캐릭터의 액션들을 가져옵니다.
        /// </summary>
        public CharacterAction[] CurrentActions { get => m_CurrentActions; }
        #endregion
        #region Value
        [SerializeField, TabGroup("Debug"), LabelText("ID로 접근되는 액션"), ReadOnly] private Dictionary<string, CharacterAction> mIdActions = new Dictionary<string, CharacterAction>();
        [SerializeField, TabGroup("Debug"), LabelText("사용 가능한 액션"), ReadOnly] private List<CharacterAction> m_ChildActions;
        [SerializeField, TabGroup("Debug"), LabelText("현재 액션"), ReadOnly] private CharacterAction[] m_CurrentActions;
        #endregion

        #region Event
        public void Initialize(MasterEntityData parentEntity)
        {
            if (!IsInited)
            {   //최초 1회 초기화
                //변수 초기화
                ParentEntity = parentEntity;
                m_ChildActions = new List<CharacterAction>(m_Actions);
                m_CurrentActions = new CharacterAction[m_DefaultActions.Length];

                //기본 초기화
                foreach (var v in m_ChildActions)
                {
                    v.Initialize(this);
                    if (!string.IsNullOrEmpty(v.ID))
                        mIdActions.Add(v.ID, v);
                }
                for (int i = 0; i < m_DefaultActions.Length; ++i)
                    if (m_DefaultActions[i])
                    {   //기본 액션 플레이
                        m_CurrentActions[i] = m_DefaultActions[i];
                        m_CurrentActions[i]?.StartAction();
                    }

                //초기화 완료!
                IsInited = true;
            }
            else
            {   //중복 초기화
                for (int i = 0; i < m_DefaultActions.Length; ++i)
                    if (m_DefaultActions[i])
                        SetAction(m_DefaultActions[i], i);
            }
        }

        //Unity Event
        private void Awake()
        {
            if (m_IsAutoInit)
                Initialize(null);
        }
        protected virtual void Update()
        {
            //Action Update
            for (int i = 0; i < m_CurrentActions.Length; ++i)
                if (m_CurrentActions[i])
                {
                    var next = m_CurrentActions[i].UpdateAction();
                    if (next != m_CurrentActions[i])
                        SetAction(next, i);
                }
        }
        protected virtual void FixedUpdate()
        {
            //Action FixeUpdate
            for (int i = 0; i < m_CurrentActions.Length; ++i)
                if (m_CurrentActions[i])
                {
                    var next = m_CurrentActions[i].FixedUpdateAction();
                    if (next != m_CurrentActions[i])
                        SetAction(next, i);
                }
        }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// 액션을 설정합니다.
        /// </summary>
        /// <param name="_action"></param>
        public void SetAction(CharacterAction _action, int _layer = 0)
        {
            //액션 설정
            if (0 <= _layer && _layer < m_CurrentActions.Length && m_ChildActions.Contains(_action))
            {
                if (_action.IsUseEnable && m_CurrentActions[_layer] != _action)
                {
                    CharacterAction oldAction = m_CurrentActions[_layer];
                    m_CurrentActions[_layer] = _action;

                    oldAction?.EndAction();
                    m_CurrentActions[_layer].StartAction();
                }
            }
        }
        #endregion
    }
}
