using System;
using UnityEngine;

using Network.Packet;
using static Network.Packet.Request.Types;
using static Network.Packet.Response.Types;
using Utils;
using UnityEngine.AI;
using Sirenix.OdinInspector;
using CulterLib.Game.Chr;
using Network.Common;

namespace Network.Server
{
    //서버에서 관리하는 엔티티 데이터. 실제 이동 연산 및 물리 처리가 진행.
    public class MasterHumanoidEntityData : MasterEntityData
    {
        // AI
        [TabGroup("Component"), SerializeField] NavMeshAgent mAgent;
        [TabGroup("Component"), SerializeField] CharacterPhysics mPhysics;
        [TabGroup("Component"), SerializeField] CharacterActionManager mActionManager;

        [SerializeField] protected Transform mWeaponEquipSocket;
        // 오딘으로 무기 타입인지 검증 필요

        public bool HasWeapon => EquippedWeaponType.Value.IsWeapon();
        public MasterWeaponEntityData WeaponEntity => mWeaponEntityData;
        protected MasterWeaponEntityData mWeaponEntityData = null;
        public CharacterActionManager ActionManager => mActionManager;

        public Transform WeaponEquipSocket => mWeaponEquipSocket;

        [Obsolete("use InputViewDirection instead")]
        public Vector3 InputMousePosition = Vector3.zero;

        public Vector2 InputMoveDirection = Vector2.zero;
        public Vector2 InputViewDirection = Vector2.zero;

        public float MoveSpeed = 2f;

        private ulong mMoveInputPacketID = 0;
        private ulong mFireInputPacketID = 0;

        private Vector3 mLastedMoveVel = new Vector3(1, 0, 0);
        public bool ShouldTeleport = true; // 기본값

        public bool CanFire => mFireDelay <= 0;
        private float mFireDelay = 0;

        /// <summary>
        /// 해당 캐릭터의 CharacterPhysics
        /// </summary>
        public CharacterPhysics Physics { get => mPhysics; }

        /// <summary>
        /// 해당 캐릭터의 NavMeshAgent
        /// </summary>
        public NavMeshAgent Agent { get => mAgent; }

        public override void Initialize(int entityID, FactionType faction, Vector3 position, Quaternion rotation, bool isEnabled, Action<int> destroyEvent)
        {
            base.Initialize(entityID, faction, position, rotation, isEnabled, destroyEvent);

            mMoveInputPacketID = 0;
            mFireInputPacketID = 0;
        }

        public void OnEnable()
        {
            mMoveInputPacketID = 0;
            mFireInputPacketID = 0;
        }

        protected override void Awake()
        {
            base.Awake();
            mOnCreated += OnCreated;
        }

        public void FixedUpdate()
        {
            if (mFireDelay > 0)
            {
                mFireDelay -= Time.fixedDeltaTime;
            }

            var info = new PlayerEntityBehavior.MovementSimulationInformation
            {
                moveDirection = InputMoveDirection,
                ChracterViewDirection = InputViewDirection,
                moveSpeed = MoveSpeed,
                deltaTime = Time.fixedDeltaTime
            };
            PlayerEntityBehavior.ProcessMoving(transform, mPhysics, info);

            //이동 방향 설정
            if (Agent && Agent.hasPath && 1 < Agent.path.corners.Length)
            {
                Velocity.Value = (Agent.path.corners[1] - Agent.transform.position).SetY(0).normalized;
                if (0.1f < Velocity.Value.magnitude)
                    mLastedMoveVel = Velocity.Value;
            }
            else if (BindedClientID.Value < 0)
                Velocity.Value = Vector3.zero;

            if (Agent && Agent.enabled)
                transform.LookAt(transform.position + mLastedMoveVel);
        }

        public void OnCreated()
        {
            InputViewDirection = Vector2.down;

            //ObtainWeapon(mDefaultWeaponType);

            //AI 초기화
            if (mActionManager)
            {
                mActionManager.Initialize(this);

                void onBindedClientIdChanged()
                {   //해당 엔티티를 조작하고 있는 인간이 없으면 AI 실행, 있으면 실행안함
                    if (0 <= BindedClientID.Value)
                        mActionManager.SetAction(mActionManager.IdActions["Off"]);
                    else
                        mActionManager.SetAction(mActionManager.IdActions["Idle"]);
                }
                BindedClientID.OnChanged += onBindedClientIdChanged;
                onBindedClientIdChanged();
            }
        }

        /// <summary>해당 아이템의 무기를 습득합니다.</summary>
        /// <param name="itemID"></param>
        public void ActionObtainWeapon(int itemID)
        {
            mFireDelay = 0;

            if (!ItemObjectManager.TryGetInstance(out var itemObjectManager))
                return;

            // Player character가 아닐 때
            if (!itemObjectManager.TryGetItemByID(itemID, out var itemObject))
                return;

            ItemType weaponType = itemObject.ItemType;

            if (!weaponType.IsWeapon())
                return;

            // Player Character일 때
            if (ServerPlayerCharacterManager.TryGetInstance(out var playerManager))
            {
                if (playerManager.HasCharacter(this))
                {
                    if (ServerSessionManager.Instance.TryObtainWeapon(this.BindedClientID.Value, weaponType))
                    {
                        EquippedWeaponType.Value = weaponType;
                        itemObjectManager.DestroyItemObjectAsMaster(itemID);
                        return;
                    }
                }
            }

            // Player Character가 아닐 때
            EquippedWeaponType.Value = weaponType;
            itemObjectManager.DestroyItemObjectAsMaster(itemID);
        }

        /// <summary>현재 들고 있는 무기를 강제로 바꿉니다. 아이템을 드랍하지 않습니다.</summary>
        public void ActionForceEquipWeapon(ItemType weaponType)
        {
            mFireDelay = 0;

            EquippedWeaponType.Value = weaponType;

            var data = GetBaseEntityActionData(EntityAction.kEquipWeapon);
            TcpEntityActionDataBuffer.Add(data.Build());
        }

        /// <summary>현재 들고 있는 무기를 떨어뜨립니다.</summary>
        public void ActionDropWeapon()
        {
            EquippedWeaponType.Value = ItemType.kNoneItemType;
        }

        [Obsolete("무기는 더 이상 Entity가 아니라 Item으로 취급된다.")]
        /// <summary>가지고 있는 무기를 장착 해제하고, 새로운 무기를 획득합니다.</summary>
        public void ObtainWeapon(EntityType weaponType)
        {
            mFireDelay = 0;

            if (!weaponType.IsWeaponEntity())
                return;

            //ActionUnequipWeapon();

            var weaponEntity = ServerMasterEntityManager.Instance
                .CreateNewEntity(weaponType, FactionType.kNeutral, WeaponEquipSocket.position, WeaponEquipSocket.rotation, true);

            ActionEquipWeapon(weaponEntity as MasterWeaponEntityData);
        }

        ///// <summary>장착하고 있는 무기를 사용합니다.</summary>
        //public override void ActionUseWeapon(Vector3 useDirection)
        //{
        //    if (!HasWeapon)
        //        return;

        //    Vector3 shooterPosition = new Vector3(Position.Value.x, mWeaponEquipSocket.position.y, Position.Value.z);
        //    mWeaponEntityData.Use(shooterPosition, useDirection, EntityID, mOwnerCollider);

        //    var data = GetBaseEntityActionData(EntityAction.kUseWeapon);
        //    TcpEntityActionDataBuffer.Add(data.Build());
        //}

        /// <summary>장착하고 있는 무기를 사용합니다.</summary>
        public override void ActionUseWeapon(Vector3 useDirection, bool ignoreAttackDelay = false)
        {
            if (!ignoreAttackDelay && !CanFire)
            {
                return;
            }

            var currentWeapon = EquippedWeaponType.Value;

            if (!currentWeapon.IsWeapon())
            {
                return;
            }

            Vector3 shooterPosition = new Vector3(Position.Value.x, mWeaponEquipSocket.position.y, Position.Value.z);

            if (!ItemManager.TryGetInstance(out var manager))
            {
                Debug.LogError(LogManager.GetLogMessage($"There is no ItemManager to use weapon!", NetworkLogType.MasterServer, true));
                return;
            }

            if (!manager.tryGetConfig(currentWeapon, out var detectorConfig))
            {
                Debug.LogError(LogManager.GetLogMessage($"There is no detector config to create detector!", NetworkLogType.MasterServer, true));
                return;
            }

            mFireDelay = detectorConfig.FIRE_DELAY;

            DetectorInfo info = new DetectorInfo()
            {
                Origin = shooterPosition,
                Direction = useDirection.normalized,
                RawViewVector = useDirection,
                OwnerCollider = mOwnerCollider,
                OwnerEntityID = EntityID,
                DamageInfo = new DamageInfo((int)detectorConfig.DAMAGE, FactionType)
            };

            ServerMasterDetectorManager.Instance.CreateNewDetector(detectorConfig.DETECTOR_TYPE, info);

            var data = GetBaseEntityActionData(EntityAction.kUseWeapon);
            TcpEntityActionDataBuffer.Add(data.Build());
        }

        [Obsolete("무기는 더 이상 Entity가 아니라 Item으로 취급된다.")]
        /// <summary>무기를 장착합니다.</summary>
        public void ActionEquipWeapon(MasterWeaponEntityData weaponEntity)
        {
            //ActionUnequipWeapon();

            mWeaponEntityData = weaponEntity;
            mWeaponEntityData.EquippedByEntity(this);

            EquippedWeaponEntityID.Value = mWeaponEntityData.EntityID;

            var data = GetBaseEntityActionData(EntityAction.kEquipWeapon)
                .SetEquippedWeaponEntityId(weaponEntity.EntityID);
            TcpEntityActionDataBuffer.Add(data.Build());
        }

        [Obsolete("무기는 더 이상 Entity가 아니라 Item으로 취급된다.")]
        /// <summary>무기 장착을 해제합니다.</summary>
        public void ActionUnequipWeapon()
        {
            if (!HasWeapon)
                return;

            EquippedWeaponEntityID.Value = -1;

            // TODO : 장착 해제된 무기 회전 및 위치 바꾸기
            mWeaponEntityData.Unequipped();
            mWeaponEntityData = null;

            var data = GetBaseEntityActionData(EntityAction.kUnequipWeapon);
            TcpEntityActionDataBuffer.Add(data.Build());
        }

        public void ActionTrigger(string actionString)
        {
            var actionTrigger = GetBaseEntityActionData(EntityAction.kTriggerString)
                .SetTriggerString(actionString);

            TcpEntityActionDataBuffer.Add(actionTrigger.Build());
        }

        [Obsolete("Entity의 이전 발생 액션 동기화는 현 시점에서 필요하지 않다.")]
        public EntityActionData GetActedActionData()
        {
            var actionData = GetBaseEntityActionData(EntityAction.kEquipWeapon);

            if (EquippedWeaponEntityID.Value >= 0)
            {
                actionData.SetEquippedWeaponEntityId(EquippedWeaponEntityID.Value);
                return actionData.Build();
            }
            else
            {
                return null;
            }
        }

        public void BindOperator(int operatorID)
        {
            BindedClientID.Value = operatorID;
            mMoveInputPacketID = 0;
            mFireInputPacketID = 0;
        }

        public void UnbindOperator()
        {
            BindedClientID.Value = -1;
            mMoveInputPacketID = 0;
            mFireInputPacketID = 0;
        }

        public void ProcessInputFromPlayer(InputData inputData, ulong packetID)
        {
            // TODO : Ignore previous packet
            //if (packetID > mMoveInputPacketID)
            {
                if (!IsEnabled.Value)
                    return;

                if (inputData.HasMovementDirection)
                {
                    InputMoveDirection = inputData.MovementDirection.ToVector2();
                    Velocity.Value = InputMoveDirection.normalized;
                    mMoveInputPacketID = packetID;
                }

                if (inputData.HasMousePosition)
                {
                    InputMousePosition = inputData.MousePosition.ToVector3();
                    mMoveInputPacketID = packetID;
                }

                if(inputData.HasViewDirection)
                {
                    if (inputData.ViewDirection.ToVector2().magnitude > Vector2.kEpsilon)
                        InputViewDirection = inputData.ViewDirection.ToVector2();
                    mMoveInputPacketID = packetID;
                }

            }

            // Jump
            if (inputData.HasJumpWish && inputData.JumpWish)
            {
                mPhysics.Jump();
            }

            if (packetID > mFireInputPacketID)
            {
                if (HasWeapon && inputData.HasUseWeaponData)
                {
                    mFireInputPacketID = packetID;
                    UseWeaponData useData = inputData.UseWeaponData;

                    if (useData.HasDirection && useData.HasOrigin)
                    {
                        Vector3 direction = useData.Direction.ToVector2().ToVector3FromXZ();
                        ActionUseWeapon(direction, true);
                    }
                }
            }
        }

        public void Teleport(Vector3 teleportPosition)
        {
            mAgent.Warp(teleportPosition);
            Physics.Teleport(teleportPosition);
            gameObject.transform.position = teleportPosition;
            Position.Value = teleportPosition;
            IsTeleport = true;
        }
    }
}
