using Network.Client;
using Network.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheckPointTeleportEvent : BaseLocationEventTrigger
{
    public int checkPointNumber;
    public List<Transform> teleportPositions;

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ClientWorldManager.TryGetInstance(out var worldManager))
        {
            return;
        }

        int spawnCount = 0;
        foreach (var player in ServerPlayerCharacterManager.Instance.PlayerEntities)
        {
            //To Do : player가 죽었다면 스폰 장소에 재생성 코드

            if (player.ShouldTeleport)
            {
                player.Teleport(teleportPositions[spawnCount].position);
                ++spawnCount;
                if(spawnCount >= teleportPositions.Count)
                {
                    break;
                }
            }
        }
    }
}

