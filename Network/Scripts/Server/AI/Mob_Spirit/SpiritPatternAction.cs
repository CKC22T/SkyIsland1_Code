using CulterLib.Game.Chr;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class SpiritPatternAction : CharacterAction
{
    #region Inspector
    [TabGroup("Option"), SerializeField] private float mDistance;
    #endregion
    #region Get,Set
    /// <summary>
    /// 스피릿 Entity 숏컷
    /// </summary>
    public MasterSpriteEntityData ParentSpirit { get => ParentManager.ParentEntity as MasterSpriteEntityData; }
    /// <summary>
    /// 해당 패턴 사용 가능 거리
    /// </summary>
    public float Distance { get => mDistance; }
    #endregion
    #region Value
    [TabGroup("Debug"), SerializeField, ReadOnly] protected MasterEntityData mTarget;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        ParentSpirit.transform.LookAt(mTarget.transform.position.SetY(ParentSpirit.transform.position.y));
    }
    #endregion
    #region Function
    //Public
    /// <summary>
    /// 타겟을 설정합니다.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public SpiritPatternAction SetTarget(MasterEntityData target)
    {
        mTarget = target;
        return this;
    }
    #endregion
}
