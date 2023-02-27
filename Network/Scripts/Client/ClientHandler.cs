using UnityEngine;

using Network.Packet;
using System;

namespace Network.Client
{
    public static class ClientHandler
    {

        public static void ServerClosed(Response responsePacket)
        {

        }

        #region Entities Handling

        public static void InitializeEntitiesSpwanData(Response responsePacket)
        {
            if (!ClientWorldManager.TryGetInstance(out var manager))
                return;

            manager.UpdateEntitySpawnData(responsePacket.PakcetId, responsePacket.EntitySpawnDataList);

            //ClientNetworkManager.Instance.
        }

        public static void UpdateEntitySpawnData(Response responsePacket)
        {
            if (!ClientWorldManager.TryGetInstance(out var manager))
                return;

            manager.UpdateEntitySpawnData(responsePacket.PakcetId, responsePacket.EntitySpawnDataList);
        }

        public static void UpdateEntityStatesData(Response responsePacket)
        {
            if (!ClientWorldManager.TryGetInstance(out var manager))
                return;

            manager.UpdateEntityStatesData(responsePacket.PakcetId, responsePacket.EntityStateDataList);
        }

        public static void UpdateEntityTransformData(Response responsePacket)
        {
            if (!ClientWorldManager.TryGetInstance(out var manager))
                return;

            manager.UpdateEntityTransformData(responsePacket.PakcetId, responsePacket.EntityTransformDataList);
        }

        public static void UpdateEntityActionData(Response responsePacket)
        {
            if (!ClientWorldManager.TryGetInstance(out var manager))
                return;

            manager.UpdateEntityActionData(responsePacket.PakcetId, responsePacket.EntityActionDataList);
        }

        #endregion

        #region Locator Handling

        public static void UpdateLocatorStateData(Response responsePacket)
        {
            if (LocatorEventManager.TryGetInstance(out var locatorEventManager))
            {
                locatorEventManager.UpdateLocatorStateDataAsRemote(responsePacket.LocatorStateDataList);
            }
        }

        public static void UpdateLocatorActionData(Response responsePacket)
        {
            if (LocatorEventManager.TryGetInstance(out var locatorEventManager))
            {
                locatorEventManager.UpdateLocatorActionAsRemote(responsePacket.LocatorActionDataList);
            }
        }

        #endregion

        #region Item Object Handling

        public static void UpdateItemObjectStateData(Response responsePacket)
        {
            if (ItemObjectManager.TryGetInstance(out var itemObjectManager))
            {
                itemObjectManager.UpdateItemObjectStateDataAsRemote(responsePacket.ItemObjectStateDataList);
            }
        }

        public static void UpdateItemObjectActionData(Response responsePacket)
        {
            if (ItemObjectManager.TryGetInstance(out var itemObjectManager))
            {
                itemObjectManager.UpdateItemObjectActionAsRemote(responsePacket.ItemObjectActionDataList);
            }
        }

        #endregion

        public static  event Action<string> OnCinemaCall;

        public static void RemoteCallCinema(Response responsePacket)
        {
            if (!responsePacket.HasRemotePlayCinemaName)
            {
                string message = $"There is no cinema type on remote cinema call packet!";

                Debug.LogError(LogManager.GetLogMessage(message, NetworkLogType.MasterClient, true));
                return;
            }

            string cinemaName = responsePacket.RemotePlayCinemaName;

            OnCinemaCall?.Invoke(cinemaName);
        }

        public static void UpdateDetectorActionData(Response requestPacket)
        {
            if (!ClientWorldManager.TryGetInstance(out var manager))
                return;

            manager.UpdateDetectorActionData(requestPacket.DetectorActionDataList);
        }

        public static void ErrorListener(Response responsePacket)
        {
            Debug.LogError(responsePacket.LogMessage);
        }

        public static void UpdateGameFrame(ref NetBuffer packet)
        {
            ClientRemoteReplicatorManager.Instance?.ReadFromBuffer(ref packet);
        }
    }
}