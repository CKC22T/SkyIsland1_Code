using CulterLib.Game.Chr;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Utils;

public class MagicBoarMoveAction : CharacterAction
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private MagicBoarRiseAction mRiseAction;
    [TabGroup("Component"), SerializeField] private MagicBoarAttackAction mAttackAction;
    [TabGroup("Option"), SerializeField] private float mDist = 2.0f;
    [TabGroup("Option"), SerializeField] private float mUpDelay = 0.1f;
    [TabGroup("Option"), SerializeField] private float mUpTime = 1.0f;
    #endregion
    #region Value
    [TabGroup("Debug"), SerializeField, ReadOnly] private float mTime;
    [TabGroup("Debug"), SerializeField, ReadOnly] private MasterEntityData mTarget;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        mTime = 0;
        ParentMob.Agent.enabled = true;

        //타겟 위치 근처로 이동 후 올라오기
        if (mTarget && mTarget.IsAlive.Value)
        {
            var dist = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)).normalized;
            var targetPos = mTarget.transform.position + dist * mDist;
            var lastedPos = ParentMob.transform.position;

            if (ParentMob.Agent.Warp(targetPos))
            {
                ParentMob.transform.LookAt(mTarget.transform.position.AdaptY(ParentMob.transform.position.y));
                return;
            }
            else
            {   //워프에 실패할 경우 원위치 및 Rise로
                ParentMob.Agent.Warp(lastedPos);
                ParentManager.SetAction(mRiseAction);
            }
        }
        //타겟이 없는 경우 Rise로
        else
            ParentManager.SetAction(mRiseAction);
    }
    protected override CharacterAction OnFixedUpdate()
    {
        float lt = mTime;
        mTime += Time.deltaTime;
        if (lt < mUpDelay && mUpDelay <= mTime)
        {   //올라오기
            ParentMob.SetAnimatorTrigger("Move");
        }
        if (mUpTime <= mTime)
        {   //공격으로
            return mAttackAction.SetTarget(mTarget);
        }

        return base.OnFixedUpdate();
    }
    protected override void OnEndAction()
    {
        base.OnEndAction();

        //피격ON
        ParentMob.Physics.TargetRigidbody.isKinematic = false;
        ParentMob.OwnerCollider.enabled = true;
    }
    #endregion
    #region Function
    //Public
    /// <summary>
    /// 타겟을 설정합니다.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public MagicBoarMoveAction SetTarget(MasterEntityData target)
    {
        mTarget = target;
        return this;
    }
    #endregion
}
