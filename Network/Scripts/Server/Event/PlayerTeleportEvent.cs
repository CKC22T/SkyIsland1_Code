using Network.Client;
using Network.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTeleportEvent : BaseLocationEventTrigger
{
    public Transform teleportPosition;

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ClientWorldManager.TryGetInstance(out var worldManager))
        {
            return;
        }

        var player = other as MasterHumanoidEntityData;
        if(player != null)
        {
            player.Teleport(teleportPosition.position);
        }
    }
}
