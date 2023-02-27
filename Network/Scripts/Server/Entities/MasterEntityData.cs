using System;
using System.Collections.Generic;
using UnityEngine;

using Network.Packet;
using static Network.Packet.Response.Types;
using Utils;

namespace Network.Server
{
    /// <summary>서버 엔티티의 기본 데이터 클래스</summary>
    public class MasterEntityData : BaseEntityData
    {
        [SerializeField] protected Collider mOwnerCollider;

        [SerializeField] protected int InitialHp = 100;
        public bool IsTeleport = false;

        public int MaxHp { get => InitialHp; }
        public float HpRegenRatio = 0.05f;
        public int HpRegenAmount => (int)(MaxHp * HpRegenRatio);

        // Action data
        public readonly List<EntityActionData> UdpEntityActionDataBuffer = new();
        public readonly List<EntityActionData> TcpEntityActionDataBuffer = new();

        // Evnet
        protected event Action<int> mDestroyEvent;
        protected event Action mOnCreated;
        protected event Action mOnDestroyed;

        private bool mIsImmortalState = false;

        public void SetImmortal(bool isImmortal)
        {
            mIsImmortalState = isImmortal;
        }

        public virtual void LateUpdate()
        {
            Position.Value = transform.position;
            Rotation.Value = transform.rotation;
        }

        protected virtual void Awake()
        {
            // Server side entity properties
            mInitialLayerIndex = gameObject.layer;
        }

        //public void OnDestroy()
        //{
        //    ActionDie();
        //}

        //public void OnDisable()
        //{
        //    ActionDie();
        //}

        public virtual void Initialize(int entityID, FactionType faction, Vector3 position, Quaternion rotation, bool isEnabled, Action<int> destroyEvent)
        {
            // Base entity information data
            EntityID = entityID;
            FactionType = faction;

            // Server controlled data
            IsEnabled.Value = isEnabled;
            BindedClientID.Value = -1;
            EquippedWeaponType.Value = ItemType.kNoneItemType;
            EquippedWeaponEntityID.Value = -1;

            // Transform data
            transform.position = position;
            transform.rotation = rotation;

            this.Position.Value = position;
            this.Rotation.Value = rotation;

            // Gameplay state data
            Hp.Value = InitialHp;
            IsAlive.Value = true;

            // Server side entity properties
            OnlyGroundPhysicsEnable = false;

            mDestroyEvent = null;
            mDestroyEvent += destroyEvent;

            mOnCreated?.Invoke();
        }

        public void ActionTakeDamage(DamageInfo info)
        {
            if (mIsImmortalState)
            {
                return;
            }

            if (Hp.Value <= 0)
                return;

            Hp.Value -= calculateDamage(info);

            if (this.FactionType == info.AttacterFaction)
            {

            }

            if (Hp.Value <= 0)
            {
                Hp.Value = 0;
                Debug.Log($"[World] Entity : {EntityID} Action Die");
                ActionDie();
            }
            else if (Hp.Value > InitialHp)
            {
                Hp.Value = InitialHp;
            }
        }

        /// <summary>데미지를 계산합니다.</summary>
        /// <param name="info">Detector 정보</param>
        /// <returns>데미지 량</returns>
        private int calculateDamage(DamageInfo info)
        {
            if (this.FactionType == info.AttacterFaction && info.damage >= 0)
            {
                return (int)(info.damage * ServerConfiguration.FriendlyFireDamageReduceRatio);
            }

            return info.damage;
        }

        // 객체의 소멸
        public void ActionDestroy()
        {
            var data = GetBaseEntityActionData(EntityAction.kDestroy);

            TcpEntityActionDataBuffer.Add(data.Build());

            mDestroyEvent.Invoke(EntityID);
        }

        // 객체의 사망
        public void ActionDie()
        {
            mOnDestroyed?.Invoke();

            IsEnabled.Value = false;

            Hp.Value = 0;
            IsAlive.Value = false;

            BindedClientID.Value = -1;

            OnlyGroundPhysicsEnable = true;

            var data = GetBaseEntityActionData(EntityAction.kDie)
                .SetLastEntityStateData(GetAllEntityStateData());

            TcpEntityActionDataBuffer.Add(data.Build());

            mDestroyEvent?.Invoke(EntityID);
        }

        public void ActionFullRegenHp()
        {
            Hp.Value = InitialHp;
        }

        public void ActionRegenHp()
        {
            Hp.Value += HpRegenAmount;

            if (Hp.Value > MaxHp)
            {
                Hp.Value = MaxHp;
            }

            var actionData = GetBaseEntityActionData(EntityAction.kRegenHp).Build();

            TcpEntityActionDataBuffer.Add(actionData);
        }

        public virtual void ActionUseWeapon(Vector3 useDirection, bool ignoreAttackDelay) { }

        public virtual void ActionAttack(int attackCode, Action onDetected) { }

        public EntityActionData.Builder GetBaseEntityActionData(EntityAction action)
        {
            return EntityActionData.CreateBuilder()
                .SetAction(action)
                .SetEntityId(EntityID);
        }

        /// <summary>Enity의 변화된 state 데이터를 반환한다. 바뀌지 않았다면 null을 반환한다.</summary>
        public bool TryGetChangedEntityStateDataOrNull(out EntityStateData data)
        {
            if (IsEnabled.IsDirty ||
                BindedClientID.IsDirty ||
                EquippedWeaponType.IsDirty ||
                //EquippedWeaponEntityID.IsDirty ||
                Hp.IsDirty ||
                IsAlive.IsDirty)
            {
                // Base entity information data
                var builder = EntityStateData.CreateBuilder()
                .SetEntityId(EntityID)
                .SetFactionType(FactionType);

                // Server controlled data
                if (IsEnabled.GetDirtyAndClear(out var isEnabledData))
                    builder.SetIsEnabled(isEnabledData);

                if (BindedClientID.GetDirtyAndClear(out var bindedClientIdData))
                    builder.SetBindedClientId(bindedClientIdData);

                if (EquippedWeaponType.GetDirtyValue(out var weaponType))
                    builder.SetEquippedWeaponType(weaponType);

                if (EquippedWeaponEntityID.GetDirtyAndClear(out var equippedWeaponEntityIdData))
                    builder.SetEquippedWeaponEntityId(equippedWeaponEntityIdData);

                // Gameplay state data
                if (Hp.GetDirtyAndClear(out var hpData))
                    builder.SetHp(hpData);

                if (IsAlive.GetDirtyAndClear(out var isAliveData))
                    builder.SetIsAlive(isAliveData);

                data = builder.Build();

                return true;
            }
            else
            {
                data = null;
                return false;
            }
        }

        /// <summary>Entity의 모든 State를 반환합니다.</summary>
        public EntityStateData GetAllEntityStateData()
        {
            // Base entity information data
            var builder = EntityStateData.CreateBuilder()
                .SetEntityId(EntityID)
                .SetEntityType(EntityType)
                .SetFactionType(FactionType)

            // Server controlled data
                .SetIsEnabled(IsEnabled.Value)
                .SetBindedClientId(BindedClientID.Value)
                .SetEquippedWeaponType(EquippedWeaponType.Value)
                .SetEquippedWeaponEntityId(EquippedWeaponEntityID.Value)

            // Gameplay state data
                .SetHp(Hp.Value)
                .SetIsAlive(IsAlive.Value);

            return builder.Build();
        }

        public bool TryGetEntityTransformData(out EntityTransformData data)
        {
            // If there is any change of transform then push every tranform data, So that can be interpolated.
            if (Position.IsDirty || Rotation.IsDirty || Velocity.IsDirty || IsTeleport)
            {
                var builder = EntityTransformData.CreateBuilder()
                    .SetEntityId(EntityID)
                    .SetPosition(Position.Value.ToData())
                    .SetRotation(Rotation.Value.ToData())
                    .SetVelocity(Velocity.Value.ToData());

                if (IsTeleport)
                {
                    builder.SetIsTeleported(true);
                    IsTeleport = false;
                }

                Position.SetPristine();
                Rotation.SetPristine();
                Velocity.SetPristine();

                data = builder.Build();

                return true;
            }
            else
            {
                data = null;
                return false;
            }
        }

        [Obsolete("패킷 최적화가 되지 않음")]
        public EntityTransformData.Builder GetEntityTransformData()
        {
            return EntityTransformData.CreateBuilder()
                .SetEntityId(EntityID)
                .SetPosition(Position.Value.ToData())
                .SetRotation(Rotation.Value.ToData())
                .SetVelocity(Velocity.Value.ToData());
        }

        public EntitySpawnData.Builder GetSpawnData()
        {
            return EntitySpawnData.CreateBuilder()
                .SetEntityId(EntityID)
                .SetEntityType(EntityType)
                .SetEntityStateData(GetEntitySpawnStateData())
                .SetSpawnPosition(Position.Value.ToData())
                .SetSpawnRotation(Rotation.Value.ToData());
        }

        public EntityStateData.Builder GetEntitySpawnStateData()
        {
            // Base entity information data
            return EntityStateData.CreateBuilder()
                .SetEntityId(EntityID)
                .SetEntityType(EntityType)
                .SetFactionType(FactionType)

                // Server controlled data
                .SetIsEnabled(IsEnabled.Value)
                .SetBindedClientId(BindedClientID.Value)
                .SetEquippedWeaponType(EquippedWeaponType.Value)
                .SetEquippedWeaponEntityId(EquippedWeaponEntityID.Value)

                // Transform data
                //.SetPosition(Position.Value.ToData())
                //.SetRotation(Rotation.Value.ToData())
                //.SetVelocity(Velocity.Value.ToData())

                // Gameplay state data
                .SetHp(Hp.Value)
                .SetIsAlive(IsAlive.Value);
        }
    }
}