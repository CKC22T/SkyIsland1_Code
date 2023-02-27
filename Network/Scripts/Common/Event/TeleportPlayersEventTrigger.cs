using Network;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayersEventTrigger : BaseLocationEventTrigger
{
    public List<Transform> mTeleportPositions = new();

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ServerConfiguration.IS_SERVER)
        {
            if (ServerPlayerCharacterManager.TryGetInstance(out var serverPlayerCharacterManager))
            {
                serverPlayerCharacterManager.TeleportToDestinationList(mTeleportPositions);
            }
        }
    }
}
