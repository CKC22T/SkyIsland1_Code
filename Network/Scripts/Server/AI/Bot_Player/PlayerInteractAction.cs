using Network.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Network.Packet;
using CulterLib.Game.Chr;
using Sirenix.OdinInspector;
using Network.Client;

public class PlayerInteractAction : BotActionBase
{
    #region Inspector
    [TabGroup("Option"), LabelText("무기우선순위"), SerializeField] private ItemType[] mPreper;
    [TabGroup("Option"), SerializeField] private float mSearchDist = 10;   //Detector 써야하는데 아이템관련코드 막판에 수정되면서 임시로 그냥 전체 아이템리스트에서 검색하는식으로 변경
    [TabGroup("Option"), SerializeField] private float mEquipDist = 2;   //vEquipDistance값 써야하는데 어셈블리 나뉘면서 못쓰게됨
    #endregion
    #region Value
    private Dictionary<ItemType, int> mPrefer = new Dictionary<ItemType, int>();
    private ReplicableItemObject mTarget;
    #endregion

    #region Event
    protected override void OnInit()
    {
        base.OnInit();

        for (int i = 0; i < mPreper.Length; i++)
            mPrefer.Add(mPreper[i], i);
    }
    protected override float OnWeight()
    {
        mTarget = GetTarget();
        return (mTarget != null) ? base.OnWeight() : 0;
    }
    protected override CharacterAction OnFixedUpdate()
    {
        if (!ItemObjectManager.TryGetInstance(out var itemObjectManager) || !itemObjectManager.HasItem(mTarget))
            return base.OnFixedUpdate();

        //타겟 무기로 이동
        ParentHuman.Agent.SetDestination(mTarget.transform.position);

        //일정거리 안에 들어가면 무기 줍고 넘어가기
        if (Vector3.Distance(mTarget.transform.position, ParentHuman.transform.position) <= mEquipDist)
        {
            ParentHuman.ActionObtainWeapon(mTarget.ItemObjectID);
            return base.OnFixedUpdate();
        }
        else
            return this;
    }
    #endregion
    #region Function
    private ReplicableItemObject GetTarget()
    {
        if (!ItemObjectManager.TryGetInstance(out var itemObjectManager))
            return null;

        //가장 선호하거나 가까운 무기를 타겟으로 한다.
        int np = int.MaxValue;
        float nd = float.MaxValue;
        ReplicableItemObject nt = null;
        foreach (var v in itemObjectManager.ItemObjects)
            if (v.ItemType.IsWeapon() && Vector3.Distance(ParentHuman.transform.position, v.transform.position) <= mSearchDist)
            {
                int p = mPrefer.TryGetValue(v.ItemType, out p) ? p : int.MaxValue;
                float d = Vector3.Distance(ParentHuman.transform.position, v.transform.position);
                if (p <= np && d < nd)
                {
                    np = p;
                    nd = d;
                    nt = v;
                }
            }

        //갖고있는 무기랑 비슷하거나 더 안좋은 무기면 그냥 무시하기
        if (ParentHuman.HasWeapon)
        {
            int cp = mPrefer.TryGetValue(ParentHuman.EquippedWeaponType.Value, out cp) ? cp : int.MaxValue;
            if (np <= cp)
                return null;
        }
        return nt;
    }
    #endregion
}
