using Network;
using Network.Packet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[Serializable]
public class SessionSlotCollection : INetworkAssignable
{
    [Sirenix.OdinInspector.ShowInInspector]
    public NetIntData SquadLeaderSessionID = new NetIntData(-1);

    [Sirenix.OdinInspector.ShowInInspector]
    public List<SessionSlot> mSessionSlots { get; private set; } = new List<SessionSlot>();

    public event Action OnPlayerEntityChanged;

    public void InitializeDataAsMaster(in MasterReplicationObject assignee)
    {
        // Network setup
        assignee.AssignDataAsReliable(SquadLeaderSessionID);

        for (int i = 0; i < ServerConfiguration.MAX_PLAYER; i++)
        {
            var slot = new SessionSlot((i + 1) * -10);

            slot.InitializeDataAsMaster(assignee);

            mSessionSlots.Add(slot);
        }
    }

    public void InitializeDataAsRemote(in RemoteReplicationObject assignee)
    {
        // Network setup
        assignee.AssignDataAsReliable(SquadLeaderSessionID);

        for (int i = 0; i < ServerConfiguration.MAX_PLAYER; i++)
        {
            var slot = new SessionSlot((i + 1) * -10);

            slot.InitializeDataAsRemote(assignee);

            mSessionSlots.Add(slot);
        }

        // Event setup for client
        foreach (var slot in mSessionSlots)
        {
            slot.EntityID.OnChanged += onPlayerEntityChanged;
        }
    }

    public void ResetData()
    {
        SquadLeaderSessionID.Value = -1;

        foreach (var slot in mSessionSlots)
        {
            slot.Reset();
        }
    }

    public void ResetInventory()
    {
        foreach (var slot in mSessionSlots)
        {
            slot.Inventory.Reset();
        }
    }

    private void onPlayerEntityChanged()
    {
        OnPlayerEntityChanged?.Invoke();
    }

    #region Operation

    public void NotifierEntityID(int sessionID, int entityID)
    {
        if (TryGetSlot(sessionID, out var slot))
        {
            slot.EntityID.Value = entityID;
        }
    }

    public void GiveEntityTypeToAI()
    {
        List<EntityType> playerEntityTypes = new(ServerConfiguration.PlayerEntityTypes);

        foreach (var slot in mSessionSlots)
        {
            playerEntityTypes.Remove(slot.SelectedCharacterType.Value);
        }

        Queue<EntityType> leftEntityTypes = new(playerEntityTypes);

        foreach (var slot in mSessionSlots)
        {
            if (slot.SelectedCharacterType.Value.IsPlayerEntity())
                continue;

            if (leftEntityTypes.IsEmpty())
                return;

            slot.SelectedCharacterType.Value = leftEntityTypes.Dequeue();
        }
    }

    public void SaveAllInventory()
    {
        foreach (var slot in mSessionSlots)
        {
            slot.Inventory.SaveCurrentInventory();
        }
    }

    public void LoadAllInventory()
    {
        foreach (var slot in mSessionSlots)
        {
            slot.Inventory.LoadSavedInventory();
        }
    }

    public void ProvideDefaultWeapon(ItemType defaultWeapon)
    {
        foreach (var slot in mSessionSlots)
        {
            if (!slot.Inventory.HasAnyWeapon())
            {
                slot.Inventory.ObtainItem(defaultWeapon);
            }
        }
    }

    public void OnConnected(int sessionID)
    {
        // 첫번재 접속자가 방장
        if (SquadLeaderSessionID.Value < 0)
        {
            SquadLeaderSessionID.Value = sessionID;
        }

        var emptySlot = GetEmptySlotOrNull();

        if (emptySlot == null)
        {
            return;
        }

        emptySlot.Connected(sessionID);
    }

    public void OnDisconnected(int sessionID)
    {
        GetSlotOrNull(sessionID)?.Disconnected();
    }

    public ServerOperationResult ObtainItemType(int sessionID, ItemType obtainItemType)
    {
        var result = ServerOperationResult.Inventory_WrongOperation;

        if (TryGetSlot(sessionID, out var slot))
        {
            result = slot.Inventory.ObtainItem(obtainItemType);
        }

        return result;
    }

    public ServerOperationResult DropWeaponItem(int sessionID, out ItemType droppedItem)
    {
        droppedItem = ItemType.kNoneItemType;

        var result = ServerOperationResult.Inventory_WrongOperation;
        if (TryGetSlot(sessionID, out var slot))
        {
            result = slot.Inventory.DropWeapon(out droppedItem);
        }

        return result;
    }

    public List<ItemType> DropAllWeaponItem(int sessionID)
    {
        if (TryGetSlot(sessionID, out var slot))
        {
            return slot.Inventory.DropAllWeapon();
        }
        else
        {
            return null;
        }
    }

    public void UpdateAllSessionState(UserSessionState state)
    {
        foreach (var slot in mSessionSlots)
        {
            slot.SessionState.Value = state;
        }
    }

    #endregion

    #region Getter

    public bool TryGetPlayerEntityIDs(out List<int> playerEntityIDs)
    {
        playerEntityIDs = new List<int>();

        foreach (var slot in mSessionSlots)
        {
            if (slot.EntityID.Value >= 0)
            {
                playerEntityIDs.Add(slot.EntityID.Value);
            }
        }

        return !playerEntityIDs.IsEmpty();
    }

    public bool TryGetEquipItem(int sessionID, out ItemType equippedWeapon)
    {
        if (TryGetSlot(sessionID, out var slot))
        {
            equippedWeapon = slot.Inventory.GetEquipWeapon();
            return true;
        }

        equippedWeapon = ItemType.kNoneItemType;
        return false;
    }

    public bool TryGetWeaponCount(int sessionID, out int weaponCount)
    {
        if (TryGetSlot(sessionID, out var slot))
        {
            weaponCount = slot.Inventory.GetWeaponCount();
            return true;
        }

        weaponCount = 0;
        return false;
    }

    public bool TryGetSlot(int sessionID, out SessionSlot slot)
    {
        slot = GetSlotOrNull(sessionID);
        return (slot != null);
    }

    public bool TryGetSlot(EntityType playerEntityType, out SessionSlot slot)
    {
        slot = GetSlotOrNull(playerEntityType);
        return (slot != null);
    }

    public bool TryGetUnarmedPlayerIdQueue(out Queue<int> unarmedPlayerIdQueue)
    {
        unarmedPlayerIdQueue = new Queue<int>();

        foreach (var slot in mSessionSlots)
        {
            if (!slot.Inventory.HasAnyWeapon())
            {
                unarmedPlayerIdQueue.Enqueue(slot.SessionID.Value);
            }
        }

        return !unarmedPlayerIdQueue.IsEmpty();
    }

    public int GetSessionCount()
    {
        int count = 0;

        foreach (SessionSlot slot in mSessionSlots)
        {
            if (slot.SessionID.Value >= 0)
            {
                count++;
            }
        }

        return count;
    }

    public SessionSlot GetSlotOrNull(int sessionID)
    {
        foreach (SessionSlot slot in mSessionSlots)
        {
            if (slot.SessionID.Value == sessionID)
            {
                return slot;
            }
        }

        return null;
    }

    public SessionSlot GetSlotOrNull(EntityType characterType)
    {
        foreach (var slot in mSessionSlots)
        {
            if (slot.SelectedCharacterType.Value == characterType)
            {
                return slot;
            }
        }

        return null;
    }

    public SessionSlot GetEmptySlotOrNull()
    {
        foreach (SessionSlot slot in mSessionSlots)
        {
            if (slot.SessionID.Value < 0)
            {
                return slot;
            }
        }

        return null;
    }

    public EntityType GetSelectedEntityTypeByClientID(int sessionID)
    {
        var slot = GetSlotOrNull(sessionID);

        if (slot == null)
        {
            return EntityType.kNoneEntityType;
        }

        return slot.SelectedCharacterType.Value;
    }

    public int GetSessionIDByEntityType(EntityType entityType)
    {
        var slot = GetSlotOrNull(entityType);
        if (slot == null)
        {
            Debug.LogError(LogManager.GetLogMessage($"There is no player entity type \"{entityType}\"", hasError : true));
            return -1;
        }
        else
        {
            return slot.SessionID.Value;
        }
    }

    public List<SessionSlot> GetConnectedSlots()
    {
        List<SessionSlot> slots = new List<SessionSlot>();

        foreach (SessionSlot slot in mSessionSlots)
        {
            if (slot.SessionID.Value >= 0)
            {
                slots.Add(slot);
            }
        }

        return slots;
    }

    public List<int> GetConnectedSessionIDs()
    {
        List<int> connectedSessionIDs = new List<int>();

        foreach (var slot in GetConnectedSlots())
        {
            connectedSessionIDs.Add(slot.SessionID.Value);
        }

        return connectedSessionIDs;
    }

    public bool IsAllSlotEmpty()
    {
        return GetSessionCount() <= 0;
    }

    public bool IsSelected(EntityType playerType)
    {
        foreach (var slot in mSessionSlots)
        {
            if (slot.SelectedCharacterType.Value == playerType)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsSquadLeader(int sessionID)
    {
        return SquadLeaderSessionID.Value == sessionID;
    }

    public bool IsConnected(int sessionID)
    {
        return TryGetSlot(sessionID, out var slot);
    }

    public bool HasExtraSlot()
    {
        return GetSessionCount() < ServerConfiguration.MAX_PLAYER;
    }

    public bool Contains(int sessionID)
    {
        foreach (SessionSlot slot in mSessionSlots)
        {
            if (slot.SessionID.Value == sessionID)
            {
                return true;
            }
        }

        return false;
    }

    public bool AreSelectCharacter()
    {
        foreach (var slot in GetConnectedSlots())
        {
            if (!slot.SelectedCharacterType.Value.IsHumanoidEntity())
            {
                return false;
            }
        }

        return true;
    }

    public bool AreReadyExcept(int sessionID)
    {
        if (IsAllSlotEmpty())
        {
            return false;
        }

        foreach (SessionSlot slot in GetConnectedSlots())
        {
            if (slot.SessionID.Value == sessionID)
            {
                continue;
            }

            if (slot.SessionState.Value != UserSessionState.Lobby_ReadyToStart)
            {
                return false;
            }
        }

        return true;
    }

    public bool AreSceneChanging_ReadyToStart()
    {
        if (IsAllSlotEmpty())
        {
            return false;
        }

        foreach (var slot in GetConnectedSlots())
        {
            if (slot.SessionState.Value != UserSessionState.SceneChanging_ReadyToStart)
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}
