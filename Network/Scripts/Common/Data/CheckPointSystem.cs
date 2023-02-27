using Network;
using Network.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class CheckPointSystem : INetworkAssignable
{
    public int CheckPointNumber => mCheckPointNumber.Value;

    [Sirenix.OdinInspector.ShowInInspector] private NetIntData mCheckPointNumber = new NetIntData(0);
    [Sirenix.OdinInspector.ShowInInspector] private List<NetEnumTypeData<ItemType>> mWeaponSlots;
    //[Sirenix.OdinInspector.ShowInInspector] private List<bool> mIsSpawnWeaponList;

    private List<ItemType> mDefaultWeapons;
    private int mMaxCheckPoint;

    public event Action<int, ItemType> OnCheckPointWeaponChanged;

    public CheckPointSystem(int maxCheckPoint)
    {
        mMaxCheckPoint = maxCheckPoint;

        mDefaultWeapons = new List<ItemType>(ServerConfiguration.DefaultWeaponItems);
        //mIsSpawnWeaponList = new List<bool>();
        mWeaponSlots = new List<NetEnumTypeData<ItemType>>();

        for (int i = 0; i < mMaxCheckPoint; i++)
        {
            mWeaponSlots.Add(new NetEnumTypeData<ItemType>(ItemType.kNoneItemType));
            //mIsSpawnWeaponList.Add(false);
        }
    }

    public void InitializeDataAsMaster(in MasterReplicationObject assignee)
    {
        assignee.AssignDataAsReliable(mCheckPointNumber);

        foreach (var w in mWeaponSlots)
        {
            assignee.AssignDataAsReliable(w);
        }
    }

    public void InitializeDataAsRemote(in RemoteReplicationObject assignee)
    {
        assignee.AssignDataAsReliable(mCheckPointNumber);

        foreach (var w in mWeaponSlots)
        {
            assignee.AssignDataAsReliable(w);
        }
    }

    public void ResetData()
    {
        mCheckPointNumber.Value = 0;

        for (int i = 0; i < mMaxCheckPoint; i++)
        {
            mWeaponSlots[i].Value = ItemType.kNoneItemType;
            //mIsSpawnWeaponList[i] = false;
        }

        OnCheckPointWeaponChanged = null;
    }

    #region Getter

    public bool TryGetCheckPointWeapon(int checkPointNumber, out ItemType checkPointWeaponItem)
    {
        checkPointWeaponItem = ItemType.kNoneItemType;

        if (!isValidCheckPointNumber(checkPointNumber))
        {
            return false;
        }

        checkPointWeaponItem = mWeaponSlots[checkPointNumber].Value;

        return checkPointWeaponItem.IsWeapon();
    }

    #endregion

    #region Operation

    public void BindOnCheckPointWeaponChanged(int checkPointNumber, Action<ItemType> onChanged)
    {
        if (!isValidCheckPointNumber(checkPointNumber))
            return;

        mWeaponSlots[checkPointNumber].OnDataChanged += onChanged;
    }

    public void CheckPoint(int checkPointNumber)
    {
        if (!isValidCheckPointNumber(checkPointNumber))
            return;

        mCheckPointNumber.Value = checkPointNumber;

        //if (mIsSpawnWeaponList[checkPointNumber])
        //    return;

        changeCheckPointWeapon(checkPointNumber, mDefaultWeapons[checkPointNumber]);
        //mIsSpawnWeaponList[checkPointNumber] = true;
    }

    public ItemType PickUpWeapon(int checkPointNumber)
    {
        if (!isValidCheckPointNumber(checkPointNumber))
            return ItemType.kNoneItemType;

        var checkPointWeapon = mWeaponSlots[checkPointNumber].Value;
        changeCheckPointWeapon(checkPointNumber, ItemType.kNoneItemType);

        return checkPointWeapon;
    }

    private void changeCheckPointWeapon(int checkPointNumber, ItemType weaponType)
    {
        mWeaponSlots[checkPointNumber].Value = weaponType;
        OnCheckPointWeaponChanged?.Invoke(checkPointNumber, weaponType);
    }

    #endregion

    private bool isValidCheckPointNumber(int checkPointNumber)
    {
        return checkPointNumber >= 0 && checkPointNumber <= mMaxCheckPoint;
    }
}
