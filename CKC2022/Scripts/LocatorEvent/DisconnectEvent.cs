using Network;
using Network.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectEvent : BaseLocationEventTrigger
{
    private string VictoryReason = "VICTORY";

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ServerConfiguration.IS_SERVER)
        {
            ServerSessionManager.Instance.StopServer(VictoryReason);
        }
    }
}
