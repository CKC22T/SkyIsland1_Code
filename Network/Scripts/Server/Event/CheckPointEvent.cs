using Network;
using Network.Client;
using Network.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointEvent : BaseLocationEventTrigger
{
    [SerializeField] private int checkPointNumber = 0;
    [SerializeField] private ReplicableLocator mCheckPointBoundary;
    [SerializeField] private CheckPointController mCheckPointController;
    [SerializeField] private List<BaseLocationEventTrigger> mOnCheckPointReach_remoteSide;
    [SerializeField] private List<BaseLocationEventTrigger> mOnCheckPointEventRaised;

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public override void TriggeredEvent(BaseEntityData other)
    {
        var networkMode = LocatorEventManager.Instance.NetworkMode;

        if (networkMode == NetworkMode.Remote)
        {
            mCheckPointController.EnableCheckPointEffect();
            foreach (var t in mOnCheckPointReach_remoteSide)
            {
                t.TriggeredEvent(other);
            }

            StartCoroutine(additionalEventCall(other));
            
        }
        else if (networkMode == NetworkMode.Master)
        {
            //mCheckPointBoundary.DestroyAsMaster();
            mCheckPointBoundary.SetActivation(false);
            StartCoroutine(checkPointEnable(other));
            StartCoroutine(additionalEventCall(other));
        }
    }

    public IEnumerator checkPointEnable(BaseEntityData other)
    {
        yield return new WaitForSeconds(ServerConfiguration.CheckPointActiveDelay);
        ServerSessionManager.Instance.OnPlayerReachCheckPoint(checkPointNumber);
    }

    public IEnumerator additionalEventCall(BaseEntityData other)
    {
        yield return new WaitForSeconds(ServerConfiguration.CheckPointActiveDelay);
        if (mOnCheckPointEventRaised != null)
        {
            foreach (var t in mOnCheckPointEventRaised)
            {
                t.TriggeredEvent(other);
            }
        }
    }
}
