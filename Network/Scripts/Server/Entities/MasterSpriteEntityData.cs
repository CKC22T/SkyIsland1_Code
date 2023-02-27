using CulterLib.Game.Chr;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Utils;

using Network.Packet;
using static Network.Packet.Response.Types;
using System.Collections;

namespace Network.Server
{
    public enum SpriteAttackType
    {
        None,
        FloorAttack,
        RockWaveAttack,
        StoneCone_1_Attack,
        StoneCone_2_Attack,
    }

    public class MasterSpriteEntityData : MasterEntityData
    {
        struct DamageSave
        {
            public float time;
            public int damage;
            public DamageSave(float _time, int _damage)
            {
                time = _time;
                damage = _damage;
            }
        }

        // Attack
        [SerializeField] private Dictionary<SpriteAttackType, DetectorType> mAttackTypeInfoTable;
        [SerializeField] private int mAttackDamage;
        [SerializeField] private Transform mAttackPosition;

        [SerializeField] private DetectorType mFloorAttackDetectorType;

        public Vector3 FloorAttackPosition => mAttackPosition.position;

        // AI
        [TabGroup("Component"), SerializeField] NavMeshAgent mAgent;
        [TabGroup("Component"), SerializeField] CharacterPhysics mPhysics;
        [TabGroup("Component"), SerializeField] CharacterActionManager mActionManager;

        [TabGroup("Option"), SerializeField] float mDamageSaveTime; //해당 시간동안 입은 데미지가 저장됨
        [TabGroup("Option"), SerializeField] float mDamagedDamge;   //저장된 데미지가 해당 값 이상이면 Damaged상태로 변경됨

#if UNITY_EDITOR
        [TabGroup("Component"), SerializeField] Animator mAnimatorForTest;
#endif

        // Physics
        /// <summary>
        /// 해당 캐릭터의 Collider
        /// </summary>
        public Collider OwnerCollider { get => mOwnerCollider; }

        /// <summary>
        /// 해당 캐릭터의 CharacterPhysics
        /// </summary>
        public CharacterPhysics Physics { get => mPhysics; }

        /// <summary>
        /// 해당 캐릭터의 NavMeshAgent
        /// </summary>
        public NavMeshAgent Agent { get => mAgent; }
        /// <summary>
        /// 페이즈가 변경되서 2페이즈인지
        /// </summary>
        public bool IsPhaseChanged { get; private set; }

        private int mLastedHP;
        private List<DamageSave> mDamageSave = new List<DamageSave>();

        private CoroutineWrapper mCoroutineWapper;

        protected override void Awake()
        {
            base.Awake();

            if (mCoroutineWapper == null)
            {
                mCoroutineWapper = new CoroutineWrapper(CoroutineWrapper.CoroutineRunner.Instance);
            }

            mOnCreated += () =>
            {
                mActionManager.Initialize(this);
                mDestroyEvent += callEvents;
            };
        }

        private void Start()
        {
            mOnCreated += () =>
            {
                mDestroyEvent += callEvents;
            };

            Hp.OnChanged += () =>
            {   //입은 데미지 저장, 만약 합계가 일정 이상일 경우 Damaged상태로 변경
                if (mActionManager.CurrentActions[0] != mActionManager.IdActions["PhaseChange"] && mActionManager.CurrentActions[0] != mActionManager.IdActions["Damaged"] && Hp.Value < mLastedHP)
                {
                    mDamageSave.Add(new DamageSave(Time.time, mLastedHP - Hp.Value));
                    mDamageSave.RemoveAll((item) => item.time < Time.time - mDamageSaveTime);
                    int total = 0;
                    foreach (var v in mDamageSave)
                        total += v.damage;
                    if (mDamagedDamge < total)
                    {
                        mActionManager.SetAction(mActionManager.IdActions["Damaged"]);
                    }
                }

                mLastedHP = Hp.Value;
            };
        }
        #region Actions

        public void ActionFreeze(float freezingTime)
        {
            mCoroutineWapper.StartSingleton(freezing());

            IEnumerator freezing()
            {
                mActionManager.SetAction(mActionManager.IdActions["Freeze"]);
                yield return new WaitForSeconds(freezingTime);
                mActionManager.SetAction(mActionManager.IdActions["Idle"]);
            }
        }

        public void ActionStoneAttack(SpriteAttackType attackType, Vector3 attackPosition, Quaternion attackRotation)
        {
            switch (attackType)
            {
                case SpriteAttackType.FloorAttack:
                    createFloorAttack(attackPosition, Vector3.forward);
                    break;

                case SpriteAttackType.RockWaveAttack:
                    createAttackEntity(EntityType.kStructureBossWaveRock, attackPosition, attackRotation);
                    break;

                case SpriteAttackType.StoneCone_1_Attack:
                    createAttackEntity(EntityType.kStructureBossStoneConePhase_1, attackPosition, attackRotation);
                    break;

                case SpriteAttackType.StoneCone_2_Attack:
                    createAttackEntity(EntityType.kStructureBossStoneConePhase_2, attackPosition, attackRotation);
                    break;
            }
        }

        private void createAttackEntity(EntityType entityType, Vector3 attackPosition, Quaternion attackRotation)
        {
            if (ServerMasterEntityManager.TryGetInstance(out var manager))
            {
                var attack = manager.CreateNewEntity(
                    entityType,
                    FactionType,
                    attackPosition,
                    attackRotation,
                    true) as MasterBossStoneConeData;

                attack.ActionInitialAttack();
            }
        }

        private void createFloorAttack(Vector3 attackPosition, Vector3 attackDirection)
        {
            DetectorInfo info = new DetectorInfo()
            {
                Origin = attackPosition,
                Direction = attackDirection,
                OwnerCollider = mOwnerCollider,
                OwnerEntityID = EntityID,
                DamageInfo = new DamageInfo(mAttackDamage, FactionType)
            };

            ServerMasterDetectorManager.Instance.CreateNewDetector(mFloorAttackDetectorType, info);
        }

        #endregion

        #region Animation Synchronize

        //Public - Replication
        /// <summary>
        /// Animator.SetTrigger를 호출합니다.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void SetAnimatorTrigger(string id)
        {
#if UNITY_EDITOR
            if (mAnimatorForTest)
                mAnimatorForTest.SetTrigger(id);
#endif
            //TODO : SetTrigger 동기화
            var data = GetBaseEntityActionData(EntityAction.kAnimation)
                .SetAnimationType(AnimationType.kTrigger)
                .SetAnimationId(id);

            TcpEntityActionDataBuffer.Add(data.Build());
        }

        /// <summary>
        /// Animator.SetBool을 호출합니다.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void SetAnimatorBool(string id, bool value)
        {
#if UNITY_EDITOR
            if (mAnimatorForTest)
                mAnimatorForTest.SetBool(id, value);
#endif
            //TODO : SetBool 동기화
            var data = GetBaseEntityActionData(EntityAction.kAnimation)
                .SetAnimationType(AnimationType.kBool)
                .SetAnimationId(id)
                .SetAnimationBoolValue(value);

            TcpEntityActionDataBuffer.Add(data.Build());
        }

        /// <summary>
        /// Animator.SetBool을 호출합니다.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void SetAnimatorInt(string id, int value)
        {
#if UNITY_EDITOR
            if (mAnimatorForTest)
                mAnimatorForTest.SetInteger(id, value);
#endif
            //TODO : SetInteger 동기화
            var data = GetBaseEntityActionData(EntityAction.kAnimation)
                .SetAnimationType(AnimationType.kInt)
                .SetAnimationId(id)
                .SetAnimationIntValue(value);

            TcpEntityActionDataBuffer.Add(data.Build());
        }

        /// <summary>
        /// Animator.SetBool을 호출합니다.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void SetAnimatorFloat(string id, float value)
        {
#if UNITY_EDITOR
            if (mAnimatorForTest)
                mAnimatorForTest.SetFloat(id, value);
#endif
            //TODO : SetFloat 동기화
            var data = GetBaseEntityActionData(EntityAction.kAnimation)
                .SetAnimationType(AnimationType.kFloat)
                .SetAnimationId(id)
                .SetAnimationFloatValue(value);

            TcpEntityActionDataBuffer.Add(data.Build());
        }
        /// <summary>
        /// 기타 특수 애니메이션 이벤트를 호출합니다.
        /// </summary>
        /// <param name="id"></param>
        public void SetAnimationOther(string id)
        {
            var data = GetBaseEntityActionData(EntityAction.kAnimation)
               .SetAnimationType(AnimationType.kOther)
               .SetAnimationId(id);

            TcpEntityActionDataBuffer.Add(data.Build());
        }

        #endregion

        [SerializeField] private List<BaseLocationEventTrigger> OnActionDieEventList;

        public void SetDieTriggerEvents(List<BaseLocationEventTrigger> dieEventList)
        {
            OnActionDieEventList = dieEventList;
        }

        private void callEvents(int entityID)
        {
            if (OnActionDieEventList == null)
                return;

            foreach (var e in OnActionDieEventList)
            {
                e.TriggeredEvent(null);
            }
        }

        public bool TryGetChangePhase(out CharacterAction _action)
        {
            if (!IsPhaseChanged && Hp.Value < MaxHp / 2)
            {
                IsPhaseChanged = true;
                _action = mActionManager.IdActions["PhaseChange"];
                return true;
            }
            else
            {
                _action = null;
                return false;
            }
        }
    }
}