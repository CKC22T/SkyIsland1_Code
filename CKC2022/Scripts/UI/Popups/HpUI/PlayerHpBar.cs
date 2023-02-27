using Network.Packet;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHpBar : HpBar
{
    #region Type
    [System.Serializable] private struct ItemIcon
    {
        public ItemType type;
        public GameObject icon;
    }
    #endregion

    #region Inspector
    [TabGroup("Component"), SerializeField] private ItemIcon[] mIcon;
    #endregion

    #region Event
    protected override void OnAddTarget()
    {
        base.OnAddTarget();

        AddDataChangeEvent(m_Target.Data.EquippedWeaponType, OnEquipChanged, true);
    }
    protected override void OnRemoveTarget()
    {
        base.OnRemoveTarget();

        RemoveDataChangeEvent(m_Target.Data.EquippedWeaponType, OnEquipChanged);
    }

    //Data Event
    private void OnEquipChanged(ItemType _type)
    {
        foreach(var v in mIcon)
            v.icon.SetActive(_type == v.type);
    }
    #endregion
}
