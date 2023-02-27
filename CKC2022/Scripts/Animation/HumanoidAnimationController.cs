using Network.Client;
using Network.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace CKC2022
{
    public class HumanoidAnimationController : MonoBehaviour
    {
        public enum BaseLayerType
        {
            Idle,
            Run,
            WakeUp,
            Knockback,
            Revival,
            Death,
            Attack,
            PickUp,

            //wisp::additionalClips
            FindStart,
            FindEnd
        }

        public enum WeaponType
        {
            None,
            Wand = 1,
            Sword = 2,
        }

        [SerializeField]
        protected Animator animator;

        private readonly Notifier<BaseLayerType> BaseLayerState = new Notifier<BaseLayerType>();

        private int MasterLayerIndex;
        private int WalkLayerIndex;

        private float LastActionTime;
        private float LastAttackTime;

        [SerializeField]
        private float recoverTime;
        [SerializeField]
        private AnimationCurve recoverCurve;

        public Vector2 TargetMovement { get; private set; }
        public Vector2 LastMovement { get; private set; }   //디버깅때문에 밖에서 접근가능하게 했음
        public float LastMagnitude { get; private set; }    //디버깅때문에 밖에서 접근가능하게 했음

        public float MoveMagnitude { get => animator.GetFloat("moveMagnitude"); }

        public readonly Notifier<float> LookAtWeight = new();

        private const string MovementStateName = "Movement";

        [Obsolete]
        private const string AttackStateName = "Base.Attack.Hit1(type2)";
        private const string FirstAttackTag = "FirstShot";

        private bool StateIsMovement { get => animator.GetCurrentAnimatorStateInfo(MasterLayerIndex).IsName(MovementStateName); }
        private bool StateIsFirstShot{ get => animator.GetCurrentAnimatorStateInfo(MasterLayerIndex).IsTag(FirstAttackTag); }


        private void Awake()
        {
            MasterLayerIndex = animator.GetLayerIndex("Base Layer");
            WalkLayerIndex = animator.GetLayerIndex("WalkLayer");

        }

        private bool AllowTransition(BaseLayerType current, BaseLayerType next)
        {
            //priority 0
            if (next == BaseLayerType.Revival)
                return true;

            //priority 1
            if (current == BaseLayerType.Death)
                return false;

            if (next == BaseLayerType.FindEnd)
                return true;

            if (current == BaseLayerType.FindStart)
                return false;

            //etc
            return true;
        }

        public void SetKnockBack()
        {
            if (AllowTransition(BaseLayerState.Value, BaseLayerType.Knockback))
            {
                animator.SetTrigger("knockback");
                BaseLayerState.Value = BaseLayerType.Knockback;
            }
        }

        public void SetRevival()
        {
            if (AllowTransition(BaseLayerState.Value, BaseLayerType.Revival))
            {
                ClearAllTrigger();
                animator.SetTrigger("revival");
                BaseLayerState.Value = BaseLayerType.Revival;
            }
        }

        public void SetDeath()
        {
            if (AllowTransition(BaseLayerState.Value, BaseLayerType.Death))
            {
                ClearAllTrigger();
                animator.SetTrigger("death");
                OnSetDeath();
                BaseLayerState.Value = BaseLayerType.Death;
            }
        }
        protected virtual void OnSetDeath()
        {
        }

        public void SetFindStart()
        {
            if (AllowTransition(BaseLayerState.Value, BaseLayerType.FindStart))
            {
                ClearAllTrigger();
                animator.SetTrigger("findStart");
                BaseLayerState.Value = BaseLayerType.FindStart;
            }
        }

        public void SetFindEnd()
        {
            if (AllowTransition(BaseLayerState.Value, BaseLayerType.FindEnd))
            {
                ClearAllTrigger();
                animator.SetTrigger("findEnd");
                BaseLayerState.Value = BaseLayerType.FindEnd;
            }
        }


        public void SetAttack(bool isDown = true, WeaponType weaponType = 0)
        {
            if (AllowTransition(BaseLayerState.Value, BaseLayerType.Attack))
            {
                var state = animator.GetCurrentAnimatorStateInfo(MasterLayerIndex);
                animator.SetBool("continuousAttack", StateIsFirstShot && state.normalizedTime < 0.5f);
                animator.SetInteger("WeaponType", (int)weaponType);
                animator.SetTrigger("attack");
                BaseLayerState.Value = BaseLayerType.Attack;
                LastActionTime = Time.time;
                LastAttackTime = Time.time;
            }
        }

        public void SetPickUp()
        {
            if (AllowTransition(BaseLayerState.Value, BaseLayerType.PickUp))
            {
                animator.SetTrigger("pickUp");
                BaseLayerState.Value = BaseLayerType.PickUp;
                LastActionTime = Time.time;
            }
        }

        //down
        public void SetLocalMovement(in Vector2 direction, bool autoMagnitude = false)
        {
            TargetMovement = direction;
            animator.SetBool("isRunning", LastMovement.magnitude > Vector2.kEpsilon);

            if (autoMagnitude)
                animator.SetFloat("moveMagnitude", LastMovement.magnitude);
        }

        public void SetLocalMovementMagnitude(float magnitude)
        {   
            LastMagnitude = Mathf.Clamp01(Mathf.Lerp(LastMagnitude, magnitude, 0.16f));    //이값 디버깅용 UI에 표시해주기

            animator.SetFloat("moveMagnitude", LastMagnitude);
        }

        private void UpdateLocalMovement()
        {
            LastMovement = Vector2.Lerp(LastMovement, TargetMovement, 0.16f);    //이값 디버깅용 UI에 표시해주기
            
            animator.SetFloat("moveLocalX", LastMovement.x);
            animator.SetFloat("moveLocalZ", LastMovement.y);
        }


        private void Update()
        {
            UpdateLocalMovement();

            CalculateWalkLayerWeight(out var weight);
            animator.SetLayerWeight(WalkLayerIndex, weight);

            CalculateLookAtWeight(out var lookAtWeight);
            LookAtWeight.Value = lookAtWeight;

            //Develop();
        }

        private void CalculateWalkLayerWeight(out float weight)
        {
            var time = Time.time - LastActionTime;
            weight = (1 - recoverCurve.Evaluate(time / recoverTime)) * MoveMagnitude;
        }

        private void CalculateLookAtWeight(out float weight)
        {
            var time = Time.time - LastActionTime;
            weight = 1 - recoverCurve.Evaluate(time / recoverTime);
        }

        private void ClearAllTrigger()
        {
            animator.ResetTrigger("attack");
            animator.ResetTrigger("pickUp");
            animator.ResetTrigger("knockback");
            animator.ResetTrigger("revival");
            animator.ResetTrigger("death");

            animator.ResetTrigger("findStart");
            animator.ResetTrigger("findEnd");

            animator.SetBool("continuousAttack", false);
        }


        private void Develop()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
            {
                SetAttack(weaponType: WeaponType.Wand);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                SetPickUp();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                SetDeath();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.T))
            {
                SetRevival();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
            {
                SetAttack(weaponType: WeaponType.Sword);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha0))
            {
                animator.SetLayerWeight(WalkLayerIndex, 0);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                animator.SetLayerWeight(WalkLayerIndex, 1);
            }

            SetLocalMovement((UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical")).ToVector2());
        }


    }
}