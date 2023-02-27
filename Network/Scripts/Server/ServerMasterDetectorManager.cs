using Network.Packet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using static Network.Packet.Response.Types;

namespace Network.Server
{
    public class ServerMasterDetectorManager : LocalSingleton<ServerMasterDetectorManager>
    {
#if UNITY_EDITOR

        [Sirenix.OdinInspector.Button]
        public void SetDetectors()
        {
            var detectorPrefabs = GlobalPath.GetPrefabsFileFromPath(GlobalPath.DetectorPrefabsPath, GlobalPath.MasterSuffix);
            DetectorPrefabs = new List<GameObject>(detectorPrefabs);
        }

#endif

        [SerializeField] private List<GameObject> DetectorPrefabs;
        private readonly Dictionary<DetectorType, GameObject> mDetectorPrefabTable = new();

        private List<DetectorActionData> mTcpDetectorActionDataBuffer = new();
        private List<BaseDetectorData> mDetectorInstances = new List<BaseDetectorData>();

        private int mDetectorID = 0;

        private bool mIsInitialized = false;
        public void OnServerStarted()
        {
            // Initialize detector prefabs by DetectorType
            foreach (var go in DetectorPrefabs)
            {
                BaseDetectorData detector = go.GetComponent<BaseDetectorData>();

                if (detector == null)
                {
                    Debug.LogError("[World] Detector prefab initialize failed! Cannot found \"BaseDetectorData\" from binded prefabs!");
                    continue;
                }

                mDetectorPrefabTable.Add(detector.DetectorType, go);
            }

            mIsInitialized = true;
        }

        public void Clear()
        {
            for (int i = mDetectorInstances.Count - 1; i >= 0; i--)
            {
                DestroyDetector(mDetectorInstances[i]);
            }
        }

        public BaseDetectorData CreateNewDetector(DetectorType detectorType, DetectorInfo info)
        {
            if (!mDetectorPrefabTable.ContainsKey(detectorType))
            {
                Debug.LogError($"[World] There is no such thing as \"{detectorType}\".");
            }

            GameObject go = PoolManager.SpawnObject(mDetectorPrefabTable[detectorType]);

            var createdDetectorData = go.GetComponent<BaseDetectorData>();
            mDetectorInstances.Add(createdDetectorData);
            createdDetectorData.Initialize(mDetectorID, info, DestroyDetector);

            mDetectorID++;

            ServerWorldManager.Instance.MoveGameObjectToScene(go);

            var detectorCreationData = DetectorCreationData.CreateBuilder()
                .SetOrigin(info.Origin.ToData())
                .SetDirection(info.RawViewVector.ToData())
                .SetType(createdDetectorData.DetectorType)
                .SetDamage(info.DamageInfo.damage)
                .SetOwnerEntityId(info.OwnerEntityID);

            var builder = DetectorActionData.CreateBuilder()
                .SetDetectorId(createdDetectorData.DetectorID)
                .SetDetectorAction(ObjectActionType.kCreated)
                .SetDetectorCreationData(detectorCreationData);

            mTcpDetectorActionDataBuffer.Add(builder.Build());

            return createdDetectorData;
        }

        public void DestroyDetector(BaseDetectorData detectorInstance)
        {
            mDetectorInstances.Remove(detectorInstance);

            PoolManager.ReleaseObject(detectorInstance.gameObject);

            var builder = DetectorActionData.CreateBuilder()
                .SetDetectorId(detectorInstance.DetectorID)
                .SetDetectorAction(ObjectActionType.kDestroyed);

            mTcpDetectorActionDataBuffer.Add(builder.Build());
        }

        #region Send to client

        public void SendDetectorActionDataViaTcpToAll()
        {
            if (mTcpDetectorActionDataBuffer.IsEmpty())
                return;

            var builder = DedicatedServerManager.Instance.GetBaseResponseBuilder(ResponseHandle.kUpdateDetectorActionData)
                .AddRangeDetectorActionData(mTcpDetectorActionDataBuffer);

            DedicatedServerManager.Instance.SendToAllClient_TCP(builder.Build());

            mTcpDetectorActionDataBuffer.Clear();
        }

        public void SendDetectorStringAction(BaseDetectorData detector, string stringPacket)
        {
            var builder = DetectorActionData.CreateBuilder()
                .SetDetectorId(detector.DetectorID)
                .SetDetectorAction(ObjectActionType.kEventOccur)
                .SetStringPacket(stringPacket)
                .Build();

            mTcpDetectorActionDataBuffer.Add(builder);
        }

        #endregion
    }
}
