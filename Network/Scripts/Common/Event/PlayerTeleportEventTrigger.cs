using UnityEngine;

public class PlayerTeleportEventTrigger : BaseLocationEventTrigger
{
    public TeleportPositionType TeleportType;

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ServerPlayerCharacterManager.TryGetInstance(out var playerManager))
        {
            playerManager.TeleportToDestination(TeleportType);
        }
    }
}