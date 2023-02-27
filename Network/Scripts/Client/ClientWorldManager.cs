using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using static Network.Packet.Response.Types;
using Network.Packet;
using Utils;
using Network.Common;
using CKC2022;

namespace Network.Client
{
    public class ClientWorldManager : LocalSingleton<ClientWorldManager>
    {
#if UNITY_EDITOR

        [Sirenix.OdinInspector.Button]
        public void testDeactivateAll()
        {
            SetAllEntitiesActivation(false);
        }

        [Sirenix.OdinInspector.Button]
        public void testActivateAll()
        {
            SetAllEntitiesActivation(true);
        }

        [Sirenix.OdinInspector.Button]
        public void SetupRemotePrefabs()
        {
            var humanoidPrefabs = GlobalPath.GetPrefabsFileFromPath(GlobalPath.HumanoidPrefabsPath, GlobalPath.RemoteSuffix);
            var EnemyPrefabs = GlobalPath.GetPrefabsFileFromPath(GlobalPath.EnemyPrefabsPath, GlobalPath.RemoteSuffix);
            //var weaponPrefabs = GlobalPath.GetPrefabsFileFromPath(GlobalPath.WeaponPrefabsPath, GlobalPath.RemoteSuffix);
            var structurePrefabs = GlobalPath.GetPrefabsFileFromPath(GlobalPath.StructurePrefabsPath, GlobalPath.RemoteSuffix);

            ReplicatedEntityPrefabs = new List<GameObject>();

            ReplicatedEntityPrefabs.AddRange(humanoidPrefabs);
            ReplicatedEntityPrefabs.AddRange(EnemyPrefabs);
            //ReplicatedEntityPrefabs.AddRange(weaponPrefabs);
            ReplicatedEntityPrefabs.AddRange(structurePrefabs);

            var detectorPrefabs = GlobalPath.GetPrefabsFileFromPath(GlobalPath.DetectorPrefabsPath, GlobalPath.RemoteSuffix);

            ReplicatedDetectorPrefabs = new List<GameObject>(detectorPrefabs);
        }

#endif

        [SerializeField] private List<GameObject> ReplicatedEntityPrefabs;
        private Dictionary<EntityType, GameObject> mEntityPrefabTable = new Dictionary<EntityType, GameObject>();

        [SerializeField] private List<GameObject> ReplicatedDetectorPrefabs;
        private Dictionary<DetectorType, GameObject> mDetectorPrefabTable = new Dictionary<DetectorType, GameObject>();

        private Scene? mCurrentLoadedGameMapScene;
        private string mCurrentSceneName;

        private Dictionary<int, ReplicatedEntityData> mRemotedEntityByID = new Dictionary<int, ReplicatedEntityData>();
        private readonly Dictionary<EntityBaseType, List<ReplicatedEntityData>> ReplicatedEntitiesByType = new();

        private Dictionary<int, BaseDetectorData> mRemotedDetectorByID = new Dictionary<int, BaseDetectorData>();

        //private List<int> mEntityDestroyIdList = new List<int>();

        private ClientSessionManager mSessionManager => ClientSessionManager.Instance;
        private UserSessionData_Remote mUserSessionData => ClientSessionManager.Instance.UserSessionData;
        private GameGlobalState_Remote mGameGlobalState => ClientSessionManager.Instance.GameGlobalState;

        public event Action OnEntityUpdated;

        public EntityBaseType GetType<T>() where T : ReplicatedEntityData
        {
            return Activator.CreateInstance(typeof(T)) switch
            {
                //ReplicatedWeaponEntityData _ => EntityBaseType.Weapon,
                ReplicatedEntityData _ => EntityBaseType.Humanoid,
                _ => EntityBaseType.None
            };
        }

        public bool TryGetEntity(in int entityID, out ReplicatedEntityData entity)
            => mRemotedEntityByID.TryGetValue(entityID, out entity);

        public bool TryGetEntity<T>(int entityID, out T entity) where T : ReplicatedEntityData
            => TryGetEntity((data) => data.EntityID == entityID, out entity);

        public bool TryGetEntity<T>(in Predicate<ReplicatedEntityData> pred, out T entity) where T : ReplicatedEntityData
            => TryGetEntity(GetType<T>(), pred, out entity);

        public bool TryGetEntity<T>(in EntityBaseType type, in Predicate<ReplicatedEntityData> pred, out T entity) where T : ReplicatedEntityData
        {
            var isSuccess = TryGetEntities<T>(type, pred, out var list);
            entity = list?.FirstOrDefault();

            return isSuccess;
        }

        public bool TryGetEntities<T>(in EntityBaseType type, in Predicate<ReplicatedEntityData> pred, out List<T> entities) where T : ReplicatedEntityData
        {
            entities = null;

            if (!ReplicatedEntitiesByType.TryGetValue(type, out var targets))
                return false;

            entities = targets.FindAll(pred).Select(entityData => entityData as T).ToList();

            if (entities == null || entities.Count < 0)
                return false;

            return true;
        }

        public int EntityCount => mRemotedEntityByID.Count;

        private void Start()
        {
            // Initialize entity prefabs by EntityType
            foreach (var go in ReplicatedEntityPrefabs)
            {
                ReplicatedEntityData entity = go.GetComponent<ReplicatedEntityData>();

                if (entity == null)
                {
                    Debug.LogError("[World] Entity prefab initialize failed! Cannot find 'ReplicatedEntityData' from binded prefabs!");
                    continue;
                }

                mEntityPrefabTable.Add(entity.EntityType, go);
            }

            // Initialize detector prefabs by DetectorType
            foreach (var go in ReplicatedDetectorPrefabs)
            {
                BaseDetectorData detector = go.GetComponent<BaseDetectorData>();

                if (detector == null)
                {
                    Debug.LogError("[World] Detector prefab initialize failed! Cannot find 'ReplicatedDetectorData' from binded prefabs!");
                    continue;
                }

                mDetectorPrefabTable.Add(detector.DetectorType, go);
            }
        }

        public void TryLoadScene(string mapName, Action callback)
        {
            mCurrentSceneName = mapName;

            if (mCurrentLoadedGameMapScene != null)
            {
                this.Clear();

                if (ItemObjectManager.TryGetInstance(out var itemObjectManager))
                {
                    itemObjectManager.ClearAsRemote();
                }

                Debug.Log(LogManager.GetLogMessage($"Unload map scene \"{mCurrentLoadedGameMapScene.Value.name}\""));

                SceneManager.UnloadSceneAsync(mCurrentLoadedGameMapScene.Value).completed += (operation)=>
                {
                    loadScene();
                };
            }
            else
            {
                loadScene();
            }

            void loadScene()
            {
                SceneManager.LoadSceneAsync(mapName, new LoadSceneParameters(LoadSceneMode.Additive)).completed += (operation)=>
                {
                    mCurrentLoadedGameMapScene = SceneManager.GetSceneByName(mapName);

                    SceneManager.SetActiveScene(mCurrentLoadedGameMapScene.Value);

                    // Initialize locator event manager when the scene completely loaded
                    if (LocatorEventManager.TryGetInstance(out var locatorEventManager))
                    {
                        locatorEventManager.InitializeByManager(NetworkMode.Remote);
                    }
                    else
                    {
                        Debug.LogError(LogManager.GetLogMessage($"There is no \"LocatorEventManager\" on scene {mapName}", NetworkLogType.LocatorManager, true));
                    }

                    // Initialize item object manager when the scene completely loaded
                    if (ItemObjectManager.TryGetInstance(out var itemObjectManager))
                    {
                        itemObjectManager.InitializeByManager(NetworkMode.Remote);
                    }
                    else
                    {
                        Debug.LogError(LogManager.GetLogMessage($"There is no \"ItemObjectManager\" on scene {mapName}", NetworkLogType.ItemObjectManager, true));
                    }

                    // Initialize check point manager when the scene completely loaded
                    if (CheckPointManager.TryGetInstance(out var checkPointManager))
                    {
                        checkPointManager.InitializeByManager(NetworkMode.Remote);
                    }
                    else
                    {
                        Debug.LogError(LogManager.GetLogMessage($"There is no \"checkPointManager\" on scene {mapName}", NetworkLogType.CheckPointManager, true));
                    }

                    if (BridgeWaveEventManager.TryGetInstance(out var bridgeWaveManager))
                    {
                        bridgeWaveManager.InitializeByManager(NetworkMode.Remote);
                    }
                    else
                    {
                        Debug.LogError(LogManager.GetLogMessage($"There is no \"BridgeWaveEventManager\" on scene {mCurrentSceneName}", NetworkLogType.BridgeWaveEventManager, true));
                    }

                    callback?.Invoke();

                    Debug.Log(LogManager.GetLogMessage($"Load map scene \"{mCurrentSceneName}\""));
                };
            }
        }

        public void Clear()
        {
            Debug.Log(LogManager.GetLogMessage($"Clear world"));

            var detectorCollection = mRemotedDetectorByID.Values.ToArray();
            for (int i = 0; i < detectorCollection.Length; i++)
            {
                updateDetectorDestroy(detectorCollection[i]);
            }
            mRemotedDetectorByID.Clear();

            // Destroy Detector
            foreach (var detector in mRemotedDetectorByID.Values)
            {
                updateDetectorDestroy(detector);
            }
            mRemotedDetectorByID.Clear();

            var idCollection = mRemotedEntityByID.Keys.ToArray();
            // Destroy Entities
            for (int i = 0; i < idCollection.Length; ++i)
            {
                DestroyEntityByID(idCollection[i]);
            }
            mRemotedEntityByID.Clear();

            // Clear Action Buffers

            Debug.Log(LogManager.GetLogMessage($"Remain entities count : {mRemotedEntityByID.Count} / Remain detector count : {mRemotedDetectorByID.Count}"));
        }

        /// <summary>클라이언트가 조종중인 엔티티를 반환받습니다.</summary>
        public bool TryGetMyEntity(out ReplicatedEntityData entity)
        {
            if (ClientSessionManager.Instance.TryGetMyPlayerEntityID(out int myPlayerEntityID))
            {
                if (mRemotedEntityByID.TryGetValue(myPlayerEntityID, out entity))
                {
                    return true;
                }
            }

            entity = null;
            return false;
            //if (mSessionManager.TryGetMyEntityType(out var myEntityType))
            //{
            //    foreach (var e in mRemotedEntityByID.Values)
            //    {
            //        if (e.EntityType == myEntityType)
            //        {
            //            entity = e;
            //            return true;
            //        }
            //    }
            //}

            //entity = null;
            //return false;
        }

        #region Getter

        public bool TryGetPlayerEntities(out List<ReplicatedEntityData> playerEntities)
        {
            playerEntities = new List<ReplicatedEntityData>();

            if (ClientSessionManager.Instance.TryGetCurrentPlayerEntityIDs(out var playerEntityIDs))
            {
                foreach (var entityID in playerEntityIDs)
                {
                    if (mRemotedEntityByID.TryGetValue(entityID, out var entity))
                    {
                        playerEntities.Add(entity);
                    }
                }

                return !playerEntities.IsEmpty();
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Operation

        public void SetAllEntitiesActivation(bool isVisible)
        {
            foreach (var e in mRemotedEntityByID.Values)
            {
                e.SetVisible(isVisible);
            }
        }

        #endregion

        #region Response From Server

        public void UpdateEntitySpawnData(in ulong packetID, in IList<EntitySpawnData> spawnDatas)
        {
            // Instantiate entities
            foreach (var e in spawnDatas)
            {
                int entityID = e.EntityId;

                //// If destroy packet was already arrived. Do not make new entity.
                //if (mEntityDestroyIdList.Contains(entityID))
                //{
                //    mEntityDestroyIdList.Remove(entityID);
                //    continue;
                //}

                // If entity already exist.
                if (mRemotedEntityByID.ContainsKey(entityID))
                {
                    Debug.Log($"[World] [ERROR] Entity spawn error! Entity already exist! [EntityID : {entityID}]");
                    continue;
                }

                EntityType entityType = e.EntityType;

                // Get entity prefab.
                if (!mEntityPrefabTable.ContainsKey(entityType))
                {
                    Debug.Log($"[World] [ERROR] There is no such thing as entity {entityType}");
                    continue;
                }

                GameObject go = mEntityPrefabTable[entityType];

                // Create entity by spawn data
                Vector3 currentPosition = e.SpawnPosition.ToVector3();
                Quaternion currentRotation = e.SpawnRotation.ToQuaternion();

                var instance = PoolManager.SpawnObject(go, currentPosition, currentRotation);
                var remotedEntity = instance.GetComponent<ReplicatedEntityData>();

                remotedEntity.Initialize(entityID, currentPosition, currentRotation);

                // This is the same behavior as SetTransform
                remotedEntity.Position.Value = currentPosition;
                remotedEntity.Rotation.Value = currentRotation;

                // Set to physics scene and add management table.
                SceneManager.MoveGameObjectToScene(instance.SetAsRoot(), mCurrentLoadedGameMapScene.Value);
                mRemotedEntityByID.Add(entityID, remotedEntity);

                if (!ReplicatedEntitiesByType.TryGetValue(entityType.GetEntityBaseType(), out var subList))
                {
                    subList = new List<ReplicatedEntityData>();
                    ReplicatedEntitiesByType[entityType.GetEntityBaseType()] = subList;
                }

                subList.Add(remotedEntity);
            }

            // Setup initial entities data
            foreach (var e in spawnDatas)
            {
                int entityID = e.EntityId;

                if (!mRemotedEntityByID.TryGetValue(entityID, out var createdEntity))
                    return;

                createdEntity.SetState(packetID, e.EntityStateData);
            }

            OnEntityUpdated?.Invoke();
        }

        public void DestroyEntityByID(int entityID)
        {
            var entityType = mRemotedEntityByID[entityID].EntityType.GetEntityBaseType();
            if (ReplicatedEntitiesByType.TryGetValue(entityType, out var subList))
                subList.Remove(mRemotedEntityByID[entityID]);

            mRemotedEntityByID[entityID].EntityDestory();
            mRemotedEntityByID.Remove(entityID);

            Debug.Log($"[World] Entity destroy [EntityID : {entityID}]");
            OnEntityUpdated?.Invoke();
        }

        public void UpdateDetectorActionData(IList<DetectorActionData> detectorActionData)
        {
            foreach (var d in detectorActionData)
            {
                int detectorID = d.DetectorId;
                var detectorAction = d.DetectorAction;

                if (detectorAction == ObjectActionType.kCreated)
                {
                    updateDetectorCreate(detectorID, d.DetectorCreationData);
                }
                else if (detectorAction == ObjectActionType.kDestroyed)
                {
                    //return; // 클라이언트가 소멸을 처리하도록 로직 변경

                    //// 생성 소멸 패킷 순서가 잘못되었다면 소멸 패킷은 실행하지 않는다.
                    //if (!mRemotedDetectorByID.ContainsKey(detectorID))
                    //{
                    //    return;
                    //}

                    //var detectorObject = mRemotedDetectorByID[detectorID];

                    //updateDetectorDestroy(detectorObject);
                }
                else if (detectorAction == ObjectActionType.kEventOccur)
                {
                    if (mRemotedDetectorByID.TryGetValue(detectorID, out var detector))
                    {
                        detector.ReceiveAsRemote(d.StringPacket);
                    }
                }
                else
                {
                    Debug.Log($"[World] [Error] There is no such thing as \"{detectorAction}\"");
                }
            }
        }

        private void updateDetectorDestroy(BaseDetectorData detectorData)
        {
            if (!mRemotedDetectorByID.ContainsKey(detectorData.DetectorID))
                return;

            SpawnEffect();

            PoolManager.ReleaseObject(detectorData.gameObject);
            mRemotedDetectorByID.Remove(detectorData.DetectorID);

            void SpawnEffect()
            {
                if (!ItemManager.TryGetConfig(detectorData.DetectorType, out var config))
                    return;

                if (config.DESTORY_EFFECT == null)
                    return;
                
                var instance = PoolManager.SpawnObject(config.DESTORY_EFFECT,
                    detectorData.transform.position,
                    Quaternion.LookRotation(detectorData.transform.forward));
            }
        }

        private bool updateDetectorCreate(int detectorID, DetectorCreationData creationData)
        {
            // Check if this object already exists
            if (mRemotedDetectorByID.ContainsKey(detectorID))
                return false;

            var detectorType = creationData.Type;
            
            // Check detector type
            if (!mDetectorPrefabTable.ContainsKey(detectorType))
                return false;

            var prefab = mDetectorPrefabTable[detectorType];
            var go = PoolManager.SpawnObject(prefab);

            var currentDetectorData = go.GetComponent<BaseDetectorData>();

            int ownerEntityID = creationData.OwnerEntityId;

            // Set owner collider
            var info = new ReplicatedDetectorInfo();

            info.Origin = creationData.Origin.ToVector3();
            info.Direction = creationData.Direction.ToVector3().normalized;
            info.RawViewVector = creationData.Direction.ToVector3();
            info.OwnerEntityID = ownerEntityID;
            info.detectorType = creationData.Type;

            if (mRemotedEntityByID.TryGetValue(ownerEntityID, out var owner))
            {
                info.OwnerCollider = owner.OwnerCollider;
                info.DamageInfo = new DamageInfo(creationData.Damage, owner.FactionType);
            }
            else
            {
                info.DamageInfo = new DamageInfo(creationData.Damage, FactionType.kEnemyFaction_1);
            }

            currentDetectorData.Initialize(detectorID, info, updateDetectorDestroy);

            SceneManager.MoveGameObjectToScene(go.SetAsRoot(), mCurrentLoadedGameMapScene.Value);

            mRemotedDetectorByID.Add(detectorID, currentDetectorData);

            //sound

            if (ItemManager.TryGetConfig(creationData.Type, out var config))
            {
                GameSoundManager.Play(config.SPAWN_SOUND_CODE, new SoundPlayData(creationData.Origin.ToVector3()));
            }

            return true;
        }


        private readonly Dictionary<int, int> mBindLookup = new();

        public void UpdateEntityStatesData(in ulong packetID, IList<EntityStateData> stateData)
        {
            foreach (var state in stateData)
            {
                int entityID = state.EntityId;

                if (!mRemotedEntityByID.ContainsKey(entityID))
                {
                    //Debug.Log($"[World] Update state error ! [EntityID : {entityID}]");
                    continue;
                }

                if(CheckBindedClientIDChanged(entityID, state.BindedClientId))
                {
                    OnEntityUpdated?.Invoke();
                }

                mRemotedEntityByID[entityID].SetState(packetID, state);
            }

            bool CheckBindedClientIDChanged(in int entityID, in int currentID)
            {
                if (mRemotedEntityByID[entityID].EntityType.GetEntityBaseType() != EntityBaseType.Humanoid)
                    return false;

                if (!mBindLookup.TryGetValue(entityID,out var prev))
                {
                    prev = -1;
                }

                var isEqual = prev == currentID;

                mBindLookup[entityID] = currentID;

                return isEqual;
            }
        }

        public void UpdateEntityTransformData(in ulong packetID, in IList<EntityTransformData> transformData)
        {
            foreach (var t in transformData)
            {
                int entityID = t.EntityId;

                if (!mRemotedEntityByID.ContainsKey(entityID))
                {
                    //Debug.Log($"[World] Update state error ! [EntityID : {entityID}]");
                    continue;
                }

                mRemotedEntityByID[entityID].SetTransform(packetID, t);
            }
        }

        public void UpdateEntityActionData(in ulong packetID, in IList<EntityActionData> actionDatas)
        {
            foreach (var action in actionDatas)
            {
                int entityID = action.EntityId;

                if (!mRemotedEntityByID.ContainsKey(entityID))
                {
                    //// When entity die packet arrived before creation packet.
                    //if (action.Action == EntityAction.kDestroy || action.Action == EntityAction.kDie)
                    //{
                    //    mEntityDestroyIdList.Add(entityID);
                    //    Debug.Log($"[World] Entity destroy action received before created! [EntityID : {entityID}]");
                    //    continue;
                    //}

                    Debug.Log($"[World] Update action error ! [EntityID : {entityID}]");

                    continue;
                }

                mRemotedEntityByID[entityID].AddAction(packetID, action);
            }
        }

        #endregion
    }
}
