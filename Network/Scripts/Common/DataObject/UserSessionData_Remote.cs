using Network;
using Network.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum CharacterState
{
    None,
    NotSelected,
    Selected,
    Readied,
}

public class UserSessionData_Remote : RemoteNetObject
{
    [Sirenix.OdinInspector.ShowInInspector]
    public SessionSlotCollection SessionSlots = new();

    public event Action OnPlayerEntityChanged;
    private bool mIsPlayerEntityChanged = false;

    public override void InitializeData(in RemoteReplicationObject assignee)
    {
        SessionSlots.InitializeDataAsRemote(assignee);
        SessionSlots.OnPlayerEntityChanged += onPlayerEntityChanged;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        SessionSlots.OnPlayerEntityChanged -= onPlayerEntityChanged;
    }

    private void onPlayerEntityChanged()
    {
        mIsPlayerEntityChanged = true;
    }

    public void ResetData()
    {
        SessionSlots.ResetData();
    }

    public void FixedUpdate()
    {
        if (mIsPlayerEntityChanged)
        {
            mIsPlayerEntityChanged = false;
            OnPlayerEntityChanged?.Invoke();
        }
    }

    #region Getter

    public bool IsAlive(int sessionID)
    {
        if (SessionSlots.TryGetSlot(sessionID, out var slot))
        {
            return slot.EntityID.Value >= 0;
        }

        return false;
    }

    #endregion

    #region Notification

    public bool IsCurrentlySquadLeader()
    {
        return SessionSlots.IsSquadLeader(ClientSessionManager.Instance.SessionID);
    }

    public bool IsSquadLeaderCharacter(EntityType characterType)
    {
        if (SessionSlots.TryGetSlot(characterType, out var slot))
        {
            return slot.SessionID.Value == SessionSlots.SquadLeaderSessionID.Value;
        }
        else
        {
            return false;
        }
    }

    public CharacterState GetLobbyReadyStateByCharacterType(EntityType characterType)
    {
        if (SessionSlots.TryGetSlot(characterType, out var slot))
        {
            switch (slot.SessionState.Value)
            {
                case UserSessionState.Lobby_NotSelected:
                    return CharacterState.NotSelected;

                case UserSessionState.Lobby_Selected:
                    return CharacterState.Selected;

                case UserSessionState.Lobby_ReadyToStart:
                    return CharacterState.Readied;

                default:
                    return CharacterState.None;
            }
        }

        return CharacterState.None;
    }

    public string GetUsernameByCharacterType(EntityType characterType)
    {
        if (SessionSlots.TryGetSlot(characterType, out var slot))
        {
            return slot.Username.Value;
        }

        return GlobalTable.GetEntityName(characterType);
    }

    #endregion
}
