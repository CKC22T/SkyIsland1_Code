using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Utils;
using Network.Packet;
using AnimationEvent = CKC2022.AnimationEvent;

namespace Network.Client
{
    [System.Obsolete("Legacy. if Humanoid entity, use ReplicatedHumanoidEntityActor instead.", true)]
    public class ReplicatedEntityActor : MonoBehaviour
    {
        [SerializeField]
        private ReplicatedEntityData replicatedData;

        [SerializeField]
        private Transform ModelRoot;

        [SerializeField]
        private Animator animator;

        private AnimationEvent animationEventRiser;

        private void Awake()
        {
            if (replicatedData.EntityType.GetEntityBaseType() == EntityBaseType.Humanoid)
            {
                replicatedData.IsAlive.OnDataChanged += IsAlive_OnDataChanged;
            }

            replicatedData.OnAction += ReplicatedData_OnAction;
            replicatedData.OnHitAction += ReplicatedData_OnHitAction;

            animationEventRiser = GetComponentInChildren<AnimationEvent>();
            if (animationEventRiser != null)
            {
                animationEventRiser.OnDissolveComplete += AnimationEventRiser_OnDeadComplete;
            }

        }

        private void AnimationEventRiser_OnDeadComplete()
        {
            //desolve

            //remove?
            PoolManager.ReleaseObject(replicatedData.gameObject);
        }

        private void IsAlive_OnDataChanged(bool isAlive)
        {
            if (isAlive)
            {
                animator.SetTrigger("revival");
            }

            else
            {
                animator.SetTrigger("death");

                animator.SetFloat("moveLocalX", 0);
                animator.SetFloat("moveLocalZ", 0);
            }
        }

        private void ReplicatedData_OnHitAction(ReplicatedDetectedInfo info)
        {
            //TODO : HitEffect or KnockBack

            //var instance = PoolManager.SpawnObject(info.DetectorInfo.effectOrigin, transform.position, Quaternion.LookRotation(info.DetectorInfo.Origin - transform.position));
        }

        private void ReplicatedData_OnAction(Response.Types.EntityActionData obj)
        {

            switch (obj.Action)
            {
                case Response.Types.EntityAction.kDestroy:
                    break;

                //case Response.Types.EntityAction.kDie: animator.SetTrigger("death"); break;
                //case Response.Types.EntityAction.kRevive: animator.SetTrigger("revival"); break;
                case Response.Types.EntityAction.kUseWeapon: animator.SetTrigger("attack"); break;
                case Response.Types.EntityAction.kEquipWeapon: animator.SetTrigger("pickUp"); break;

                case Response.Types.EntityAction.kUnequipWeapon:
                    break;

                default:
                    break;
            }

        }

        private void LateUpdate()
        {
            if (!replicatedData.IsMine && replicatedData.IsEnabled.Value)
            {
                if (replicatedData.Velocity.GetDirtyAndClear(out var direction))
                {
                    var localDirection = ModelRoot.InverseTransformDirection(direction);
                    animator.SetFloat("moveLocalX", localDirection.x);
                    animator.SetFloat("moveLocalZ", localDirection.y);
                }
            }
        }

        private void OnDisable()
        {
        }

    }
}