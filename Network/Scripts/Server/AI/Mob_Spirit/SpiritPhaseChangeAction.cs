using CulterLib.Game.Chr;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritPhaseChangeAction : CharacterAction
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private CharacterAction mIdleAction;
    [TabGroup("Option"), SerializeField] private float mTime = 10.0f;
    #endregion
    #region Get,Set
    /// <summary>
    /// 스피릿 Entity 숏컷
    /// </summary>
    public MasterSpriteEntityData ParentSpirit { get => ParentManager.ParentEntity as MasterSpriteEntityData; }
    #endregion
    #region Value
    private float mTimer = 0;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        mTimer = 0;
        ParentSpirit.SetAnimatorBool("Death1", true);
        ParentSpirit.SetImmortal(true);
    }
    protected override CharacterAction OnFixedUpdate()
    {
        float lasted = mTimer;
        mTimer += Time.deltaTime;

        if (lasted < mTime * 0.5f && mTime * 0.5f <= mTimer)
            ParentSpirit.SetAnimatorBool("Death1", false);

        if (mTime <= mTimer)
            return mIdleAction;
        else
            return base.OnFixedUpdate();
    }
    protected override void OnEndAction()
    {
        base.OnEndAction();
        ParentSpirit.SetImmortal(false);
    }
    #endregion
}
