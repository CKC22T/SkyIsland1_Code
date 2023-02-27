using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Game.Chr
{
    /// <summary>
    /// 캐릭터의 행동을 정의하는 클래스
    /// </summary>
    public abstract class CharacterAction : MonoBehaviour
    {
        #region Inspector
        [Title("CharacterAction")]
        [TabGroup("Option"), SerializeField] private string mID;
        #endregion
        #region Get,Set
        /// <summary>
        /// 해당 액션의 ID
        /// </summary>
        public string ID { get { return mID; } }
        /// <summary>
        /// 해당 액션을 사용하고 있는 캐릭터
        /// </summary>
        public CharacterActionManager ParentManager { get; private set; }
        /// <summary>
        /// 해당 액션이 현재 사용가능한 상태인지 여부
        /// </summary>
        public virtual bool IsUseEnable { get => true; }

        //숏컷
        /// <summary>
        /// 몹 Entity 숏컷
        /// </summary>
        public MasterMobEntityData ParentMob { get => ParentManager.ParentEntity as MasterMobEntityData; }
        /// <summary>
        /// 휴머노이드 Entity 숏컷
        /// </summary>
        public MasterHumanoidEntityData ParentHuman { get => ParentManager.ParentEntity as MasterHumanoidEntityData; }
        #endregion

        #region Event
        /// <summary>
        /// 초기화합니다.
        /// </summary>
        /// <param name="_parent"></param>
        public void Initialize(CharacterActionManager _parent)
        {
            ParentManager = _parent;

            OnInit();
        }

        //CharacterAction Event
        /// <summary>
        /// 초기화시 호출되는 이벤트입니다.
        /// </summary>
        protected virtual void OnInit()
        {
        }
        /// <summary>
        /// 액션이 시작되었을 때 호출됩니다.
        /// </summary>
        protected virtual void OnStartAction()
        {
        }
        /// <summary>
        /// 액션이 업데이트 될 때 호출됩니다.
        /// </summary>
        /// <returns>다음 재생될 액션(기본 : 자기자신 리턴)</returns>
        protected virtual CharacterAction OnUpdate()
        {
            return this;
        }
        /// <summary>
        /// 액션이 FixedUpdate 될 때 호출됩니다.
        /// </summary>
        /// <returns>다음 재생될 액션(기본 : 자기자신 리턴)</returns>
        protected virtual CharacterAction OnFixedUpdate()
        {
            return this;
        }
        /// <summary>
        /// 액션이 끝났을 때 호출됩니다.
        /// </summary>
        protected virtual void OnEndAction()
        {
        }
        #endregion
        #region Function
        //Internal
        /// <summary>
        /// 액션을 시작합니다.
        /// </summary>
        /// <param name="character">해당 액션을 시작한 캐릭터</param>
        internal void StartAction()
        {
            OnStartAction();
        }
        /// <summary>
        /// 액션을 업데이트합니다.
        /// </summary>
        /// <returns>다음 재생될 액션(기본 : 자기자신 리턴)</returns>
        internal CharacterAction UpdateAction()
        {
            return OnUpdate();
        }
        /// <summary>
        /// 액션을 FixedUpdate합니다.
        /// </summary>
        /// <returns></returns>
        internal CharacterAction FixedUpdateAction()
        {
            return OnFixedUpdate();
        }
        /// <summary>
        /// 액션을 끝냅니다.
        /// </summary>
        internal void EndAction()
        {
            OnEndAction();
        }
        #endregion
    }
}