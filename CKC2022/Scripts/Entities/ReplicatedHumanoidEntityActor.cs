using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Utils;
using CKC2022;
using Network.Packet;
using AnimationEvent = CKC2022.AnimationEvent;
using CKC2022.Input;

namespace Network.Client
{
    public class ReplicatedHumanoidEntityActor : MonoBehaviour
    {
        [SerializeField]
        private ReplicatedEntityData replicatedData;
        [SerializeField]
        private ReplicatedHumanoidEntityInteractor replicatedInteractor;

        private Notifier<InputContainer> InputContainer => replicatedInteractor.InputContainer;

        [SerializeField]
        private Transform ModelRoot;

        [SerializeField]
        private HumanoidAnimationController animator;

        private AnimationEvent animationEventRiser;
        private Rigidbody rigid;

        private void Awake()
        {
            if (replicatedData.EntityType.GetEntityBaseType() != EntityBaseType.Humanoid)
            {
                Debug.LogError($"Wrong Entity Component Exist. ReplicatedHumanoidEntityActor will be removed. Entity Type is {replicatedData.EntityType}");
                Destroy(this);
                return;
            }

            InputContainer.OnDataChangedDelta += Input_OnDataChangedDelta;

            replicatedData.BindedClientID.OnDataChanged += BindedClientID_OnDataChanged;
            replicatedData.IsAlive.OnDataChanged += IsAlive_OnDataChanged;
            replicatedData.OnAction += ReplicatedData_OnAction;
            replicatedData.OnHitAction += ReplicatedData_OnHitAction;

            animator.LookAtWeight.OnDataChanged += LookAtWeight_OnDataChanged;

            animationEventRiser = GetComponentInChildren<AnimationEvent>();
            rigid = GetComponent<Rigidbody>();
            if (animationEventRiser != null)
            {
                animationEventRiser.OnDissolveComplete += AnimationEventRiser_OnDeadComplete;
            }
        }

        private void LookAtWeight_OnDataChanged(float weight)
        {
            replicatedInteractor.LookAtWeight = weight;
        }

        private void BindedClientID_OnDataChanged(int clientID)
        {
            if (replicatedData.IsMine)
            {
                if (PlayerInputNetworkManager.TryGetInputContainer(ClientSessionManager.Instance.SessionID, out var container))
                    InputContainer.Value = container;
            }
            else
            {
                if (InputContainer.Value != null && InputContainer.Value.Input != null && InputContainer.Value.Input.PlayerID != clientID)
                    InputContainer.Value = null;
            }
        }

        private void Input_OnDataChangedDelta(InputContainer prev, InputContainer current)
        {
            if (prev != null)
            {
                prev.OnAttack -= Attack_OnDataChanged;
                prev.OnWeaponSelected -= OnWeaponSelected;
            }

            if (current != null)
            {
                current.OnAttack += Attack_OnDataChanged;
                current.OnWeaponSelected += OnWeaponSelected;
            }
        }

        private void OnWeaponSelected(InputContainer container, ReplicableItemObject data)
        {
            animator.SetPickUp();
        }

        //predict by client
        private void IsAlive_OnDataChanged(bool isAlive)
        {
            if (isAlive)
            {
                animator.SetRevival();
                
                rigid.isKinematic = false;
            }

            else
            {
                animator.SetDeath();
                animator.SetLocalMovement(Vector2.zero);

                rigid.isKinematic = true;
            }
        }


        private void Attack_OnDataChanged(InputContainer container, bool isDown)
        {
            if (isDown)
            {
                //animator.
                animator.SetAttack(weaponType: GetEquipedWeaponType(replicatedData));
            }
        }



        private void ReplicatedData_OnHitAction(ReplicatedDetectedInfo info)
        {
            //TODO : HitEffect or KnockBack
            
            //var instance = PoolManager.SpawnObject(info.DetectorInfo.effectOrigin, transform.position, Quaternion.LookRotation(info.DetectorInfo.Origin - transform.position));
        }

        //call by Server
        private void ReplicatedData_OnAction(Response.Types.EntityActionData obj)
        {
            switch (obj.Action)
            {
                case Response.Types.EntityAction.kDestroy:
                    break;

                case Response.Types.EntityAction.kUseWeapon:
                    if (replicatedData.IsMine == false)
                        animator.SetAttack(weaponType: GetEquipedWeaponType(replicatedData));
                    break;

                case Response.Types.EntityAction.kEquipWeapon:
                    if (replicatedData.IsMine == false)
                        animator.SetPickUp();
                    break;

                case Response.Types.EntityAction.kUnequipWeapon:
                    break;

                //case Response.Types.EntityAction.kDie:
                //    animator.SetFindStart();
                //    break;
                //case Response.Types.EntityAction.kDie:
                //    animator.SetFindEnd();
                //    break;

                default:
                    break;
            }

        }

        private HumanoidAnimationController.WeaponType GetEquipedWeaponType(ReplicatedEntityData data)
        {
            var type = data.EquippedWeaponType.Value;

            var baseWeaponType = type.GetWeaponBaseType();

            return baseWeaponType switch
            {
                WeaponBaseType.Melee
                => HumanoidAnimationController.WeaponType.Sword,

                WeaponBaseType.Ranged or
                WeaponBaseType.SpecialRarnged
                => HumanoidAnimationController.WeaponType.Wand,

                _
                => HumanoidAnimationController.WeaponType.None,
            };
        }

        private void LateUpdate()
        {
            if (replicatedData.IsMine && replicatedData.IsEnabled.Value)
            {
                var localDirection = ModelRoot.InverseTransformDirection(InputContainer.Value.CameraSpaceMovementDirection.Value.ToVector3FromXZ());
                animator.SetLocalMovement(localDirection.ToXZ(), true);
            }
            else if (!replicatedData.IsMine && replicatedData.IsEnabled.Value)
            {
                if (replicatedData.Velocity.GetDirtyAndClear(out var direction))
                {
                    var localDirection = ModelRoot.InverseTransformDirection(direction.ToVector2().ToVector3FromXZ());
                    animator.SetLocalMovement(localDirection.ToXZ());
                }
                else
                {
                    var localDirection = ModelRoot.InverseTransformDirection(direction.ToVector2().ToVector3FromXZ());
                    animator.SetLocalMovementMagnitude(localDirection.ToXZ().magnitude);
                }
            }
        }

        private void AnimationEventRiser_OnDeadComplete()
        {
            //remove?
            PoolManager.ReleaseObject(replicatedData.gameObject);
        }


        private void OnDisable()
        {
            
        }

        private void OnDestroy()
        {
            if (InputContainer.Value != null)
            {
                InputContainer.Value.OnAttack -= Attack_OnDataChanged;
                InputContainer.Value.OnWeaponSelected -= OnWeaponSelected;
            }
        }
    }
}