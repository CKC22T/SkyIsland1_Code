using CulterLib.Game.Chr;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotIdleAction : BotActionBase
{
    #region Event
    protected override void OnStartAction()
    {
        ParentHuman.Agent.SetDestination(transform.position);
        base.OnStartAction();
    }
    protected override CharacterAction OnUpdate()
    {
        return base.OnUpdate();
    }
    protected override CharacterAction OnFixedUpdate()
    {
        return this;
    }
    #endregion
}
