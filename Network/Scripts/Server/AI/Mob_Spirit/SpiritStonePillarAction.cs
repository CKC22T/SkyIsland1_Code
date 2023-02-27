using CulterLib.Game.Chr;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritStonePillarAction : SpiritPatternAction
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private CharacterAction mIdleAction;
    [TabGroup("Option"), SerializeField] private float mAttackDelay = 0.6f;
    [TabGroup("Option"), SerializeField] private float mTime = 3.0f;
    #endregion
    #region Value
    private float mTimer;
    private Vector3 mTargetPos;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        ParentSpirit.SetAnimatorTrigger("Pillar");
        mTimer = 0;
        mTargetPos = mTarget.transform.position;
    }
    protected override CharacterAction OnFixedUpdate()
    {
        float lastedTimer = mTimer;
        mTimer += Time.deltaTime;

        if (lastedTimer < mAttackDelay && mAttackDelay <= mTimer)
        {   //적절한 시점에 피격판정
            if (ParentSpirit.MaxHp / 2 <= ParentSpirit.Hp.Value)
                ParentSpirit.ActionStoneAttack(SpriteAttackType.StoneCone_1_Attack, mTargetPos, Quaternion.identity);
            else
                ParentSpirit.ActionStoneAttack(SpriteAttackType.StoneCone_2_Attack, mTargetPos, Quaternion.identity);
        }
        if (mTime <= mTimer)
            return mIdleAction;

        return base.OnFixedUpdate();
    }
    #endregion
}
