using CulterLib.Game.Chr;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritStoneWaveAction : SpiritPatternAction
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private CharacterAction mIdleAction;
    [TabGroup("Option"), SerializeField] private float mAttackDelay = 1.0f;
    [TabGroup("Option"), SerializeField] private float mTime = 3.0f;
    [TabGroup("Option"), SerializeField] private float mFirstDist = 2.0f;
    [TabGroup("Option"), SerializeField] private float mDist = 1.0f;
    [TabGroup("Option"), SerializeField] private float mDelay = 0.1f;
    #endregion
    #region Value
    private float mTimer;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        ParentSpirit.SetAnimatorTrigger("Wave");
        mTimer = 0;
    }
    protected override CharacterAction OnFixedUpdate()
    {
        float lastedTimer = mTimer;
        mTimer += Time.deltaTime;

        if (lastedTimer < mAttackDelay && mAttackDelay <= mTimer)
        {   //적절한 시점에 피격판정
            StartCoroutine(AttackCor());
        }
        if (mTime <= mTimer)
            return mIdleAction;

        return base.OnFixedUpdate();
    }
    #endregion
    #region Function
    private IEnumerator AttackCor()
    {
        var pos = ParentSpirit.transform.position;
        var dir = ParentSpirit.transform.forward;
        for (int i = 0; i < 5; ++i)
        {
            ParentSpirit.ActionStoneAttack(SpriteAttackType.RockWaveAttack, pos + dir * mFirstDist + dir * i * mDist, Quaternion.identity);
            yield return new WaitForSeconds(mDelay);
        }
    }
    #endregion
}
