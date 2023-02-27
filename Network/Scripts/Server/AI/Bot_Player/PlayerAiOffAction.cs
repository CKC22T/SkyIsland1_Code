using CulterLib.Game.Chr;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAiOffAction : CharacterAction
{
    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();
        ParentHuman.Agent.enabled = false;
        ParentHuman.Physics.enabled = true;
    }
    protected override void OnEndAction()
    {
        base.OnEndAction();
        ParentHuman.Agent.enabled = true;
        ParentHuman.Physics.enabled = false;
    }
    #endregion
}
