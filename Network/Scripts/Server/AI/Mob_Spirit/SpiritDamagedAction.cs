using CulterLib.Game.Chr;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritDamagedAction : CharacterAction
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private CharacterAction mIldeAction;
    [TabGroup("Option"), SerializeField] private float mTime;
    #endregion
    #region Get,Set
    /// <summary>
    /// 스피릿 Entity 숏컷
    /// </summary>
    public MasterSpriteEntityData ParentSpirit { get => ParentManager.ParentEntity as MasterSpriteEntityData; }
    #endregion
    #region Value
    private float mTimer;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        ParentSpirit.SetAnimatorTrigger("Damaged");
    }
    protected override CharacterAction OnFixedUpdate()
    {
        mTimer += Time.fixedDeltaTime;
        if (mTime <= mTimer)
            return this;
        else
            return mIldeAction;
    }
    #endregion
}
