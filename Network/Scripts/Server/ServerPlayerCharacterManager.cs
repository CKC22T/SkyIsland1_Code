using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Network.Packet;
using Network.Server;
using Network;

using Utils;
using static Network.Packet.Request.Types;

public enum TeleportPositionType
{
    StartPosition,
    BossPosition,
}

public class ServerPlayerCharacterManager : LocalSingleton<ServerPlayerCharacterManager>
{
    private SessionSlotCollection SessionSlots => ServerSessionManager.Instance.UserSessionData.SessionSlots;
    private GameGlobalState GlobalState => ServerSessionManager.Instance.GameGlobalState.GameGlobalState;

    [SerializeField] private List<GameObject> PlayerPrefabs;
    [SerializeField] private Queue<EntityType> mReviveQueue = new();

    private Dictionary<int, List<Transform>> mCheckPointSpawnPositionTable = new();
    private Dictionary<TeleportPositionType, List<Transform>> mTeleportPosition = new();

    private MasterPlayerCharacterTable mPlayerTable = new();
    public List<MasterHumanoidEntityData> PlayerEntities => mPlayerTable.PlayerEntities;

    private Coroutine mPlayerHpRegen;

    private bool mIsInitialized = false;

    public CoroutineWrapper Wrapper;

    protected override void Awake()
    {
        base.Awake();

        if (Wrapper == null)
        {
            Wrapper = new CoroutineWrapper(CoroutineWrapper.CoroutineRunner.Instance);
        }
    }

    public void InitializeByManager()
    {
        if (mPlayerHpRegen != null)
        {
            StopCoroutine(mPlayerHpRegen);
        }
        mPlayerHpRegen = StartCoroutine(regenHpIfStickTogether());

        if (mIsInitialized)
            return;

        mIsInitialized = true;

        // Initialize Check Points
        foreach (var p in CheckPointManager.Instance.CheckPointSpawnPositionList)
        {
            mCheckPointSpawnPositionTable.Add(p.CheckPointNumber, p.PositionList);
        }

        // Initialize Teleport Points
        foreach (var t in CheckPointManager.Instance.TeleportPositionList)
        {
            mTeleportPosition.Add(t.TeleportType, t.TeleportList);
        }

        mReviveQueue.Enqueue(EntityType.kPlayerGriffin);
        mReviveQueue.Enqueue(EntityType.kPlayerPoopu);
        mReviveQueue.Enqueue(EntityType.kPlayerClo);
    }

    private IEnumerator regenHpIfStickTogether()
    {
        while (true)
        {
            yield return new WaitForSeconds(ServerConfiguration.PlayerHpRegenDelay);

            var players = PlayerEntities;

            for (int i = 0; i < players.Count; i++)
            {
                var currentPlayer = players[i];
                var currentPos = currentPlayer.Position.Value;

                for (int p = 0; p < players.Count; p++)
                {
                    if (i == p)
                    {
                        continue;
                    }

                    float distance = (players[p].Position.Value - currentPos).sqrMagnitude;

                    if (distance < ServerConfiguration.PlayerHpRegenDistance)
                    {
                        currentPlayer.ActionRegenHp();
                        break;
                    }
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void Clear()
    {
        mCheckPointSpawnPositionTable.Clear();
        mReviveQueue.Clear();
        mPlayerTable.Clear();

        mIsInitialized = false;
    }

    public Queue<int> GetDeadPlayerQueue()
    {
        Queue<int> deadPlayerQueue = new();

        foreach (var playerType in mReviveQueue)
        {
            if (mPlayerTable.TryGetPlayerCharacterSetByType(playerType, out var p))
            {
                deadPlayerQueue.Enqueue(p.SessionID);
            }
        }

        return deadPlayerQueue;
    }

    /// <summary>플레이어가 체크포인트에 도달했을 때 죽은 플레이어를 부활시키고 이전 영역에 있는 플레이어를 텔레포트 시킵니다.</summary>
    public void OnPlayerReachCheckPoint(int checkPoint)
    {
        if (!mCheckPointSpawnPositionTable.ContainsKey(checkPoint))
        {
            InitializeByManager();

            string message = $"There is no check point! Check Point : {checkPoint}";
            Debug.LogError(LogManager.GetLogMessage(message, NetworkLogType.MasterServer, true));
            return;
        }

        // Kill every enemy entities
        if (ServerMasterEntityManager.TryGetInstance(out var entityManager))
        {
            entityManager.KillEntities(FactionType.kEnemyFaction_1);
            entityManager.KillEntities(FactionType.kEnemyFaction_2);
            entityManager.KillEntities(FactionType.kEnemyFaction_3);
            entityManager.KillEntities(FactionType.kEnemyFaction_4);
        }

        // Teleport and regen characters
        var spawnPointList = mCheckPointSpawnPositionTable[checkPoint];

        int indexer = 0;
        int count = spawnPointList.Count;

        foreach (var p in PlayerEntities)
        {
            p.ActionFullRegenHp();

            indexer = indexer >= count ? count -1 : indexer;

            if (p.ShouldTeleport)
            {
                p.Teleport(spawnPointList[indexer].position);
            }

            indexer++;
        }

        // Revive Characters
        while (!mReviveQueue.IsEmpty())
        {
            indexer = indexer >= count ? count -1 : indexer;

            EntityType reviveEntityType = mReviveQueue.Dequeue();
            createPlayerCharacter(reviveEntityType, spawnPointList[indexer]);

            indexer++;
        }

        foreach (var p in PlayerEntities)
        {
            p.ShouldTeleport = true;
        }
    }

    public void TeleportToDestinationList(List<Transform> teleportPositions)
    {
        int indexer = 0;
        int count = teleportPositions.Count;

        foreach (var p in PlayerEntities)
        {
            p.ActionFullRegenHp();

            indexer = indexer >= count ? count - 1 : indexer;

            if (p.ShouldTeleport)
            {
                p.Teleport(teleportPositions[indexer].position);
            }

            indexer++;
        }
    }

    public void TeleportToDestination(TeleportPositionType teleportPositionType)
    {
        // Teleport and regen characters
        if (!mTeleportPosition.TryGetValue(teleportPositionType, out var teleportList))
        {
            return;
        }

        int indexer = 0;
        int count = teleportList.Count;

        foreach (var p in PlayerEntities)
        {
            p.ActionFullRegenHp();

            indexer = indexer >= count ? count - 1 : indexer;

            if (p.ShouldTeleport)
            {
                p.Teleport(teleportList[indexer].position);
            }

            indexer++;
        }
    }

    public void LockPlayerAI(float lockingTime)
    {
        Wrapper.StartSingleton(lockPlayerCharacter(lockingTime));
    }

    public void SetPlayerImmortal(bool isImmortal)
    {
        foreach (var entity in PlayerEntities)
        {
            entity.SetImmortal(isImmortal);
        }
    }

    private IEnumerator lockPlayerCharacter(float lockingTime)
    {
        foreach (MasterHumanoidEntityData entity in PlayerEntities)
        {
            if (entity.BindedClientID.Value < 0)
            {
                entity.ActionManager.SetAction(entity.ActionManager.IdActions["Off"]);
            }
        }

        yield return new WaitForSeconds(lockingTime);

        foreach (MasterHumanoidEntityData entity in PlayerEntities)
        {
            if (entity.BindedClientID.Value < 0)
            {
                entity.ActionManager.SetAction(entity.ActionManager.IdActions["Idle"]);
            }
        }
    }

    public void LockPlayer(bool isLock)
    {
        if (isLock)
        {
            foreach (MasterHumanoidEntityData entity in PlayerEntities)
            {
                if (entity.BindedClientID.Value < 0)
                {
                    entity.ActionManager.SetAction(entity.ActionManager.IdActions["Off"]);
                }
                else
                {
                    entity.InputMoveDirection = Vector2.zero;
                }
            }
        }
        else
        {
            foreach (MasterHumanoidEntityData entity in PlayerEntities)
            {
                if (entity.BindedClientID.Value < 0)
                {
                    entity.ActionManager.SetAction(entity.ActionManager.IdActions["Idle"]);
                }
                else
                {
                    entity.InputMoveDirection = Vector2.zero;
                }
            }
        }
    }

    private MasterHumanoidEntityData createPlayerCharacter(EntityType entityType, Transform spawnTransfrom)
    {
        Vector3 spawnPosition = spawnTransfrom.position;
        Quaternion spawnRotation = spawnTransfrom.rotation;

        if (mPlayerTable.HasPlayerByType(entityType))
        {
            return null;
        }

        if (SessionSlots.TryGetSlot(entityType, out var playerSlot))
        {
            var playerSessionID = playerSlot.SessionID.Value;

            if (ServerMasterEntityManager.TryGetInstance(out var entityManager))
            {
                var playerEntity = entityManager.CreateNewEntity(
                    entityType,
                    FactionType.kPlayer,
                    spawnPosition,
                    spawnRotation,
                    true,
                    DestroyPlayerCharacter,
                    playerSessionID) as MasterHumanoidEntityData;

                // Bind to player
                playerEntity.BindOperator(playerSessionID);
                playerEntity.ActionForceEquipWeapon(playerSlot.Inventory.GetEquipWeapon());

                mPlayerTable.TryAdd(playerSessionID, playerEntity);

                // Notifiy to session manager
                SessionSlots.NotifierEntityID(playerSessionID, playerEntity.EntityID);

                return playerEntity;
            }
        }

        return null;
    }

    public void SwapWeapon(int sessionID, ItemType weaponType)
    {
        if (mPlayerTable.TryGetPlayerEntityByID(sessionID, out var entity))
        {
            entity?.ActionForceEquipWeapon(weaponType);
        }
    }

    public bool HasCharacter(MasterHumanoidEntityData entity)
    {
        return mPlayerTable.HasPlayerByEntity(entity);
    }

    private void DestroyPlayerCharacter(int playerEntityID)
    {
        if (mPlayerTable.TryGetPlayerCharacterSetByEntityID(playerEntityID, out var playerCharacterSet))
        {
            ServerSessionManager.Instance.OnPlayerDie(playerCharacterSet.SessionID);
            mReviveQueue.Enqueue(playerCharacterSet.Entity.EntityType);
            mPlayerTable.Remove(playerCharacterSet.SessionID);
            mPlayerTable.BindOperator();

            SessionSlots.NotifierEntityID(playerCharacterSet.SessionID, -1);
        }

        if (!mPlayerTable.HasAnyAlivePlayer())
        {
            ServerSessionManager.Instance.RestartAtLsatCheckPoint();
        }
    }

    public void HandleUserInput(int clientID, ulong packetID, InputData inputData)
    {
        if (mPlayerTable.TryGetPlayerEntityByID(clientID, out var entity))
        {
            entity.ProcessInputFromPlayer(inputData, packetID);
        }
    }

    /// <summary>클라이언트 ID를 기준으로, 해당 클라이언트가 조종하고 있는 플레이어 Entity ID를 반환합니다. 조종하고 있는 Entity가 없다면 -1을 반환합니다.</summary>
    public int GetPlayerEntityIdByClientID(int sessionID)
    {
        if (mPlayerTable.TryGetPlayerCharacterSetByID(sessionID, out var playerCharacterSet))
        {
            return playerCharacterSet.SessionID;
        }

        return -1;
    }

    public bool TryGetPlayerEntityBySessionID(int sessionID, out MasterHumanoidEntityData entity)
    {
        return mPlayerTable.TryGetPlayerEntityByID(sessionID, out entity);
    }

    public void TryRegenHp(int sessionID)
    {
        if (mPlayerTable.TryGetPlayerEntityByID(sessionID, out var entity))
        {
            entity.ActionFullRegenHp();
        }
    }
}
