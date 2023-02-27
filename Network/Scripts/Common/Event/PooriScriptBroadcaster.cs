using Network;
using System;
using UnityEngine;

public class PooriScriptBroadcaster : BaseLocationEventTrigger
{
    public PooriScriptType PooriScriptType;

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ServerConfiguration.IS_SERVER)
        {
            ServerSessionManager.Instance.BroadcasePooriScriptToClients(PooriScriptType);
        }
    }
}