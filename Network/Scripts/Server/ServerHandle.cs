using UnityEngine;

using Network.Packet;

namespace Network.Server
{
    public static class ServerHandle
    {
        public static void UpdateInput(int fromSession, Request packet)
        {
            ServerPlayerCharacterManager.Instance.HandleUserInput(fromSession, packet.PakcetId, packet.UpdateInput);
        }

        public static void RequestWeaponEquipWeapon(int fromSession, Request packet)
        {
            //ServerWorldManager.Instance.HandleEquipWeaponPacket(fromSession, packet.RequestEquipWeapon);
        }

        public static void RequestItemObjectObtain(int fromSession, Request packet)
        {
            if (!packet.HasRequestItemObjectObtainData ||
                !packet.RequestItemObjectObtainData.HasItemObjectId)
            {
                return;
            }

            if (!ItemObjectManager.TryGetInstance(out var manager))
            {
                return;
            }

            int itemID = packet.RequestItemObjectObtainData.ItemObjectId;
            var itemType = manager.GetItemTypeByID(itemID);

            if (ServerSessionManager.Instance.TryObtainWeapon(fromSession, itemType))
            {
                manager.DestroyItemObjectAsMaster(itemID);
            }
        }

        public static void RequestItemObjectDropWeapon(int fromSession, Request packet)
        {
            ServerSessionManager.Instance.DropWeapon(fromSession);
        }

        public static void RequestSwapWeapon(int fromSession, Request packet)
        {
            if (!packet.HasSwapInventoryIndex)
            {
                return;
            }

            var swapIndex = packet.SwapInventoryIndex;
            ServerSessionManager.Instance.SwapWeapon(fromSession, swapIndex);
        }

        public static void RequestTryObtainCheckPointItem(int fromSession, Request packet)
        {
            if (!packet.HasCheckPointNumber)
            {
                return;
            }

            ServerSessionManager.Instance.TryObtainCheckPointWeapon(fromSession, packet.CheckPointNumber);
        }
    }
}
