using System;
using Network.Packet;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ItemInfo
{
    public ItemType WeaponType;
    public GameObject WeaponObject;
}

public class WeaponSlotUI : MonoBehaviour
{
    [field: SerializeField] public int WeaponSlotNumber { get; private set; } = -1;
    [SerializeField] private Animator weaponSlotAnimator;
    [SerializeField] private List<ItemInfo> WeaponImageInfoList = new();
    private Dictionary<ItemType, GameObject> mWeaponImageTable = new();
    private ItemType slotWeaponType;

    public void Awake()
    {
        foreach (var i in WeaponImageInfoList)
        {
            mWeaponImageTable.Add(i.WeaponType, i.WeaponObject);
        }
    }

    public void SetSlot(ItemType weaponType)
    {
        foreach (var w in WeaponImageInfoList)
        {
            w.WeaponObject.SetActive(w.WeaponType == weaponType);
        }
    }

    public void SetWeaponPointer(int currentPointer)
    {
        if (WeaponSlotNumber == currentPointer)
        {
            weaponSlotAnimator.SetTrigger("Selected");
        }
        else
        {
            weaponSlotAnimator.SetTrigger("Normal");
        }
    }
}
