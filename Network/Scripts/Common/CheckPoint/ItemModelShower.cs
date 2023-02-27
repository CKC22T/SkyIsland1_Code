using Network;
using Network.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[Serializable]
public class ItemModel
{
    public GameObject Model;
    public ItemType Type;
}

public class ItemModelShower : MonoBehaviour
{
    [SerializeField] private List<ItemModel> mItemModels;

    public void Start()
    {
        if (mItemModels == null || mItemModels.IsEmpty())
        {
            Debug.LogError(LogManager.GetLogMessage($"There is no item models!", NetworkLogType.None, true));
            return;
        }

        foreach (var m in mItemModels)
        {
            m.Model.SetActive(false);
        }
    }

    public void ChangeItemModel(ItemType weaponItemType)
    {
        foreach (var m in mItemModels)
        {
            m.Model.SetActive(m.Type == weaponItemType);
        }
    }
}
