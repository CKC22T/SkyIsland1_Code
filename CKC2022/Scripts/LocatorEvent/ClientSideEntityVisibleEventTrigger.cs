using Network.Client;
using System;
using UnityEngine;

class ClientSideEntityVisibleEventTrigger : BaseLocationEventTrigger
{
    public bool SetActive = false;

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ClientWorldManager.TryGetInstance(out var clientWorldManager))
        {
            clientWorldManager.SetAllEntitiesActivation(SetActive);
        }
    }
}
