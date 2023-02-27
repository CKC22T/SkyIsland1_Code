using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBlockingRockController : MonoBehaviour
{
    public GameObject BossRockObject;

    public void FixedUpdate()
    {
        bool isRockOn;

        if (ServerConfiguration.IS_SERVER)
        {
            isRockOn = ServerSessionManager.Instance.GameGlobalState.GameGlobalState.BossBlockingRock.Value;
        }
        else
        {
            isRockOn = ClientSessionManager.Instance.GameGlobalState.GameGlobalState.BossBlockingRock.Value;
        }

        BossRockObject.SetActive(isRockOn);
    }
}
