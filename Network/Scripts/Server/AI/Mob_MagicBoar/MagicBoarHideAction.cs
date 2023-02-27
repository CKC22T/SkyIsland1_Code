using CulterLib.Game.Chr;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBoarHideAction : CharacterAction
{
    #region Type
    public enum HideType
    {
        Idle,
        Attack,
    }
    #endregion

    #region Inspector
    [TabGroup("Component"), SerializeField] private MagicBoarRiseAction mRiseAction;
    [TabGroup("Component"), SerializeField] private MagicBoarMoveAction mMoveAction;
    [TabGroup("Option"), SerializeField, MinMaxSlider(1.0f, 5.0f, true)] private Vector2 mHideTimeRange = new Vector2(2.0f, 4.0f);
    #endregion
    #region Value
    [TabGroup("Debug"), SerializeField, ReadOnly] private float mHideTime;
    [TabGroup("Debug"), SerializeField, ReadOnly] private float mTime;
    [TabGroup("Debug"), SerializeField, ReadOnly] private MasterEntityData mTarget;
    [TabGroup("Debug"), SerializeField, ReadOnly] private HideType mType;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        mTime = 0;
        mHideTime = Random.Range(mHideTimeRange.x, mHideTimeRange.y);
        if (mType == HideType.Idle)
            ParentMob.SetAnimatorTrigger("HideI");
        else
            ParentMob.SetAnimatorTrigger("HideA");

        //피격 off
        ParentMob.Physics.TargetRigidbody.isKinematic = true;
        ParentMob.OwnerCollider.enabled = false;
    }
    protected override CharacterAction OnFixedUpdate()
    {
        mTime += Time.fixedDeltaTime;
        if (mHideTime <= mTime)
        {
            if (mTarget && mTarget.IsAlive.Value)
                return mMoveAction.SetTarget(mTarget);
            else
                return mRiseAction;
        }

        return base.OnFixedUpdate();
    }
    protected override void OnEndAction()
    {
        base.OnEndAction();
    }
    #endregion
    #region Function
    //Public
    /// <summary>
    /// 타겟을 설정합니다.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public MagicBoarHideAction SetTarget(MasterEntityData target)
    {
        mTarget = target;
        return this;
    }
    /// <summary>
    /// 애니메이션 타입을 설정합니다.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public MagicBoarHideAction SetType(HideType type)
    {
        mType = type;
        return this;
    }
    #endregion
}
