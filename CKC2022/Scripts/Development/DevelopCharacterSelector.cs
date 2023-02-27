using CulterLib.Presets;
using Network.Client;
using Network.Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Network.Packet.Request.Types;

public class DevelopCharacterSelector : MonoBehaviour
{
    void Update()
    {
        for (int i = (int)KeyCode.Alpha0; i < (int)KeyCode.Alpha5; ++i)
        {
            if (Input.GetKeyDown((KeyCode)i))
            {
                if (ConvertToEntityID(i - (int)KeyCode.Alpha0, out var entityID))
                {
                    SendRequestEntityBindData(entityID);
                    InitializedDataContrainer.Instance.SelectedCharacterID.Value = i - (int)KeyCode.Alpha0 + 1;
                }
            }
        }
    }
    public static void SendRequestEntityBindData(int entityID)
    {
        //var entityBindBuilder = RequestEntityBindData.CreateBuilder()
        //    .SetBindRequestEntityId(entityID);

        //var builder = ClientNetworkManager.Instance.GetRequestBuilder(RequestHandle.kRequestEntityBind)
        //    .SetRequestEntityBind(entityBindBuilder);

        //ClientNetworkManager.Instance.SendToServerViaTcp(builder.Build());
    }

    public static bool ConvertToEntityID(int characterID, out int entityID)
    {
        entityID = 0;

        if (!ClientWorldManager.TryGetInstance(out var clientWorldManager))
            return false;

        if (!clientWorldManager.TryGetEntities<ReplicatedEntityData>(EntityBaseType.Humanoid, IsPlayerEntity, out var list))
            return false;

        var target = list.Find(entity => entity.EntityType == (EntityType.kPlayerGriffin + (characterID - 1)));

        if (target == null)
            return false;

        entityID = target.EntityID;

        return true;

        // Local Predication Function
        bool IsPlayerEntity(ReplicatedEntityData entity)
        {
            return EntityType.kHumanoid < entity.EntityType && entity.EntityType < EntityType.kLastPlayerEntity;
        }
    }
}