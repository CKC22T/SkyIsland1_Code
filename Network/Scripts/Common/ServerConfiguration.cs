using Network.Packet;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Network
{
    public class ServerConfiguration
    {
        public const int DATA_BUFFER_SIZE = 3000;//4096;
        public const int MAX_BUFFER_CAPACITY_SIZE = 65536 * 20;//4096;

        public const int SERVER_PHYSICS_TICK = 60;
        public const float SERVER_TICK_DELTA_TIME = 1.0f / SERVER_PHYSICS_TICK;

        public const int SERVER_NETWORK_TICK = 30;
        public const float SERVER_NETWORK_DELTA_TIME = 1.0f / SERVER_NETWORK_TICK;

        public const int MAX_PLAYER = 3;
        public const int MAX_WEAPON_ITEM_INVENTORY = 3;

        public const float ServerTimeoutSeconds = 5;
        public const int ServerInitialPortNumber = 50000;
        public const int TcpReceiveTimeout = 3000;
        public const int TcpSendTimeout = 3000;

        public const int UdpCheckTimeout = 3000;
        public const int UdpCheckDelay = 500;

        public const int MaxReceivedNetBufferCount = 10000;

        /// <summary>최대 전송 가능한 TCP 패킷 청크 바이트 길이입니다.</summary>
        public const int MAX_TCP_PACKET_LENGTH = 8192; // 8kb
        /// <summary>최대 전송 가능한 UDP 패킷 청크 바이트 길이입니다.</summary>
        public const int MAX_UDP_PACKET_LENGTH = 1024; // 1kb
        /// <summary>예상되는 패킷 헤더의 길이입니다.</summary>
        public const int PACKET_HEADER_LENGTH = 32;
        public const int UDP_HEARTBEAT_FREQUENCY = 1500; // Milliseconds

        /// <summary>Replication Object가 미리 할당될 수 있는 수치입니다. 새로운 Replication Object는 이 번호 이후로 할당받습니다.</summary>
        public const int REPLICATOR_INITIAL_COUNTER_INDEX_OFFSET = 100000;

        public static bool IS_SERVER = false;
        public static bool IS_CLIENT { get => !IS_SERVER; }

        public static float MaxLatency = 0.5f;
        public static float CinemaFadeoutDelay = 1.0f;

        // In Game
        public static float DropForcePower = 10.0f;
        public static ItemType PlayerDefaultWeapon = ItemType.kWeaponWiseMansWand;
        public static Vector3 HumanoidWeaponDropPosition = new Vector3(0f, 0.8f, 0f);

        public static float GameOverCutsceneDelay = 3.0f;
        public static float FriendlyFireDamageReduceRatio = 0.2f;
        public static float CheckPointInteractDistance = 5.0f;
        public static float CheckPointActiveDelay = 3.5f;
        public static float PlayerHpRegenDelay = 1.0f;
        public static float PlayerHpRegenDistance = 10.0f;

        public const int MAX_CHECK_POINT_COUNT = 6;
        public const ItemType WispDefaultWeapon = ItemType.kWeaponWiseMansWand;

        public const float DefaultPooriPopupStayDelay = 0.3f;
        public const float DefaultPooriTextEachCharacterDelay = 0.03f;
        public const float PooriPopupTextingSpeed = 0.4f;

        public const int BridgeWaveCount = 5;
        public const int MaxBridgeWaveCountIndexer = BridgeWaveCount;

        public const int TurretCount = 5;

        public static bool TriggerStartInitialCutscene = false;
        public static bool IsJoinInGame = false;

        public static readonly List<EntityType> PlayerEntityTypes = new()
        {
            EntityType.kPlayerGriffin,
            EntityType.kPlayerPoopu,
            EntityType.kPlayerClo,
            EntityType.kPlayerDerin
        };

        public static readonly List<ItemType> DefaultWeaponItems = new()
        {
            // 없음
            ItemType.kNoneItemType,
            // 최초 무기 습득
            ItemType.kWeaponSwordOfTheWorldTree,
            ItemType.kWeaponLightningWand,
            ItemType.kWeaponKeyOfWisdom,
            ItemType.kWeaponHellishMace,
            ItemType.kWeaponNobleSacrifice,
        };
    }
}
