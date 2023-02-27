using CulterLib.Game.Chr;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollowAction : BotActionBase
{
    #region Inspector
    [TabGroup("Option"), SerializeField] private float mMaxDist = 8.0f;
    [TabGroup("Option"), SerializeField] private float mMinDist = 2.0f;
    [TabGroup("Option"), SerializeField] private float mWarpDist = 20.0f;
    #endregion
    #region Value
    private MasterHumanoidEntityData mTarget;
    #endregion

    #region Event
    protected override float OnWeight()
    {
        mTarget = GetTarget();

        return (mTarget != null) ? base.OnWeight() : 0;
    }
    protected override void OnStartAction()
    {
        base.OnStartAction();

        ParentHuman.Agent.SetDestination(mTarget.transform.position);
    }
    protected override CharacterAction OnUpdate()
    {
        float dist = Vector3.Distance(ParentHuman.transform.position, mTarget.transform.position);

        //이동 (너무 멀면 순간이동)
        if (mWarpDist < dist)
            ParentHuman.Agent.Warp(mTarget.transform.position);
        else
            ParentHuman.Agent.SetDestination(mTarget.transform.position);

        //충분히 가까워지면 다른액션으로 변경 시도
        if (dist < mMinDist)
            EndTimer();

        return base.OnUpdate();
    }
    protected override CharacterAction OnFixedUpdate()
    {
        return this;
    }
    protected override void OnEndAction()
    {
        base.OnEndAction();
    }
    #endregion
    #region Function
    /// <summary>
    /// 가장 가까이 있는 인간이 조종중인 아군을 가져옵니다.
    /// </summary>
    /// <returns></returns>
    private MasterHumanoidEntityData GetTarget()
    {
        bool isNear = false;
        float nd = float.MaxValue;
        MasterHumanoidEntityData nt = null;
        foreach (var v in ServerPlayerCharacterManager.Instance.PlayerEntities)
            if (0 <= v.BindedClientID.Value)
            {
                float d = Vector3.Distance(v.transform.position, ParentHuman.transform.position);
                if (mMaxDist < d && d < nd)
                {
                    nd = d;
                    nt = v;
                }
                if(d <= mMaxDist)
                    isNear = true;
            }

        if (isNear)
            return null;
        else
            return nt;
    }
    #endregion
}
