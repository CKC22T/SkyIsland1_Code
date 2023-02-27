using CulterLib.Game.Chr;
using Network.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritFreezeAction : CharacterAction
{
    #region Get,Set
    /// <summary>
    /// 스피릿 Entity 숏컷
    /// </summary>
    public MasterSpriteEntityData ParentSpirit { get => ParentManager.ParentEntity as MasterSpriteEntityData; }
    #endregion

    #region Event
    protected override void OnEndAction()
    {
        base.OnEndAction();

        ParentSpirit.Agent.enabled = true;
    }
    #endregion
}
