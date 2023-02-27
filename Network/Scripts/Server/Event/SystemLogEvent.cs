using Network.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemLogEvent : BaseLocationEventTrigger
{


    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ClientWorldManager.TryGetInstance(out var worldManager))
        {
            return;
        }

        //OnSystemMessage
    }
}
