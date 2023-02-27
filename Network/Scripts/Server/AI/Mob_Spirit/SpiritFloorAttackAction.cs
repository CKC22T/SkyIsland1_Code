using CulterLib.Game.Chr;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritFloorAttackAction : SpiritPatternAction
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private CharacterAction mIdleAction;
    [TabGroup("Option"), SerializeField] private float mAttackDelay = 0.6f;
    [TabGroup("Option"), SerializeField] private float mTime = 3.1f;
    #endregion
    #region Value
    private float mTimer;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        ParentSpirit.SetAnimatorTrigger("FloorStart");
        mTimer = 0;
    }
    protected override CharacterAction OnFixedUpdate()
    {
        float lastedTimer = mTimer;
        mTimer += Time.deltaTime;

        if (lastedTimer < mAttackDelay && mAttackDelay <= mTimer)
        {   //적절한 시점에 피격판정
            var attackPosition = ParentSpirit.FloorAttackPosition;
            ParentSpirit.ActionStoneAttack(SpriteAttackType.FloorAttack, attackPosition, Quaternion.identity);
        }
        if (mTime <= mTimer)
            return mIdleAction;

        return base.OnFixedUpdate();
    }
    #endregion
}
