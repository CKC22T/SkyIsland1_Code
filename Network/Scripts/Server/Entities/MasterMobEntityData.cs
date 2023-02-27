using CulterLib.Game.Chr;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Utils;

using Network.Packet;
using static Network.Packet.Response.Types;

namespace Network.Server
{
    public class MasterMobEntityData : MasterEntityData
    {
        // Attack
        [SerializeField] private DetectorType mDetectorType;
        [SerializeField] private int mAttackDamage;
        [SerializeField] private Transform mAttackPosition;

        // AI
        [TabGroup("Component"), SerializeField] NavMeshAgent mAgent;
        [TabGroup("Component"), SerializeField] CharacterPhysics mPhysics;
        [TabGroup("Component"), SerializeField] protected CharacterActionManager mActionManager;

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

        protected override void Awake()
        {
            base.Awake();
            mOnCreated += () =>
            {
                mActionManager.Initialize(this);
            };
        }

        #region Actions

        private List<BaseDetectorData> mCurrentAttackDetectors = new List<BaseDetectorData>();

        public override void ActionAttack(int attackCode, Action onDetected)
        {
            if (mCurrentAttackDetectors.IsEmpty() == false)
                return;

            DetectorInfo info = new DetectorInfo()
            {
                Origin = mAttackPosition.position,
                Direction = mAttackPosition.forward,
                OwnerCollider = mOwnerCollider,
                OwnerEntityID = EntityID,
                DamageInfo = new DamageInfo(mAttackDamage, FactionType)
            };

            var detector = ServerMasterDetectorManager.Instance.CreateNewDetector(mDetectorType, info);
            detector.SetParent(transform, onDetected);
            mCurrentAttackDetectors.Add(detector);
        }

        public void CancelAttack()
        {
            foreach (var detector in mCurrentAttackDetectors)
            {
                detector.ForceDestroy();
            }

            mCurrentAttackDetectors.Clear();
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
    }
}
