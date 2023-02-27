using Network;
using Network.Packet;
using Network.Server;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using static Network.Packet.Response.Types;

public class ItemObjectManager : LocalSingleton<ItemObjectManager>
{
#if UNITY_EDITOR

    [Sirenix.OdinInspector.Button]
    public void SetItems()
    {
        var itemPrefabs = GlobalPath.GetPrefabsFileFromPath(GlobalPath.ItemPrefabsPath, GlobalPath.ItemSuffix);

        ItemPrefabs = new List<GameObject>(itemPrefabs);
    }

#endif

    public List<GameObject> ItemPrefabs;

    [SerializeField] private List<ReplicableItemObject> mInitialReplicableItemObjects = new List<ReplicableItemObject>();
    [field: SerializeField] public NetworkMode NetworkMode { get; private set; } = NetworkMode.None;

    private Dictionary<ItemType, GameObject> mItemPrefabTable = new Dictionary<ItemType, GameObject>();
    private int mLastInitialItemObjectIndex = -1;
    private List<int> mDestroyedItemObjectIDs = new List<int>();
    private Dictionary<int, ReplicableItemObject> mReplicableItemObjects = new Dictionary<int, ReplicableItemObject>();
    public Dictionary<int, ReplicableItemObject>.ValueCollection ItemObjects => mReplicableItemObjects.Values;
    private bool mIsInitialized = false;

    private int mItemObjectId = 0;
    private int getNewItemObjectID() => mItemObjectId++;

    [Sirenix.OdinInspector.Button]
    private void setupReplicableItemObjects()
    {
        mInitialReplicableItemObjects = FindObjectsOfType<ReplicableItemObject>().ToList();
        Debug.Log(LogManager.GetLogMessage($"Replicable item list initialized! Count : {mInitialReplicableItemObjects.Count}", NetworkLogType.ItemObjectManager));
    }

    private Queue<ReplicableItemObject> ItemObjectsQueue
    {
        get
        {
            Queue<ReplicableItemObject> queue = new();

            foreach (var i in ItemObjects)
            {
                queue.Enqueue(i);
            }

            return queue;
        }
    }

    public void ClearAsMaster()
    {
        mIsInitialized = false;
        var destroyQueue = ItemObjectsQueue;

        while (!destroyQueue.IsEmpty())
        {
            var destroyInstance = destroyQueue.Dequeue();
            DestroyItemObjectAsMaster(destroyInstance.ItemObjectID);
        }
    }

    public void ClearAsRemote()
    {
        mIsInitialized = false;
        var destroyQueue = ItemObjectsQueue;

        while (!destroyQueue.IsEmpty())
        {
            var destroyInstance = destroyQueue.Dequeue();
            destroyItemObjectAsRemote(destroyInstance.ItemObjectID);
        }
    }

    public void InitializeByManager(NetworkMode networkMode)
    {
        if (mIsInitialized)
            return;

        // Set locator prefabs
        foreach (var prefab in ItemPrefabs)
        {
            var item = prefab.GetComponent<ReplicableItemObject>();

            if (item == null)
            {
                Debug.LogError(LogManager.GetLogMessage($"Item object prefab initialize failed! Cannot found \"ReplicableItemObject\" " +
                    $"from binded prefabs! Object name : {prefab.name}", NetworkLogType.ItemObjectManager, true));
                continue;
            }

            mItemPrefabTable.Add(item.ItemType, prefab);
        }

        // Set exist locators
        NetworkMode = networkMode;

        for (int i = 0; i < mInitialReplicableItemObjects.Count; i++)
        {
            var initialItemObject = mInitialReplicableItemObjects[i];
            initialItemObject.SetNetworkMode(NetworkMode);
            initialItemObject.SetItemObjectID(i);
            mReplicableItemObjects.Add(i, initialItemObject);
        }

        mLastInitialItemObjectIndex = mInitialReplicableItemObjects.Count;
        mItemObjectId = mInitialReplicableItemObjects.Count;

        mIsInitialized = true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        foreach (var item in mReplicableItemObjects.Values)
        {
            if (NetworkMode == NetworkMode.Remote)
            {
                item.OnDestroyAsRemote();
            }
            else
            {
                item.OnDestroyAsMaster();
            }
        }
    }

    #region Getter

    public bool TryGetItemByID(int itemObjectID, out ReplicableItemObject itemObject)
    {
        return mReplicableItemObjects.TryGetValue(itemObjectID, out itemObject);
    }

    public bool HasItem(ReplicableItemObject itemObject)
    {
        return mReplicableItemObjects.ContainsKey(itemObject.ItemObjectID);
    }

    public ItemType GetItemTypeByID(int itemObjectID)
    {
        if (mReplicableItemObjects.TryGetValue(itemObjectID, out var item))
        {
            return item.ItemType;
        }

        return ItemType.kNoneItemType;
    }

    #endregion

    #region Server Sender

    public void SendItemObjectStateUpdateData()
    {
        if (!DedicatedServerManager.TryGetInstance(out var server))
            return;

        var builder = server.GetBaseResponseBuilder(ResponseHandle.kUpdateItemObjectStateData);

        bool hasData = false;

        foreach (var i in ItemObjects)
        {
            if (i.TryGetChangedItemObjectStateDataOrNull(out var data))
            {
                builder.AddItemObjectStateData(data);
                hasData = true;
            }
        }

        if (hasData)
        {
            server.SendToAllClient_TCP(builder.Build());
        }
    }

    /// <summary>ItemObject의 초기 생성 데이터와 이미 소멸된 정보를 해당하는 클라이언트에게 전송합니다.</summary>
    public void SendInitialItemObjectLifeDataTo(int sessionID)
    {
        if (!DedicatedServerManager.TryGetInstance(out var server))
            return;

        var builder = server.GetBaseResponseBuilder(ResponseHandle.kUpdateItemObjectActionData);

        // Append already destroyed locator information
        foreach (var destroyItemObjectID in mDestroyedItemObjectIDs)
        {
            var destroyAction = ItemObjectActionData.CreateBuilder()
                .SetItemObjectAction(ObjectActionType.kDestroyed)
                .SetItemId(destroyItemObjectID);
            builder.AddItemObjectActionData(destroyAction);
        }

        // Append exsit locators information
        foreach (var e in ItemObjects)
        {
            var creationData = e.GetCreationAction();
            builder.AddItemObjectActionData(creationData);
        }

        var packet = builder.Build();

        server.SendToClient_TCP(sessionID, packet);
    }

    private void sendCreationActionToAll(ReplicableItemObject createdItemObject)
    {
        if (!DedicatedServerManager.TryGetInstance(out var server))
            return;

        var builder = server.GetBaseResponseBuilder(ResponseHandle.kUpdateItemObjectActionData);

        builder.AddItemObjectActionData(createdItemObject.GetCreationAction());

        server.SendToAllClient_TCP(builder.Build());
    }

    private void sendDestroyActionToAll(ReplicableItemObject destroyItemObject)
    {
        if (!DedicatedServerManager.TryGetInstance(out var server))
            return;

        var builder = server.GetBaseResponseBuilder(ResponseHandle.kUpdateItemObjectActionData);

        var destroyAction = ItemObjectActionData.CreateBuilder()
            .SetItemId(destroyItemObject.ItemObjectID)
            .SetItemObjectAction(ObjectActionType.kDestroyed);

        builder.AddItemObjectActionData(destroyAction);

        server.SendToAllClient_TCP(builder.Build());
    }

    #endregion

    #region Controll as Master

    public ReplicableItemObject CreateItemObjectAsMaster(ItemType itemType, Vector3 position, Quaternion rotation, Vector3 spanwForce)
    {
        if (NetworkMode != NetworkMode.Master)
        {
            return null;
        }

        // Get locator prefabs from prefab table
        if (!mItemPrefabTable.ContainsKey(itemType))
        {
            Debug.LogError(LogManager.GetLogMessage($"There is no such thing as \"{itemType}\".", NetworkLogType.ItemObjectManager, true));
            return null;
        }

        // Poolling and create GameObject
        GameObject go = PoolManager.SpawnObject(mItemPrefabTable[itemType]);
        var createdItemObject = go.GetComponent<ReplicableItemObject>();

        // Setup locator properties
        createdItemObject.SetNetworkMode(NetworkMode.Master);
        createdItemObject.SetItemObjectID(getNewItemObjectID());

        createdItemObject.SetState(position, rotation);
        createdItemObject.transform.position = position;
        createdItemObject.transform.rotation = rotation;

        // Add force as master
        createdItemObject.ForceAsMaster(spanwForce);

        // Send creation action to all clients
        sendCreationActionToAll(createdItemObject);

        // Add to management locator
        mReplicableItemObjects.Add(createdItemObject.ItemObjectID, createdItemObject);

        return createdItemObject;
    }

    public void DestroyItemObjectAsMaster(int itemObjectID)
    {
        if (NetworkMode != NetworkMode.Master)
        {
            return;
        }

        if (!mReplicableItemObjects.ContainsKey(itemObjectID))
            return;

        var destroyLocator = mReplicableItemObjects[itemObjectID];

        sendDestroyActionToAll(destroyLocator);

        mReplicableItemObjects.Remove(itemObjectID);
        PoolManager.ReleaseObject(destroyLocator.gameObject);

        // Cache initial locator destroy action
        if (itemObjectID <= mLastInitialItemObjectIndex)
        {
            if (!mDestroyedItemObjectIDs.Contains(itemObjectID))
            {
                mDestroyedItemObjectIDs.Add(itemObjectID);
            }
        }
    }

    #endregion

    #region Controll as Remote

    private void destroyItemObjectAsRemote(int itemObjectID)
    {
        if (mReplicableItemObjects.ContainsKey(itemObjectID))
        {
            var destroyItem = mReplicableItemObjects[itemObjectID];

            mReplicableItemObjects.Remove(itemObjectID);

            destroyItem.OnDestroyAsRemote();
        }
    }

    public void UpdateItemObjectStateDataAsRemote(IList<ItemObjectStateData> stateDataList)
    {
        foreach (var data in stateDataList)
        {
            int itemID = data.ItemId;

            if (mReplicableItemObjects.TryGetValue(itemID, out var itemObject))
            {
                itemObject.SetState(data);
            }
        }
    }

    public void UpdateItemObjectActionAsRemote(IList<ItemObjectActionData> itemObjectActionList)
    {
        foreach (var action in itemObjectActionList)
        {
            var actionType = action.ItemObjectAction;
            int itemObjectID = action.ItemId;

            switch (actionType)
            {
                case ObjectActionType.kCreated:
                    {
                        var creationData = action.ItemObjectCreationData;

                        // Create locator if it's not exist.
                        if (!mReplicableItemObjects.ContainsKey(itemObjectID))
                        {
                            CreateItemObjectAsRemote(itemObjectID, creationData.Type, creationData.Position.ToVector3(), creationData.Rotation.ToQuaternion());
                        }
                        else
                        {
                            var itemObject = mReplicableItemObjects[itemObjectID];
                            itemObject.SetState(creationData.Position.ToVector3(), creationData.Rotation.ToQuaternion());
                        }
                    }
                    break;

                case ObjectActionType.kDestroyed:
                    {
                        destroyItemObjectAsRemote(itemObjectID);
                    }
                    break;

                default:
                    Debug.LogError(LogManager.GetLogMessage($"Wrong ObjectActionType for item object! Type : {actionType}", NetworkLogType.ItemObjectManager, true));
                    break;
            }
        }
    }

    public ReplicableItemObject CreateItemObjectAsRemote(int itemObjectID, ItemType itemType, Vector3 position, Quaternion rotation)
    {
        if (NetworkMode != NetworkMode.Remote)
        {
            return null;
        }

        // Get locator prefabs from prefab table
        if (!mItemPrefabTable.ContainsKey(itemType))
        {
            Debug.LogError(LogManager.GetLogMessage($"There is no such thing as \"{itemType}\".", NetworkLogType.ItemObjectManager, true));
            return null;
        }

        // Poolling and create GameObject
        GameObject go = PoolManager.SpawnObject(mItemPrefabTable[itemType], position, rotation);
        var createdItemObejct = go.GetComponent<ReplicableItemObject>();

        // Setup locator properties
        createdItemObejct.SetNetworkMode(NetworkMode.Remote);
        createdItemObejct.Initialize(itemObjectID, itemType, position, rotation);

        // Add to management locator
        mReplicableItemObjects.Add(itemObjectID, createdItemObejct);

        return createdItemObejct;
    }

    #endregion
}
