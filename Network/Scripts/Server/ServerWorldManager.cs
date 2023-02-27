using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Network.Packet;
using static Network.Packet.Request.Types;
using static Network.Packet.Response.Types;
using Utils;
using UnityEngine.Events;
using System.Collections;

namespace Network.Server
{
    public class ServerWorldManager : MonoSingleton<ServerWorldManager>
    {
        //public event Action OnWorldLoaded;

        //public string LoadPhysicsSceneName = "PhysicsMapScene";
        private float PhysicsSceneTimeScale = 1;

        private string mCurrentSceneName;

        private LoadSceneParameters mMapSceneLoadOption = new LoadSceneParameters(LoadSceneMode.Additive);//, LocalPhysicsMode.Physics3D);
        private Scene? mCurrentLoadedGameMapScene = null;
        private PhysicsScene mLoadedGameMapPhysicsScene;

        private int mEntityID = 1;
        public int NewEntityID => mEntityID++;

        private CoroutineWrapper NetworkRoutine;
        private CoroutineWrapper PhysicsRoutine;

        protected override void Initialize()
        {
            base.Initialize();

            if (NetworkRoutine == null)
            {
                NetworkRoutine = new CoroutineWrapper(CoroutineWrapper.CoroutineRunner.Instance);
            }

            if (PhysicsRoutine == null)
            {
                PhysicsRoutine = new CoroutineWrapper(CoroutineWrapper.CoroutineRunner.Instance);
            }
        }

        private void Start()
        {
            // Setup physics option
            Physics.autoSimulation = false;
        }

        public void OnServerStarted()
        {
            // Initialize every manager
            ServerMasterNetObjectManager.Instance.OnServerStarted();
            ServerMasterEntityManager.Instance.OnServerStarted();
            ServerMasterDetectorManager.Instance.OnServerStarted();

            NetworkRoutine.StartSingleton(updateTick());
            PhysicsRoutine.StartSingleton(updateWorld());
        }

        private IEnumerator updateWorld()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                if ((ServerSessionManager.Instance.CurrentServerState == ServerState.GamePlayScene) ||
                    (mLoadedGameMapPhysicsScene != null))
                {
                    // Simulate world
                    mLoadedGameMapPhysicsScene.Simulate(Time.fixedDeltaTime * PhysicsSceneTimeScale);
                }
            }
        }

        private IEnumerator updateTick()
        {
            while (true)
            {
                yield return new WaitForSeconds(ServerConfiguration.SERVER_NETWORK_DELTA_TIME);
                if ((ServerSessionManager.Instance.CurrentServerState == ServerState.GamePlayScene) ||
                    (mLoadedGameMapPhysicsScene != null))
                {
                    ServerMasterEntityManager.Instance?.SendEntityStatesDataToAll();
                    ServerMasterEntityManager.Instance?.SendEntityTransformDataToAll();

                    ServerMasterEntityManager.Instance?.SendEntityActionDataViaUdpToAll();
                    ServerMasterEntityManager.Instance?.SendEntityActionDataViaTcpToAll();

                    ServerMasterDetectorManager.Instance?.SendDetectorActionDataViaTcpToAll();

                    if (LocatorEventManager.TryGetInstance(out var locatorEventManager))
                    {
                        locatorEventManager.SendLocatorStateUpdateData();
                    }

                    if (ItemObjectManager.TryGetInstance(out var itemObjectManager))
                    {
                        itemObjectManager.SendItemObjectStateUpdateData();
                    }
                }
            }
        }

        #region Controll Scene

        public void TryLoadScene(string mapName, Action callback)
        {
            mCurrentSceneName = mapName;

            if (mCurrentLoadedGameMapScene.HasValue)
            {
                ServerPlayerCharacterManager.Instance.Clear();
                ServerMasterEntityManager.Instance.Clear();
                ServerMasterDetectorManager.Instance.Clear();

                if (ItemObjectManager.TryGetInstance(out var itemObjectManager))
                {
                    itemObjectManager.ClearAsMaster();
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
                SceneManager.LoadSceneAsync(mCurrentSceneName, mMapSceneLoadOption).completed += (operation)=>
                {
                    mCurrentLoadedGameMapScene = SceneManager.GetSceneByName(mCurrentSceneName);
                    mLoadedGameMapPhysicsScene = mCurrentLoadedGameMapScene.Value.GetPhysicsScene();

                    SceneManager.SetActiveScene(mCurrentLoadedGameMapScene.Value);

                    // Initialize locator event manager when the scene completely loaded
                    if (LocatorEventManager.TryGetInstance(out var locatorEventManager))
                    {
                        locatorEventManager.InitializeByManager(NetworkMode.Master);
                    }
                    else
                    {
                        Debug.LogError(LogManager.GetLogMessage($"There is no \"LocatorEventManager\" on scene {mCurrentSceneName}", NetworkLogType.LocatorManager, true));
                    }

                    // Initialize item object manager when the scene completely loaded
                    if (ItemObjectManager.TryGetInstance(out var itemObjectManager))
                    {
                        itemObjectManager.InitializeByManager(NetworkMode.Master);
                    }
                    else
                    {
                        Debug.LogError(LogManager.GetLogMessage($"There is no \"ItemObjectManager\" on scene {mCurrentSceneName}", NetworkLogType.ItemObjectManager, true));
                    }

                    // Initialize check point manager when the scene completely loaded
                    if (CheckPointManager.TryGetInstance(out var checkPointManager))
                    {
                        checkPointManager.InitializeByManager(NetworkMode.Master);
                    }
                    else
                    {
                        Debug.LogError(LogManager.GetLogMessage($"There is no \"checkPointManager\" on scene {mapName}", NetworkLogType.CheckPointManager, true));
                    }

                    // Initialize server player character manager when the scene completely loaded
                    if (ServerPlayerCharacterManager.TryGetInstance(out var serverPlayerCharacterManager))
                    {
                        serverPlayerCharacterManager.InitializeByManager();
                    }
                    else
                    {
                        Debug.LogError(LogManager.GetLogMessage($"There is no \"ServerPlayerCharacterManager\" on scene {mCurrentSceneName}", NetworkLogType.MasterServer, true));
                    }

                    // Initialize server player character manager when the scene completely loaded
                    if (BlockingWaveEventManager.TryGetInstance(out var waveEventManager))
                    {
                        waveEventManager.InitializeByManager(NetworkMode.Master);
                    }
                    else
                    {
                        Debug.LogError(LogManager.GetLogMessage($"There is no \"BlockingWaveEventManager\" on scene {mCurrentSceneName}", NetworkLogType.BlockingWaveEventManager, true));
                    }

                    if (BridgeWaveEventManager.TryGetInstance(out var bridgeWaveManager))
                    {
                        bridgeWaveManager.InitializeByManager(NetworkMode.Master);
                    }
                    else
                    {
                        Debug.LogError(LogManager.GetLogMessage($"There is no \"BridgeWaveEventManager\" on scene {mCurrentSceneName}", NetworkLogType.BridgeWaveEventManager, true));
                    }

                    callback?.Invoke();

                    Debug.Log(LogManager.GetLogMessage($"Load map scene \"{mCurrentSceneName}\""));
                };
            }

            //void loadScene()
            //{
            //    var sceneLoadOperation = SceneManager.LoadSceneAsync(mCurrentSceneName, mMapSceneLoadOption);

            //    StartCoroutine(physicsloadScene());

            //    IEnumerator physicsloadScene()
            //    {
            //        yield return new WaitUntil(() => sceneLoadOperation.isDone);

            //        mCurrentLoadedGameMapScene = SceneManager.GetSceneByName(mCurrentSceneName);
            //        mLoadedGameMapPhysicsScene = mCurrentLoadedGameMapScene.Value.GetPhysicsScene();

            //        SceneManager.SetActiveScene(mCurrentLoadedGameMapScene.Value);

            //        ServerMasterEntityManager.Instance.Clear();
            //        ServerMasterDetectorManager.Instance.Clear();
            //        ServerPlayerCharacterManager.Instance.Clear();

            //        // Initialize locator event manager when the scene completely loaded
            //        if (LocatorEventManager.TryGetInstance(out var locatorEventManager))
            //        {
            //            locatorEventManager.InitializeByManager(NetworkMode.Master);
            //        }
            //        else
            //        {
            //            Debug.LogError(LogManager.GetLogMessage($"There is no \"LocatorEventManager\" on scene {mCurrentSceneName}", NetworkLogType.WorldManager, true));
            //        }

            //        callback?.Invoke();

            //        Debug.Log("[World] Physics scene loaded");
            //    }
            //}
        }

        #endregion

        public void MoveGameObjectToScene(GameObject go)
        {
            SceneManager.MoveGameObjectToScene(go.SetAsRoot(), mCurrentLoadedGameMapScene.Value);
        }

        #region Handle Common Actions

        [Obsolete("무기 줍기는 더 이상 쓰이지 않음")]
        public void HandleEquipWeaponPacket(int fromClient, RequestEquipWeaponData equipWeaponData)
        {
            int weaponEntityID = equipWeaponData.WeaponEntityId;
            int playerEntityID = ServerPlayerCharacterManager.Instance.GetPlayerEntityIdByClientID(fromClient);

            if (playerEntityID < 0)
			{
                return;
			}

            HandleEquipWeapon(playerEntityID, weaponEntityID);
        }

        [Obsolete("무기 줍기는 더 이상 쓰이지 않음")]
        public void HandleEquipWeapon(int humanoidEntityID, int weaponEntityID)
        {
            var humanoidEntity = ServerMasterEntityManager.Instance.GetEntityOrNull(humanoidEntityID) as MasterHumanoidEntityData;
            if (humanoidEntity == null)
                return;

            // Unequip weapon
            if (weaponEntityID < 0)
            {
                humanoidEntity.ActionUnequipWeapon();
                return;
            }

            var weaponEntity = ServerMasterEntityManager.Instance.GetEntityOrNull(weaponEntityID) as MasterWeaponEntityData;
            if (weaponEntity == null)
                return;

            // Equip weapon
            if (weaponEntity.IsEnabled.Value)
            {
                humanoidEntity.ActionEquipWeapon(weaponEntity);
            }
            else
            {
                return; // Do not equip when weapon is disabled
            }
        }

        #endregion
    }
}

