using Network.Packet;
using UnityEngine;
public class ItemSpawnEvent : BaseLocationEventTrigger
{
    public ItemType SpawnItemType;
    public Vector3 SpawnForce;

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ItemObjectManager.TryGetInstance(out var itemObjectManager))
        {
            itemObjectManager.CreateItemObjectAsMaster(SpawnItemType, transform.position, Quaternion.identity, SpawnForce);
        }
    }
}
