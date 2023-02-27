using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Utils;
using Network.Packet;
using static Network.Packet.Response.Types;

using Sirenix.OdinInspector;

namespace Network.Client
{

    //클라이언트에서 받는 복제된 데이터. 
    //해당 데이터를 참조하여 엔티티 갱신을 진행해야함.
    public class ReplicatedEntityData : BaseEntityData
    {
        [SerializeField, ReadOnly] private List<SkinnedMeshRenderer> mSkinnedMeshRendererList = new();
        [SerializeField, ReadOnly] private List<MeshRenderer> mMeshRendererList = new();
        [SerializeField] private List<GameObject> mParticleList = new();

#if UNITY_EDITOR

        [Sirenix.OdinInspector.Button]
        public void SetupMeshRenderer()
        {
            mSkinnedMeshRendererList = new List<SkinnedMeshRenderer>(GetComponentsInChildren<SkinnedMeshRenderer>());
            mMeshRendererList = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        }

#endif

        public Collider OwnerCollider;

        public event Action<EntityActionData> OnAction;
        public event Action OnSnap;
        public event Action<ReplicatedDetectedInfo> OnHitAction;

        public event Action<Vector3, Quaternion> OnInitialized;

        /// <summary>controlled entity</summary>
        public bool IsMine
        {
            get
            {
                //if (!ClientNetworkService.TryGetInstance(out var clientNetworkService))
                //    return true; // debugging only

                //return BindedClientID.Value == clientNetworkService.ConnectedID;

                return BindedClientID.Value == ClientSessionManager.Instance.SessionID;
            }
        }

        private ulong currentStatePacketID = 0;
        private ulong currentTransformPacketID = 0;
        private ulong currentActionPacketID = 0;

        protected virtual void Awake()
        {
            mInitialLayerIndex = gameObject.layer;
        }

        public void SetVisible(bool shouldVisible)
        {
            foreach (var r in mSkinnedMeshRendererList)
            {
                r.enabled = shouldVisible;
            }

            foreach (var r in mMeshRendererList)
            {
                r.enabled = shouldVisible;
            }

            foreach (var p in mParticleList)
            {
                p.SetActive(shouldVisible);
            }
        }

        public void Initialize(int entityID, Vector3 position, Quaternion rotation)
        {
            EntityID = entityID;
            IsAlive.Value = true;
            BindedClientID.Value = -1;
            currentTransformPacketID = 0;

            OnlyGroundPhysicsEnable = false;
            OnInitialized?.Invoke(position, rotation);
        }

        public void SetState(in ulong packetID, in EntityStateData entityStateData)
        {
            // TCP로만 송수신 하기 때문에 패킷 유효성 검사는 필요하지 않다.
            //if (currentStatePacketID >= packetID)
            //    return;

            //currentStatePacketID = packetID;

            // Base entity information data
            if (entityStateData.HasEntityId)
                EntityID = entityStateData.EntityId;

            if (entityStateData.HasEntityType)
                EntityType = entityStateData.EntityType;

            if (entityStateData.HasFactionType)
                FactionType = entityStateData.FactionType;

            // Server controlled data
            if (entityStateData.HasIsEnabled)
                IsEnabled.Value = entityStateData.IsEnabled;

            if (entityStateData.HasBindedClientId)
                BindedClientID.Value = entityStateData.BindedClientId;

            if (entityStateData.HasEquippedWeaponType)
                EquippedWeaponType.Value = entityStateData.EquippedWeaponType;

            if (entityStateData.HasEquippedWeaponEntityId)
                EquippedWeaponEntityID.Value = entityStateData.EquippedWeaponEntityId;

            // Gameplay state data
            if (entityStateData.HasHp)
                Hp.Value = entityStateData.Hp;

            if (entityStateData.HasIsAlive)
                IsAlive.Value = entityStateData.IsAlive;
        }

        public void SetTransform(in ulong packetID, in EntityTransformData entityTransformData)
        {
            if (currentTransformPacketID >= packetID)
                return;

            currentTransformPacketID = packetID;

            if (entityTransformData.HasIsTeleported)
            {
                OnSnap?.Invoke();
            }

            // Transform data
            if (entityTransformData.HasPosition)
                Position.Value = entityTransformData.Position.ToVector3();

            if (entityTransformData.HasRotation)
                Rotation.Value = entityTransformData.Rotation.ToQuaternion();

            if (entityTransformData.HasVelocity)
                Velocity.Value = entityTransformData.Velocity.ToVector3();
        }

        public void AddAction(in ulong packetID, in EntityActionData entityActionData)
        {
            //Debug.Log($"[World] Entity : {entityActionData.EntityId} / {entityActionData.Action}");

            bool isPastPacket = (currentActionPacketID > packetID);
            currentActionPacketID = packetID;

            if (isPastPacket)
                return;

            // Destroy Action인 경우 곧바로 엔티티 제거
            // Die Action인 경우 엔티티의 체력은 0으로 확정. 서버에서는 실제로 사라짐.
            // TODO : Die Action을 전송받으면 죽음, 혹은 파괴 처리
            //ActionDataBuffer.Add(entityActionData);
            OnAction?.Invoke(entityActionData);

            switch (entityActionData.Action)
            {
                case EntityAction.kDestroy: // 즉시 사라짐
                    Hp.Value = 0;           // TODO : 즉시 사라져야한다.
                    break;

                case EntityAction.kDie: // 죽음과 관련된 이벤트 진행 후 사라짐
                    Hp.Value = 0;
                    SetState(packetID, entityActionData.LastEntityStateData);

                    OnlyGroundPhysicsEnable = true;
                    break;

                //case EntityAction.kRevive:
                //    OnlyGroundPhysicsEnable = false;
                //    TryConnectInput();
                //    break;

                case EntityAction.kUseWeapon:
                    break;

                case EntityAction.kEquipWeapon:
                    break;

                case EntityAction.kUnequipWeapon:
                    break;

                default:
                    break;
            }
        }

        public void RaiseHitInfo(ReplicatedDetectedInfo info)
        {
            OnHitAction?.Invoke(info);
        }

        public void EntityDestory()
        {
            OnlyGroundPhysicsEnable = true;

            IsAlive.Value = false;
        }
    }
}