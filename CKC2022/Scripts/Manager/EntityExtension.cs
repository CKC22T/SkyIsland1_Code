using Network.Client;
using Network.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CKC2022
{
    public static class EntityExtension
    {
        //TODO : 냄새나는 코드. 수정 필요
        public static void SendRequestEntityBindData(int entityID)
        {
            //var entityBindBuilder = Request.Types.RequestEntityBindData.CreateBuilder()
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
}