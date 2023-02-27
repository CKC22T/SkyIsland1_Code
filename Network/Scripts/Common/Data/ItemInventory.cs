using System;
using System.Collections.Generic;
using Network;
using Network.Packet;

using UnityEngine;
using Utils;

[Serializable]
public class ItemInventory
{
    [SerializeField]
    public List<NetEnumTypeData<ItemType>> WeaponSlots;
    public NetIntData WeaponPointer = new NetIntData(0);

    public List<ItemType> LastCheckPointWeapons;

    #region Initializer

    public ItemInventory()
    {
        WeaponSlots = new List<NetEnumTypeData<ItemType>>();
        LastCheckPointWeapons = new List<ItemType>();

        for (int i = 0; i < ServerConfiguration.MAX_WEAPON_ITEM_INVENTORY; i++)
        {
            WeaponSlots.Add(new NetEnumTypeData<ItemType>(ItemType.kNoneItemType));
            LastCheckPointWeapons.Add(ItemType.kNoneItemType);
        }
    }

    public void InitializeDataAsMaster(in MasterReplicationObject assignee)
    {
        assignee.AssignDataAsReliable(WeaponPointer);

        foreach (var item in WeaponSlots)
        {
            assignee.AssignDataAsReliable(item);
        }
    }

    public void InitializeDataAsRemote(in RemoteReplicationObject assignee)
    {
        assignee.AssignDataAsReliable(WeaponPointer);

        foreach (var item in WeaponSlots)
        {
            assignee.AssignDataAsReliable(item);
        }
    }

    public void Reset()
    {
        WeaponPointer.Value = 0;

        foreach (var item in WeaponSlots)
        {
            item.Value = ItemType.kNoneItemType;
        }
    }

    #endregion

    #region Handler

    public ServerOperationResult ObtainItem(ItemType itemType)
    {
        if (itemType.IsWeapon())
        {
            return obtainWeapon(itemType);
        }

        return ServerOperationResult.Inventory_CannotObtainThisItem;
    }

    private ServerOperationResult obtainWeapon(ItemType weaponType)
    {
        if (HasWeapon(weaponType))
        {
            return ServerOperationResult.Inventory_YouAlreadyHaveThisWeapon;
        }

        if (!HasEmptyWeaponSlot())
        {
            return ServerOperationResult.Inventory_FullWeaponSlot;
        }

        foreach (var weaponSlot in WeaponSlots)
        {
            if (weaponSlot.Value == ItemType.kNoneItemType)
            {
                weaponSlot.Value = weaponType;
                arrangeWeaponSlots();
                int lastWeaponIndex = GetLastWeaponIndex();
                WeaponPointer.Value = lastWeaponIndex;
                SwapWeapon(lastWeaponIndex);

                return ServerOperationResult.Inventory_SuccessObtainWeapon;
            }
        }

        return ServerOperationResult.Inventory_WrongOperation;
    }

    public ServerOperationResult DropWeapon(out ItemType droppedItem)
    {
        var currentEquipWeapon = GetEquipWeapon();
        if (currentEquipWeapon.IsWeapon())
        {
            droppedItem = currentEquipWeapon;

            WeaponSlots[WeaponPointer.Value].Value = ItemType.kNoneItemType;
            arrangeWeaponSlots();

            // Set last weapon if current slot empty
            var currentWeapon = GetEquipWeapon();

            if (currentWeapon == ItemType.kNoneItemType)
            {
                SwapToLastWeapon();
            }

            return ServerOperationResult.Inventory_SuccessDropWeapon;
        }

        droppedItem = ItemType.kNoneItemType;
        return ServerOperationResult.Inventory_WrongOperation;
    }

    public List<ItemType> DropAllWeapon()
    {
        WeaponPointer.Value = 0;
        List<ItemType> droppedWeapons = new List<ItemType>();

        foreach (var weaponSlot in WeaponSlots)
        {
            if (weaponSlot.Value.IsWeapon())
            {
                droppedWeapons.Add(weaponSlot.Value);
                weaponSlot.Value = ItemType.kNoneItemType;
            }
        }

        return droppedWeapons;
    }

    public void SaveCurrentInventory()
    {
        for (int i = 0; i < WeaponSlots.Count; i++)
        {
            LastCheckPointWeapons[i] = WeaponSlots[i].Value;
        }
    }

    public void LoadSavedInventory()
    {
        for (int i = 0; i < WeaponSlots.Count; i++)
        {
            WeaponSlots[i].Value = LastCheckPointWeapons[i];
        }
    }

    #endregion

    #region Getter

    public bool HasAnyWeapon()
    {
        foreach (var slot in WeaponSlots)
        {
            if (slot.Value.IsWeapon())
            {
                return true;
            }
        }

        return false;
    }

    public bool HasWeapon(ItemType weaponType)
    {
        foreach (var slot in WeaponSlots)
        {
            if (slot.Value == weaponType)
            {
                return true;
            }
        }

        return false;
    }

    public int GetWeaponCount()
    {
        int weaponCount = 0;

        foreach (var slot in WeaponSlots)
        {
            if (slot.Value.IsWeapon())
            {
                weaponCount++;
            }
        }

        return weaponCount;
    }

    public ItemType GetWeaponByIndex(int index)
    {
        if (index < WeaponSlots.Count)
        {
            return WeaponSlots[index].Value;
        }

        return ItemType.kNoneItemType;
    }

    public bool HasEmptyWeaponSlot()
    {
        foreach (var weaponSlot in WeaponSlots)
        {
            if (weaponSlot.Value == ItemType.kNoneItemType)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>무기 슬롯의 무기를 빈 공간 없이 앞에서 부터 정렬합니다.</summary>
    private void arrangeWeaponSlots()
    {
        Queue<ItemType> currentWeapons = new();

        foreach (var weaponSlot in WeaponSlots)
        {
            if (weaponSlot.Value != ItemType.kNoneItemType)
            {
                currentWeapons.Enqueue(weaponSlot.Value);
                weaponSlot.Value = ItemType.kNoneItemType;
            }
        }

        int weaponSlotIndex = 0;
        while (!currentWeapons.IsEmpty())
        {
            WeaponSlots[weaponSlotIndex].Value = currentWeapons.Dequeue();
            weaponSlotIndex++;
        }
    }

    public ItemType GetEquipWeapon()
    {
        return GetWeaponByIndex(WeaponPointer.Value);
    }

    public int GetLastWeaponIndex()
    {
        return GetWeaponCount() > 0 ? GetWeaponCount() - 1 : 0;
    }

    #endregion

    #region Operation

    public ItemType SwapWeapon(int weaponIndex)
    {
        var weaponType = GetWeaponByIndex(weaponIndex);

        if (weaponType.IsWeapon())
        {
            WeaponPointer.Value = weaponIndex;
        }

        return weaponType;
    }

    public ItemType SwapToLastWeapon()
    {
        WeaponPointer.Value = GetLastWeaponIndex();
        return GetWeaponByIndex(WeaponPointer.Value);
    }

    #endregion
}