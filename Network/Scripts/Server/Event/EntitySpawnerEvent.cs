using Network.Packet;
using Network.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawnerEvent : BaseLocationEventTrigger
{
    public EntityType SpawnType;
    public FactionType Faction;
    public ItemType WeaponType;

    public override void TriggeredEvent(BaseEntityData other)
    {
        CreateEntity();
    }

    public bool TrySpawnEvent(BaseEntityData other, out MasterEntityData masterEntityData)
    {
        masterEntityData = CreateEntity();
        return masterEntityData != null;
    }

    private MasterEntityData CreateEntity()
    {
        if (!ServerMasterEntityManager.TryGetInstance(out var EntityManager))
            return null;

        var entityData = EntityManager.CreateNewEntity(SpawnType, Faction, transform.position, transform.rotation, true);
        switch (entityData)
        {
            case MasterHumanoidEntityData humanoid when WeaponType.IsWeapon():
                humanoid.ActionForceEquipWeapon(WeaponType);
                break;

            default: break;
        }

        return entityData;
    }
}
