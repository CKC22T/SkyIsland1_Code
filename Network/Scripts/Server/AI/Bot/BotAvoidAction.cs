using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Network.Packet;
using CulterLib.Game.Chr;

public class BotAvoidAction : BotActionBase
{
    #region Type
    private enum AvoidType
    {
        Avoid,
        Hide,
        Runaway
    }
    [System.Serializable] private struct AvoidWeight
    {
        public AvoidType type;
        public float weight;
    }
    #endregion

    #region Inspector
    [Title("회피방식")]
    [TabGroup("Option"), SerializeField] private AvoidWeight[] mTypeWeight;
    #endregion
    #region Value
    private MasterEntityData mTarget;
    private AvoidType mAvoidType;

    //AvoidType.Avoid
    private Vector3 mAvoidDir;
    #endregion

    #region Event
    protected override float OnWeight()
    {
        float weight = base.OnWeight();

        mTarget = GetTarget();
        if (!mTarget)
            weight = 0;

        return weight;
    }
    protected override void OnStartAction()
    {
        base.OnStartAction();

        mAvoidType = GetAvoidType();
        switch (mAvoidType)
        {
            case AvoidType.Avoid:
                mAvoidDir = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)).normalized;
                break;
            case AvoidType.Hide:
                break;
            case AvoidType.Runaway:
                break;
        }
    }
    protected override CharacterAction OnFixedUpdate()
    {
        if (!mTarget)
        {
            EndTimer();
            return base.OnFixedUpdate();
        }

        switch (mAvoidType)
        {
            case AvoidType.Avoid:
                {   //TODO : 정해진 방향(mAvoidDir) 으로 mTime동안 이동한다.
                    ParentHuman.Agent.SetDestination(ParentHuman.transform.position + mAvoidDir);
                }
                break;
            case AvoidType.Hide:
                {   //TODO : 정해진 위치로 이동 후 mTime만큼 대기한다.
                }
                break;
            case AvoidType.Runaway:
                {   //TODO : 적 반대 방향으로 mTime동안 이동한다.
                    var dir = (ParentHuman.transform.position - mTarget.transform.position).normalized;
                    ParentHuman.Agent.SetDestination(ParentHuman.transform.position + dir);
                }
                break;
        }

        return base.OnFixedUpdate();
    }
    protected override void OnEndAction()
    {
        switch (mAvoidType)
        {
            case AvoidType.Avoid:
                mAvoidDir = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)).normalized;
                break;
            case AvoidType.Hide:
                break;
            case AvoidType.Runaway:
                break;
        }

        base.OnEndAction();
    }
    #endregion
    #region Function
    private AvoidType GetAvoidType()
    {
        float total = 0;
        foreach (var v in mTypeWeight)
            total += v.weight;

        float rand = Random.Range(0, total);
        foreach(var v in mTypeWeight)
        {
            rand -= v.weight;
            if (rand <= 0)
                return v.type;
        }

        return mTypeWeight[mTypeWeight.Length - 1].type;
    }
    private MasterEntityData GetTarget()
    {
        //가장 가까운 적을 타겟으로 한다.
        float nd = float.MaxValue;
        MasterEntityData nt = null;
        foreach (var v in ParentManager.Detector.Detected)
            if (v && ParentHuman.FactionType.IsEnemy(v.FactionType))
            {
                float d = Vector3.Distance(ParentHuman.transform.position, v.transform.position);
                if (d < nd)
                {
                    nd = d;
                    nt = v;
                }
            }

        return nt;
    }
    #endregion
}
