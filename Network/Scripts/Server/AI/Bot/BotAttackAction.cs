using CulterLib.Game.Chr;
using Network.Packet;
using Network.Server;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class BotAttackAction : BotActionBase
{
    #region Value
    [TabGroup("Debug"), SerializeField] private MasterEntityData mTarget;
    #endregion

    #region Event
    protected override float OnWeight()
    {
        mTarget = GetTarget();

        if (!ParentHuman.HasWeapon || mTarget && (!mTarget.enabled || !mTarget.IsAlive.Value || !mTarget.IsEnabled.Value || mTarget.Hp.Value <= 0))
            return 0;
        else
            return (mTarget != null) ? base.OnWeight() : 0;
    }
    protected override void OnStartAction()
    {
        base.OnStartAction();
    }
    protected override CharacterAction OnFixedUpdate()
    {
        if (!mTarget)
            return base.OnFixedUpdate();

        //타겟 적에게 이동
        ParentHuman.Agent.SetDestination(mTarget.transform.position);

        //열심히 공격하기
        Vector3 dir = (mTarget.transform.position - ParentHuman.transform.position).SetY(0).normalized;
        ParentHuman.ActionUseWeapon(dir);
        ParentHuman.Velocity.Value = dir;

        //
        return base.OnFixedUpdate();
    }
    protected override void OnEndAction()
    {
        base.OnEndAction();
    }
    #endregion
    #region Function
    /// <summary>
    /// 공격할 타겟을 가져옵니다.
    /// </summary>
    /// <returns></returns>
    private MasterEntityData GetTarget()
    {
        if (ParentHuman.EquippedWeaponType.Value == ItemType.kWeaponNobleSacrifice)
        {   //힐러면 가장 체력이 적은 아군을 타겟으로 한다.
            float lh = float.MaxValue;
            MasterEntityData lt = null;
            foreach (var v in ParentManager.Detector.Detected)
                if (v && ParentHuman.FactionType.IsAlliance(v.FactionType) && v.Hp.Value < v.MaxHp && v.Hp.Value < lh)
                {
                    lh = v.Hp.Value;
                    lt = v;
                }

            return lt;
        }
        else
        {   //딜러면 가장 가까운 적을 타겟으로 한다.
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
    }
    #endregion
}
