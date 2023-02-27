using CulterLib.Game.Chr;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBoarDamagedAction : CharacterAction
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private CharacterAction mIldeAction;
    [TabGroup("Option"), SerializeField] private float mTime;
    #endregion
    #region Value
    private float mTimer;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        mTimer = 0;
        ParentMob.SetAnimatorTrigger("Hit");
    }
    protected override CharacterAction OnFixedUpdate()
    {
        mTimer += Time.fixedDeltaTime;
        if (mTime <= mTimer)
            return mIldeAction;
        else
            return this;
    }
    #endregion
    #region Function
    //Public
    internal void ResetAni()
    {
        mTimer = 0;
        ParentMob.SetAnimatorTrigger("Hit");
    }
    #endregion
}
