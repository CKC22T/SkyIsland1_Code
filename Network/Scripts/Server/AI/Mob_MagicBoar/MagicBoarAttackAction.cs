using CulterLib.Game.Chr;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class MagicBoarAttackAction : CharacterAction
{
    #region Type
    /// <summary>
    /// 현재 상태
    /// </summary>
    public enum AttackState
    {
        Ready,  //0
        Jump,    //1
        Done,   //2
    }
    #endregion

    #region Inspector
    [TabGroup("Component"), SerializeField] private MagicBoarHideAction mHideAction;
    [TabGroup("Option"), SerializeField] private float mReadyTime = 0.6f;
    [TabGroup("Option"), SerializeField] private float mJumpTime = 0.34f;
    [TabGroup("Option"), SerializeField] private float mDoneTime = 1.6f;
    #endregion
    #region Get,Set
    /// <summary>
    /// 공격 진행상황
    /// </summary>
    public AttackState State { get => mState; }
    #endregion
    #region Value
    [TabGroup("Debug"), SerializeField, ReadOnly] private AttackState mState;
    [TabGroup("Debug"), SerializeField, ReadOnly] private float mTime;
    [TabGroup("Debug"), SerializeField, ReadOnly] private MasterEntityData mTarget;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        ParentMob.SetAnimatorTrigger("Attack");
        SetState(AttackState.Ready);
        ParentMob.Agent.enabled = false;
        ParentMob.Physics.TargetRigidbody.angularVelocity = Vector3.zero;
        ParentMob.Physics.TargetRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }
    protected override CharacterAction OnFixedUpdate()
    {
        mTime += Time.fixedDeltaTime;
        switch(mState)
        {
            case AttackState.Ready:
                if(mReadyTime <= mTime)
                {
                    SetState(AttackState.Jump);
                }
                break;
            case AttackState.Jump:
                ParentMob.Physics.Move(ParentMob.transform.forward.ToXZ());
                if (mJumpTime <= mTime)
                {
                    SetState(AttackState.Done);
                }
                break;
            case AttackState.Done:
                if (mDoneTime <= mTime)
                {
                    if (ParentMob.Physics.State.Value == ECharacterState.Ground)
                        return mHideAction.SetTarget(mTarget).SetType(MagicBoarHideAction.HideType.Attack);
                }
                break;
        }

        return base.OnFixedUpdate();
    }
    protected override void OnEndAction()
    {
        base.OnEndAction();

        ParentMob.Physics.TargetRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
    }
    #endregion
    #region Function
    //Public
    /// <summary>
    /// 타겟을 설정합니다.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public MagicBoarAttackAction SetTarget(MasterEntityData target)
    {
        mTarget = target;
        return this;
    }

    //Private
    /// <summary>
    /// 현재 상태를 설정하고, 동기화합니다.
    /// </summary>
    /// <param name="state"></param>
    private void SetState(AttackState state)
    {
        mState = state;
        mTime = 0;
        ParentMob.SetAnimatorInt("AttackState", (int)mState);

        if (mState == AttackState.Jump)
        {
            ParentMob.ActionAttack(0, () =>
            {   //데미지를 입힐 경우 완료 및 쾅 이펙트 실행하게하기
                ParentMob.SetAnimationOther("AttackDone");
                SetState(AttackState.Done);
            });
        }
        else
        {
            ParentMob.CancelAttack();
        }
    }
    #endregion
}
