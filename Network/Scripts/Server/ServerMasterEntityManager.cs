using System.Collections.Generic;
using UnityEngine;

using Network.Server;
using Network.Packet;
using static Network.Packet.Response.Types;

using Utils;
using System;
using UnityEditor;
using Network;
using System.Collections;

public class ServerMasterEntityManager : LocalSingleton<ServerMasterEntityManager>
{

#if UNITY_EDITOR

    [Sirenix.OdinInspector.Button]
    public void SetEntities()
    {
        var humanoidPrefabs = GlobalPath.GetPrefabsFileFromPath(GlobalPath.HumanoidPrefabsPath, GlobalPath.MasterSuffix);
        var enemyPrefabs = GlobalPath.GetPrefabsFileFromPath(GlobalPath.EnemyPrefabsPath, GlobalPath.MasterSuffix);
        var structurePrefabs = GlobalPath.GetPrefabsFileFromPath(GlobalPath.StructurePrefabsPath, GlobalPath.MasterSuffix);

        EntityPrefabs = new List<GameObject>();

        EntityPrefabs.AddRange(humanoidPrefabs);
        EntityPrefabs.AddRange(enemyPrefabs);
        EntityPrefabs.AddRange(structurePrefabs);
    }

#endif

    [SerializeField] private List<GameObject> EntityPrefabs;
    private readonly Dictionary<EntityType, GameObject> mEntityPrefabTalbe = new();
    private readonly Dictionary<int, MasterEntityData> mEntities = new();

    public Dictionary<int, MasterEntityData>.ValueCollection Entites => mEntities.Values;

    // Action data
    private List<EntityActionData> mUdpEntityActionDataBuffer = new();
    private List<EntityActionData> mTcpEntityActionDataBuffer = new();

    private bool mIsInitialized = false;
    public void OnServerStarted()
    {
        // Initialize entity prefabs by EntityType
        foreach (var go in EntityPrefabs)
        {
            MasterEntityData entity = go.GetComponent<MasterEntityData>();

            if (entity == null)
            {
                Debug.LogError(LogManager.GetLogMessage($"Entity prefab initialize failed! Cannot found \"MasterEntityData\" " +
                    $"from binded prefabs! Object name : {go.name}", NetworkLogType.EntityManager, true));
                continue;
            }

            mEntityPrefabTalbe.Add(entity.EntityType, go);
        }

        mIsInitialized = true;
    }

    public void Clear()
    {
        mEntities.Clear();
    }

    /// <summary>해당하는 Entity의 EntityType을 반환합니다. 타입이 없거나 존재하지 않는 Entity인 경우 kNoneEntityType을 반환합니다.</summary>
    public EntityType GetEntityTypeByEntityID(int entityID)
    {
        var entity = mEntities.GetValueOrNull(entityID);
        if (entity == null)
        {
            return EntityType.kNoneEntityType;
        }
        else
        {
            return entity.EntityType;
        }
    }

    public MasterEntityData GetEntityOrNull(int entityID)
    {
        return mEntities.GetValueOrNull(entityID);
    }

    /// <summary>엔티티를 생성합니다.</summary>
    /// <param name="isEnabled">엔티티가 처음 생성될 때 활성화 되었는지 여부입니다.</param>
    /// <param name="destroyEvent">엔티티가 제거될 때 호출되는 Event입니다.</param>
    /// <returns>생성된 엔티티입니다.</returns>
    public MasterEntityData CreateNewEntity(EntityType entityType, FactionType faction, Vector3 position, Quaternion rotation, bool isEnabled, Action<int> destroyEvent = null, int bindedClientID = -1)
    {
        // Get entity prefabs from prefab table
        if (!mEntityPrefabTalbe.ContainsKey(entityType))
        {
            Debug.LogError(LogManager.GetLogMessage($"There is no such thing as \"{entityType}\".", NetworkLogType.EntityManager, true));
            return null;
        }

        // Poolling and create GameObject
        GameObject go = PoolManager.SpawnObject(mEntityPrefabTalbe[entityType], position, rotation);
        var createdEntityData = go.GetComponent<MasterEntityData>();

        // Set destroy event
        Action<int> destroyAction = null;

        if (destroyEvent != null)
        {
            destroyAction = destroyEvent;
        }

        destroyAction += DestoryEntityByID;

        int newEntityID = ServerWorldManager.Instance.NewEntityID;

        createdEntityData.Initialize(newEntityID, faction, position, rotation, isEnabled, destroyAction);
        createdEntityData.BindedClientID.Value = bindedClientID;

        mEntities.Add(newEntityID, createdEntityData);

        // Move current created object to physics scene
        ServerWorldManager.Instance.MoveGameObjectToScene(go);

        // Send spawn data to all
        SendEntitySpawnDataToAll(createdEntityData.GetSpawnData());

        return createdEntityData;
    }

    /// <summary>엔티티를 제거합니다.</summary>
    public void DestoryEntityByID(int entityID)
    {
        if (mEntities.ContainsKey(entityID))
        {
            var destoryEntity = mEntities[entityID];

            // Remove entity and add action to buffer before destroy
            destoryEntity.TcpEntityActionDataBuffer.MoveTo(ref mTcpEntityActionDataBuffer);
            destoryEntity.UdpEntityActionDataBuffer.MoveTo(ref mUdpEntityActionDataBuffer);

            PoolManager.ReleaseObject(destoryEntity.gameObject);
            mEntities.Remove(entityID);
        }
    }

    public void KillEntity(int entityID)
    {
        if (mEntities.TryGetValue(entityID, out var entity))
        {
            entity.ActionDie();
        }
    }

    public void KillEntities(FactionType factionType)
    {
        Queue<MasterEntityData> entities = new Queue<MasterEntityData>();

        foreach (var entity in mEntities.Values)
        {
            if (entity.FactionType == factionType)
            {
                entities.Enqueue(entity);
            }
        }

        while (!entities.IsEmpty())
        {
            var e = entities.Dequeue();
            e.ActionDie();
        }
    }


    #region Packet Builder

    private Response.Builder getEntitiesSpawnDataPacketBuilder()
    {
        if (mEntities.IsEmpty())
            return null;

        var builder = DedicatedServerManager.Instance.GetBaseResponseBuilder(ResponseHandle.kUpdateEntitySpawnData);

        foreach (var i in mEntities.Keys)
        {
            var spawnData = mEntities[i].GetSpawnData();
            builder.AddEntitySpawnData(spawnData);
        }

        return builder;
    }

    #endregion

    #region Server Sender

    public void SendEntitySpawnDataToAll(EntitySpawnData.Builder entitySpawnData)
    {
        var builder = DedicatedServerManager.Instance.GetBaseResponseBuilder(ResponseHandle.kUpdateEntitySpawnData)
            .AddEntitySpawnData(entitySpawnData);

        DedicatedServerManager.Instance.SendToAllClient_TCP(builder.Build());
    }

    public void SendInitialEntitiesSpawnDataTo(int sessionID)
    {
        var packetBuilder = getEntitiesSpawnDataPacketBuilder();

        if (packetBuilder == null)
		{
            return;
		}
        else
        {
            DedicatedServerManager.Instance.SendToClient_TCP(sessionID, packetBuilder.Build());
        }
    }

    [Obsolete("Test용 함수")]
    public void SendEntitiesSpawnDataToAll()
    {
        var packetBuilder = getEntitiesSpawnDataPacketBuilder();

        if (packetBuilder == null)
        {
            return;
        }
        else
        {
            DedicatedServerManager.Instance.SendToAllClient_TCP(packetBuilder.Build());
        }
    }

    public void SendInitialEntitiesStateDataTo(int sessionID)
    {
        if (mEntities.IsEmpty())
            return;

        var builder = DedicatedServerManager.Instance.GetBaseResponseBuilder(ResponseHandle.kUpdateEntityStatesData);

        foreach (MasterEntityData e in mEntities.Values)
        {
            var data = e.GetAllEntityStateData();
            builder.AddEntityStateData(data);
        }

        DedicatedServerManager.Instance.SendToClient_TCP(sessionID, builder.Build());
    }

    [Obsolete("Entity의 이전 발생 액션 동기화는 현 시점에서 필요하지 않다.")]
    public void SendEntitiesActedActionDataTo(int sessionID)
    {
        var builder = DedicatedServerManager.Instance.GetBaseResponseBuilder(ResponseHandle.kUpdateEntityActionData);

        int actionDataCount = 0;

        foreach (var entity in mEntities.Values)
        {
            var baseType = entity.EntityType.GetEntityBaseType();
            if (baseType == EntityBaseType.Humanoid)
            {
                var humanoidEntity = entity as MasterHumanoidEntityData;
                var actionData = humanoidEntity.GetActedActionData();

                if (actionData == null)
                    continue;

                builder.AddEntityActionData(actionData);

                actionDataCount++;
            }
        }

        if (actionDataCount > 0)
        {
            DedicatedServerManager.Instance.SendToClient_TCP(sessionID, builder.Build());
        }
    }

    public void SendEntityStatesDataToAll()
    {
        if (mEntities.IsEmpty())
            return;

        var builder = DedicatedServerManager.Instance.GetBaseResponseBuilder(ResponseHandle.kUpdateEntityStatesData);

        bool hasData = false;

        foreach (MasterEntityData e in mEntities.Values)
        {
            if (e.TryGetChangedEntityStateDataOrNull(out var data))
            {
                builder.AddEntityStateData(data);
                hasData = true;
            }
        }

        if (hasData)
        {
            DedicatedServerManager.Instance.SendToAllClient_TCP(builder.Build());
        }
    }

    public void SendEntityTransformDataToAll()
    {
        if (mEntities.IsEmpty())
            return;

        var builder = DedicatedServerManager.Instance.GetBaseResponseBuilder(ResponseHandle.kUpdateEntityTransformData);

        bool hasData = false;

        foreach (MasterEntityData e in mEntities.Values)
        {
            if (e.TryGetEntityTransformData(out var data))
            {
                builder.AddEntityTransformData(data);
                hasData = true;
            }
        }

        if (hasData)
        {
            DedicatedServerManager.Instance.SendToAllClient_UDP(builder.Build());
        }
    }

    public void SendEntityActionDataViaUdpToAll()
    {
        if (mEntities.IsEmpty())
            return;

        foreach (var e in mEntities.Values)
        {
            e.UdpEntityActionDataBuffer.MoveTo(ref mUdpEntityActionDataBuffer);
        }

        if (mUdpEntityActionDataBuffer.IsEmpty())
            return;

        var builder = DedicatedServerManager.Instance.GetBaseResponseBuilder(ResponseHandle.kUpdateEntityActionData)
            .AddRangeEntityActionData(mUdpEntityActionDataBuffer);

        DedicatedServerManager.Instance.SendToAllClient_UDP(builder.Build());

        mUdpEntityActionDataBuffer.Clear();
    }

    public void SendEntityActionDataViaTcpToAll()
    {
        if (mEntities.IsEmpty())
            return;

        foreach (var e in mEntities.Values)
        {
            e.TcpEntityActionDataBuffer.MoveTo(ref mTcpEntityActionDataBuffer);
        }

        if (mTcpEntityActionDataBuffer.IsEmpty())
            return;

        var builder = DedicatedServerManager.Instance.GetBaseResponseBuilder(ResponseHandle.kUpdateEntityActionData)
            .AddRangeEntityActionData(mTcpEntityActionDataBuffer);

        DedicatedServerManager.Instance.SendToAllClient_TCP(builder.Build());

        mTcpEntityActionDataBuffer.Clear();
    }

    #endregion
}
