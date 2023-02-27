using CulterLib.Game.Chr;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToGoalAction : BotActionBase
{
    #region Inspector
    [TabGroup("Option"), SerializeField] private float mMinTime = 1.0f;
    #endregion
    #region Value
    private Transform m_Target;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();

        ParentHuman.Agent.SetDestination(RouteManager.Instance.StageFinish.position);
    }
    protected override CharacterAction OnFixedUpdate()
    {
        return this;
    }
    protected override void OnEndAction()
    {
        base.OnEndAction();
        ParentHuman.Agent.SetDestination(ParentHuman.transform.position);
    }
    #endregion
}
