using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class CheckPointDoorEventTrigger : BaseLocationEventTrigger
{
    public float DoorOpenDelay = 4f;
    public List<BaseLocationEventTrigger> OnDoorOpen = new();

    private CoroutineWrapper Wrapper;

    public void Awake()
    {
        if (Wrapper == null)
        {
            Wrapper = new CoroutineWrapper(CoroutineWrapper.CoroutineRunner.Instance);
        }
    }

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ServerConfiguration.IS_SERVER)
        {
            Wrapper.StartSingleton(doorOpenDelay(DoorOpenDelay));
        }
    }

    public IEnumerator doorOpenDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (ServerConfiguration.IS_SERVER)
        {
            ServerSessionManager.Instance.GameGlobalState.GameGlobalState.LastCheckPointDoor.Value = true;
        }

        foreach (var e in OnDoorOpen)
        {
            e.TriggeredEvent(null);
        }
    }
}
