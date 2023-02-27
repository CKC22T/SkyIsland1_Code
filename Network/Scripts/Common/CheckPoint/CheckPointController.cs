using Network.Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointController : MonoBehaviour
{
    [field : SerializeField] public int CheckPointNumber { get; private set;} = 0;
    public GameObject CheckPointEffect;
    public ItemModelShower ItemModelShower;

    private NetworkMode mNetworkMode = NetworkMode.None;

    private bool mInitialize = false;

    public void Awake()
    {
        CheckPointEffect.SetActive(false);
    }

    public void InitializeByManager(NetworkMode networkMode)
    {
        if (mInitialize)
            return;

        mInitialize =true;

        mNetworkMode = networkMode;

        //if (mNetworkMode == NetworkMode.Remote)
        //{
        //    var checkPointSystem = ClientSessionManager.Instance.GameGlobalState.GameGlobalState.CheckPointSystem;
        //    checkPointSystem.BindOnCheckPointWeaponChanged(CheckPointNumber, ItemModelShower.ChangeItemModel);
        //}
    }

    private ItemType mCheckPointWeaponModelType;

    public void FixedUpdate()
    {
        if (mCheckPointWeaponModelType != 0)
        {
            ItemModelShower.ChangeItemModel(mCheckPointWeaponModelType);
            mCheckPointWeaponModelType = 0;
        }
    }

    public Vector3 GetItemShowerPosition()
    {
        return ItemModelShower.transform.position;
    }

    public void TrySetWeaponItem(ItemType weaponItem)
    {
        mCheckPointWeaponModelType = weaponItem;
    }

    public void EnableCheckPointEffect()
    {
        CheckPointEffect.SetActive(true);
    }
}
