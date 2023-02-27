using Network;
using Network.Packet;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Network.Packet.Response.Types;

public enum UserSessionState
{
    None = 0,

    Lobby_NotSelected, // 캐릭터가 선택되지 않음
    Lobby_Selected, // 캐릭터를 선택함
    Lobby_ReadyToStart, // 캐릭터를 선택하고 준비중

    SceneChanging_Loading, // 인게임으로 로딩중
    SceneChanging_ReadyToStart, // 초기 데이터를 수신받고 준비됨

    InGame_Playing, // 게임 플레이중
    InGame_Ending, // 엔딩 보는중
}

[Serializable]
public class SessionSlot
{
    [SerializeField]
    public NetIntData SessionID = new(-1);
    [SerializeField]
    public NetStringData Username = new NetStringData();
    [SerializeField]
    public NetEnumTypeData<UserSessionState> SessionState = new(UserSessionState.Lobby_NotSelected);
    [SerializeField]
    public NetEnumTypeData<EntityType> SelectedCharacterType = new(EntityType.kNoneEntityType);
    [SerializeField]
    public NetIntData EntityID = new NetIntData(-1);
    [SerializeField]
    public ItemInventory Inventory = new ItemInventory();

    public SessionSlot(int initialSessionID)
    {
        SessionID.Value = initialSessionID;
    }

    public void InitializeDataAsMaster(in MasterReplicationObject assignee)
    {
        assignee.AssignDataAsReliable(SessionID);
        assignee.AssignDataAsReliable(Username);
        assignee.AssignDataAsReliable(SessionState);
        assignee.AssignDataAsReliable(SelectedCharacterType);
        assignee.AssignDataAsReliable(EntityID);

        Inventory.InitializeDataAsMaster(assignee);
    }

    public void InitializeDataAsRemote(in RemoteReplicationObject assignee)
    {
        assignee.AssignDataAsReliable(SessionID);
        assignee.AssignDataAsReliable(Username);
        assignee.AssignDataAsReliable(SessionState);
        assignee.AssignDataAsReliable(SelectedCharacterType);
        assignee.AssignDataAsReliable(EntityID);

        Inventory.InitializeDataAsRemote(assignee);
    }

    public void Connected(int sessionID)
    {
        SessionID.Value = sessionID;
    }

    public void Disconnected()
    {
        Reset();
    }

    public void Reset()
    {
        SessionID.Value = -1;
        Username.Value = "";
        SessionState.Value = UserSessionState.Lobby_NotSelected;
        SelectedCharacterType.Value = EntityType.kNoneEntityType;
        EntityID.Value = -1;
        Inventory.Reset();
    }
}

public enum SessionStatisticsIntType
{
    GiveDamageEnemy,
    GetDamageEnemy,
    GiveHealEnemy,
    DieByEnemyCount,
    KillEnemyCount,

    GiveDamageFriendly,
    GetDamageFriendly,
    GiveHealFriendly,
    DieByFriendlyCount,
    KillFriendlyCount,

    GiveDamageBoss,
    FallCount,
}

public enum SessionStatisticsFloatType
{
    MoveDistance,
}

public class UserStatistics
{
    private readonly Dictionary<SessionStatisticsIntType, int> IntSessionStatistics = new();
    private readonly Dictionary<SessionStatisticsFloatType, int> FloatSessionStatistics = new();
    public EntityType EquippedWeaponType { get; private set; } = EntityType.kNoneEntityType;

    // Weapon statistics
    public int WeaponUsedCount { get; private set; } = 0;

    // Statistics
    public int WeaponObtainCount { get; private set; } = 0;
    public int WeaponDiscoveryCount { get; private set; } = 0;
    public List<EntityType> WeaponCollectList { get; private set; } = new();

    public int GiveDamageEnemy = 0;
    public int GetDamageEnemy = 0;
    public int GiveHealEnemy = 0;
    public int DieByEnemyCount = 0;
    public int KillEnemyCount = 0;

    public int GiveDamageFriendly = 0;
    public int GetDamageFriendly = 0;
    public int GiveHealFriendly = 0;
    public int DieByFriendlyCount = 0;
    public int KillFriendlyCount = 0;

    public int GiveDamageBoss = 0;
    public int fallCount = 0;

    public float MoveDistance = 0;

    public void AddWeaponUsedCount(int count = 1)
    {
        if (EquippedWeaponType.IsWeaponEntity() == false)
            return;

        if (count <= 0)
        {
            Debug.LogError(LogManager.GetLogMessage($"Wrong increasment count! : {count}", NetworkLogType.None, true));
            return;
        }

        WeaponUsedCount += count;
    }

    public void OnEquipWeapon(EntityType weaponType)
    {
        if (weaponType.IsWeaponEntity() == false)
        {
            Debug.LogError(LogManager.GetLogMessage($"There is no such weapon type as {weaponType}", NetworkLogType.None, true));
            return;
        }

        EquippedWeaponType = weaponType;

        WeaponObtainCount++;

        if (WeaponCollectList.Contains(weaponType) == false)
        {
            WeaponDiscoveryCount++;
            WeaponCollectList.Add(weaponType);
        }
    }

    public void OnUnequipWeapon()
    {
        EquippedWeaponType = EntityType.kNoneEntityType;
    }
}
