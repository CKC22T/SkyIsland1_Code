using Network.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using static Network.Packet.Response.Types;

[Serializable]
public class DestroyDebrisInfo
{
    public GameObject DebrisPrefab;
    public Transform DebrisTransform;
}

public class ReplicatedWaveBlockingStoneActor : MonoBehaviour
{
    [SerializeField] private ReplicatedEntityData mEntityData;
    [SerializeField] private List<DestroyDebrisInfo> mDestroyDebrisPrefab;

    public void OnEnable()
    {
        mEntityData.OnAction += OnAction;
        mIsDestroyed = false;
    }


    private bool mIsDestroyed = false;

    private void OnAction(EntityActionData actionData)
    {
        if (!actionData.HasAction)
        {
            return;
        }

        var actionType = actionData.Action;

        switch (actionType)
        {
            case EntityAction.kDestroy:
            case EntityAction.kDie:
                if (!mIsDestroyed)
                {
                    foreach (var i in mDestroyDebrisPrefab)
                    {
                        Instantiate(i.DebrisPrefab, i.DebrisTransform.position, i.DebrisTransform.rotation);
                    }

                    Destroy(mEntityData.gameObject);
                    mIsDestroyed = true;
                }
                break;
        }
    }


}