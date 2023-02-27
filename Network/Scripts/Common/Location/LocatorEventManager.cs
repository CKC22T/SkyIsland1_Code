using Network;
using Network.Packet;
using Network.Server;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using static Network.Packet.Response.Types;

public class LocatorEventManager : LocalSingleton<LocatorEventManager>
{
    public List<GameObject> LocatorPrefab;

    [SerializeField] private List<ReplicableLocator> mInitialReplicableLocators = new List<ReplicableLocator>();
    [field : SerializeField] public NetworkMode NetworkMode { get; private set; } = NetworkMode.None;

    private Dictionary<LocatorType, GameObject> mLocatorPrefabTable = new Dictionary<LocatorType, GameObject>();
    private int mLastInitialLocatorIndex = -1;
    private List<int> mDestroyedLocatorIDs = new List<int>();
    private Dictionary<int, ReplicableLocator> mReplicableLocators = new Dictionary<int, ReplicableLocator>();
    public Dictionary<int, ReplicableLocator>.ValueCollection Locators => mReplicableLocators.Values;
    private bool mIsInitialized = false;

    private int mLocatorId = 0;
    //private int getNewLocatorID() => mLocatorId++;

    [Sirenix.OdinInspector.Button]
    private void setupReplicableLocators()
    {
        mInitialReplicableLocators = FindObjectsOfType<ReplicableLocator>().ToList();
        Debug.Log(LogManager.GetLogMessage($"Replicable locator list initialized! Count : {mInitialReplicableLocators.Count}", NetworkLogType.LocatorManager));
    }

    //public void Clear()
    //{
    //    // TODO : Remove all existing locators
    //}

    public void InitializeByManager(NetworkMode networkMode)
    {
        if (mIsInitialized)
            return;

        // Set locator prefabs
        foreach (var prefab in LocatorPrefab)
        {
            var locator = prefab.GetComponent<ReplicableLocator>();

            if (locator == null)
            {
                Debug.LogError(LogManager.GetLogMessage($"Locator prefab initialize failed! Cannot found \"ReplicableLocator\" " +
                    $"from binded prefabs! Object name : {prefab.name}", NetworkLogType.LocatorManager, true));
                continue;
            }

            mLocatorPrefabTable.Add(locator.LocatorType, prefab);
        }

        // Set exist locators
        NetworkMode = networkMode;

        for (int i = 0; i < mInitialReplicableLocators.Count; i++)
        {
            var initialLocator = mInitialReplicableLocators[i];
            initialLocator.SetNetworkMode(NetworkMode);
            initialLocator.SetLocatorID(i);
            mReplicableLocators.Add(i, initialLocator);
        }

        mLastInitialLocatorIndex = mInitialReplicableLocators.Count;
        mLocatorId = mInitialReplicableLocators.Count;

        mIsInitialized = true;
    }

    #region Server Sender

    public void SendLocatorStateUpdateData()
    {
        if (!DedicatedServerManager.TryGetInstance(out var server))
            return;

        var builder = server.GetBaseResponseBuilder(ResponseHandle.kUpdateLocatorStateData);

        bool hasData = false;

        foreach (var e in Locators)
        {
            if (e.TryGetChangedLocatorStateDataOrNull(out var data))
            {
                builder.AddLocatorStateData(data);
                hasData = true;
            }
        }

        if (hasData)
        {
            server.SendToAllClient_TCP(builder.Build());
        }
    }

    /// <summary>Locator의 초기 생성 데이터와 이미 소멸된 정보를 해당하는 클라이언트에게 전송합니다.</summary>
    public void SendInitialLocatorLifeDataTo(int sessionID)
    {
        if (!DedicatedServerManager.TryGetInstance(out var server))
            return;

        var builder = server.GetBaseResponseBuilder(ResponseHandle.kUpdateLocatorActionData);

        // Append already destroyed locator information
        foreach (var destroyLocatorID in mDestroyedLocatorIDs)
        {
            var destroyAction = LocatorActionData.CreateBuilder()
                .SetLocatorAction(ObjectActionType.kDestroyed)
                .SetLocatorId(destroyLocatorID);
            builder.AddLocatorActionData(destroyAction);
        }

        // Append exsit locators information
        foreach (var e in Locators)
        {
            var creationData = e.GetCreationAction();
            builder.AddLocatorActionData(creationData);
        }

        var packet = builder.Build();

        server.SendToClient_TCP(sessionID, packet);
    }

    //private void sendCreationActionToAll(ReplicableLocator createdLocator)
    //{
    //    if (!DedicatedServerManager.TryGetInstance(out var server))
    //        return;

    //    var builder = server.GetBaseResponseBuilder(ResponseHandle.kUpdateLocatorActionData);

    //    builder.AddLocatorActionData(createdLocator.GetCreationAction());

    //    server.SendToAllClient_TCP(builder.Build());
    //}

    //private void sendDestroyActionToAll(ReplicableLocator destroyLocator)
    //{
    //    if (!DedicatedServerManager.TryGetInstance(out var server))
    //        return;

    //    var builder = server.GetBaseResponseBuilder(ResponseHandle.kUpdateLocatorActionData);

    //    var destroyAction = LocatorActionData.CreateBuilder()
    //        .SetLocatorId(destroyLocator.LocatorID)
    //        .SetLocatorAction(ObjectActionType.kDestroyed);

    //    builder.AddLocatorActionData(destroyAction);

    //    server.SendToAllClient_TCP(builder.Build());
    //}

    #endregion

    #region Controll as Master

    //public ReplicableLocator CreateLocatorAsMaster(LocatorType locatorType, Vector3 position, Quaternion rotation)
    //{
    //    if (NetworkMode != NetworkMode.Master)
    //    {
    //        return null;
    //    }

    //    // Get locator prefabs from prefab table
    //    if (!mLocatorPrefabTable.ContainsKey(locatorType))
    //    {
    //        Debug.LogError(LogManager.GetLogMessage($"There is no such thing as \"{locatorType}\".", NetworkLogType.LocatorManager, true));
    //        return null;
    //    }

    //    // Poolling and create GameObject
    //    GameObject go = PoolManager.SpawnObject(mLocatorPrefabTable[locatorType]);
    //    var createdLocator = go.GetComponent<ReplicableLocator>();

    //    // Setup locator properties
    //    createdLocator.SetNetworkMode(NetworkMode);
    //    createdLocator.SetLocatorID(getNewLocatorID());
    //    createdLocator.transform.position = position;
    //    createdLocator.transform.rotation = rotation;
    //    createdLocator.IsActivated.Value = true;

    //    // Send creation action to all clients
    //    sendCreationActionToAll(createdLocator);

    //    // Add to management locator
    //    mReplicableLocators.Add(mLocatorId, createdLocator);

    //    return createdLocator;
    //}

    //public void DestroyLocatorAsMaster(int locatorId)
    //{
    //    if (NetworkMode != NetworkMode.Master)
    //    {
    //        return;
    //    }

    //    if (!mReplicableLocators.ContainsKey(locatorId))
    //        return;

    //    var destroyLocator = mReplicableLocators[locatorId];

    //    sendDestroyActionToAll(destroyLocator);

    //    mReplicableLocators.Remove(locatorId);
    //    PoolManager.ReleaseObject(destroyLocator.gameObject);

    //    // Cache initial locator destroy action
    //    if (locatorId <= mLastInitialLocatorIndex)
    //    {
    //        if (!mDestroyedLocatorIDs.Contains(locatorId))
    //        {
    //            mDestroyedLocatorIDs.Add(locatorId);
    //        }
    //    }
    //}

    #endregion

    #region Controll as Remote

    public void UpdateLocatorStateDataAsRemote(IList<LocatorStateData> stateDataList)
    {
        foreach (var data in stateDataList)
        {
            int locatorId = data.LocatorId;

            if (mReplicableLocators.TryGetValue(locatorId, out var locator))
            {
                locator.SetState(data);
            }
        }
    }

    public void UpdateLocatorActionAsRemote(IList<LocatorActionData> locatorActionList)
    {
        foreach (var action in locatorActionList)
        {
            var actionType = action.LocatorAction;
            int locatorID = action.LocatorId;

            switch (actionType)
            {
                case ObjectActionType.kCreated:
                    {
                        var creationData = action.LocatorCreationData;

                        // Create locator if it's not exist.
                        if (!mReplicableLocators.ContainsKey(locatorID))
                        {
                            CreateLocatorAsRemote(locatorID, creationData.Type, creationData.Position.ToVector3(), creationData.Rotation.ToQuaternion());
                        }
                        else
                        {
                            var locator = mReplicableLocators[locatorID];
                            locator.SetState(creationData.Position.ToVector3(), creationData.Rotation.ToQuaternion());
                        }
                    }
                    break;

                case ObjectActionType.kDestroyed:
                    {
                        // Destroy if locator exist.
                        if (mReplicableLocators.ContainsKey(locatorID))
                        {
                            var destroyLocator = mReplicableLocators[locatorID];

                            mReplicableLocators.Remove(locatorID);
                            PoolManager.ReleaseObject(destroyLocator.gameObject);
                        }
                    }
                    break;

                default:
                    Debug.LogError(LogManager.GetLogMessage($"Wrong ObjectActionType for locator! Type : {actionType}", NetworkLogType.LocatorManager, true));
                    break;
            }
        }
    }

    public ReplicableLocator CreateLocatorAsRemote(int locatorID, LocatorType locatorType, Vector3 position, Quaternion rotation)
    {
        if (NetworkMode != NetworkMode.Remote)
        {
            return null;
        }

        // Get locator prefabs from prefab table
        if (!mLocatorPrefabTable.ContainsKey(locatorType))
        {
            Debug.LogError(LogManager.GetLogMessage($"There is no such thing as \"{locatorType}\".", NetworkLogType.LocatorManager, true));
            return null;
        }

        // Poolling and create GameObject
        GameObject go = PoolManager.SpawnObject(mLocatorPrefabTable[locatorType]);
        var createdLocator = go.GetComponent<ReplicableLocator>();

        // Setup locator properties
        createdLocator.SetLocatorID(locatorID);
        createdLocator.transform.position = position;
        createdLocator.transform.rotation = rotation;
        createdLocator.IsActivated.Value = true;

        // Add to management locator
        mReplicableLocators.Add(mLocatorId, createdLocator);

        return createdLocator;
    }

    #endregion
}
