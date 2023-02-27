using Network.Server;
using Network.Packet;
using UnityEngine;

public class CheckPointBoundary : BaseLocationEventTrigger
{
    public bool ShouldTeleport = false;

    public override void TriggeredEvent(BaseEntityData other)
    {
       NetworkMode networkMode = LocatorEventManager.Instance.NetworkMode;

        if (networkMode == NetworkMode.Master)
        {
            var entity = other as MasterHumanoidEntityData;
            if (entity != null && entity.EntityType.IsPlayerEntity())
            {
                entity.ShouldTeleport = ShouldTeleport;
            }
        }
    }
}
