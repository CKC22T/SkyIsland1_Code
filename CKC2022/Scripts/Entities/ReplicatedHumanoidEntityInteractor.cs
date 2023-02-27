using CKC2022;
using CKC2022.Input;
using CulterLib.Presets;
using Network.Client;
using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using Utils;
using Network.Common;

namespace Network.Client
{
    public class ReplicatedHumanoidEntityInteractor : MonoBehaviour
    {
        public static readonly Notifier<bool> GlobalAttackableState = new(true);

        [SerializeField]
        private ReplicatedEntityData data;

        public readonly Notifier<ReplicableItemObject> HoverWeapon = new();
        
        public readonly Notifier<InputContainer> InputContainer = new();

        public ReplicatedEntityData BindingTarget { get => data; }

        public bool Active { get; private set; }

        public float DetectorCreationDelay
        {
            get
            {
                if (data.EquippedWeaponType.Value == Packet.ItemType.kNoneItemType)
                    return 0;

                if (!ItemManager.TryGetConfig(data.EquippedWeaponType.Value, out var config))
                    return 0;

                return config.GENERATION_DELAY;
            }
        }

        public float LookAtWeight { get; set; }

        private float lastWeaponUsedTime;

        private void Awake()
        {
            data.BindedClientID.OnDataChanged += BindedClientID_OnDataChanged;
            GrassTextureInteraction.players.Add(data);
        }

        public void Initialize(in InputContainer container)
        {
            InputContainer.Value = container;
        }

        public void SetActiveInteraction(bool active)
        {
            Active = active;
        }

        public void SetWeaponUsedTime()
        {
            lastWeaponUsedTime = Time.time;
        }


        private void BindedClientID_OnDataChanged(int clientID)
        {
            if (clientID == ClientSessionManager.Instance.SessionID)
            {
                PlayerInputNetworkManager.Instance.AddInitializeInvocation(this);
            }
            else
            {
                PlayerInputNetworkManager.Instance.ReleaseInteractor(this);
            }
        }

        private void Update()
        {
            if (!Active)
                return;

            CastMouseInput();
        }


        public bool CheckWeaponCanBeUsed()
        {
            if (data.EquippedWeaponType.Value == Packet.ItemType.kNoneItemType)
                return false;

            if (!ItemManager.TryGetConfig(data.EquippedWeaponType.Value, out var config))
                return false;

            var diff = Time.time - lastWeaponUsedTime;
            if (diff < config.FIRE_DELAY)
                return false;

            if (GlobalAttackableState.Value == false)
                return false;

            //confirmed
            return true;
        }
        
        private bool CheckEquipable(ReplicableItemObject item)
        {
            var dir = (item.Position.Value.ToXZ() - BindingTarget.transform.position.ToXZ());

            if (dir.magnitude < 1)
                return true;

            if (Vector2.Dot(BindingTarget.transform.forward.ToXZ(), dir.normalized) < 0.7071f)
                return false;

            if (dir.magnitude > GlobalManager.Instance.DataMgr.vEquipDistance)
                return false;

            return true;
        }

        private bool CheckHoverWeapon(out ReplicableItemObject item)
        {
            item = null;

            if (!ItemObjectManager.TryGetInstance(out var itemObjectManager))
                return false;
            
            var targets = itemObjectManager.ItemObjects.Where(CheckEquipable);
            if (targets == null || targets.Count() == 0)
                return false;

            var target = targets.OrderBy(x => (x.Position.Value.ToXZ() - BindingTarget.transform.position.ToXZ()).magnitude).First();

            item = target;
            return true;
        }

        private void CastMouseInput()
        {
            if (BindingTarget == null)
                return;

            var hasSelection = CheckHoverWeapon(out var weapon);

            //HoverWeapon.Value?.HoverExit();
            HoverWeapon.Value = weapon;
            //HoverWeapon.Value?.HoverEnter();
        }

        private void OnDestroy()
        {
            GrassTextureInteraction.players.Remove(data);
            if (PlayerInputNetworkManager.IsQuitting == false)
                PlayerInputNetworkManager.Instance.ReleaseInteractor(this);
            data.BindedClientID.OnDataChanged -= BindedClientID_OnDataChanged;
        }
    }
}