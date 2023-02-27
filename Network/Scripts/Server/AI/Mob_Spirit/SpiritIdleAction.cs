using CulterLib.Game.Chr;
using Network.Packet;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritIdleAction : CharacterAction
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private SpiritMoveAction mMoveAction;
    [TabGroup("Component"), SerializeField] private SpiritPhaseChangeAction mPhaseChangeAction;
    #endregion
    #region Get,Set
    /// <summary>
    /// 스피릿 Entity 숏컷
    /// </summary>
    public MasterSpriteEntityData ParentSpirit { get => ParentManager.ParentEntity as MasterSpriteEntityData; }
    #endregion
    #region Value
    [TabGroup("Debug"), SerializeField, ReadOnly] private MasterEntityData mTarget;
    [TabGroup("Debug"), SerializeField, ReadOnly] private bool mPhaseChanged;
    #endregion

    #region Event
    protected override void OnStartAction()
    {
        base.OnStartAction();
        ParentSpirit.Agent.SetDestination(ParentSpirit.transform.position);
        ParentSpirit.SetAnimatorTrigger("Idle");
    }
    protected override CharacterAction OnFixedUpdate()
    {
        //페이즈2로 넘어갈 조건이면 넘어감
        if (ParentSpirit.TryGetChangePhase(out var next))
            return next;

        if (0 < ParentManager.Detector.Detected.Count)
        {   //가장 가까운 적으로 타겟 설정
            float ndist = float.MaxValue;
            MasterEntityData nentity = null;
            foreach (var v in ParentManager.Detector.Detected)
            {
                float dist = Vector3.Distance(ParentSpirit.transform.position, v.transform.position);
                if (ParentSpirit.FactionType.IsEnemy(v.FactionType) && dist < ndist)
                {
                    nentity = v;
                    ndist = dist;
                }
            }
            mTarget = nentity;
        }

        if (mTarget)
        {   //이동
            return mMoveAction.SetTarget(mTarget);
        }

        return base.OnFixedUpdate();
    }
    #endregion
}
