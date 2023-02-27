using CulterLib.Game.Chr;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotWanderingAction : BotActionBase
{
    #region Value
    private Vector3 mWanderingDir;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        mWanderingDir = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)).normalized;
    }
    protected override CharacterAction OnFixedUpdate()
    {
        ParentHuman.Agent.SetDestination(ParentHuman.transform.position + mWanderingDir);

        return base.OnFixedUpdate();
    }
    #endregion
}
