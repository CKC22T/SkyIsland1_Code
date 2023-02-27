using UnityEngine;
using Network;
using System.Collections.Generic;
using Network.Packet;
using Network.Server;
using Utils;

public class MasterPlayerCharacterTable
{
    public class PlayerCharacterSet
    {
        public int SessionID { get; private set; } = -1;
        public MasterHumanoidEntityData Entity { get; private set; } = null;
        public GameObject PlayerGameObject => Entity.gameObject;

        public PlayerCharacterSet(int sessionID, MasterHumanoidEntityData entity)

        {
            SessionID = sessionID;
            Entity = entity;
        }
    }

    private Dictionary<int, PlayerCharacterSet> mPlayerTable = new();
    private Dictionary<EntityType, int> mSessionIdByEntityType = new();
    private List<MasterHumanoidEntityData> mPlayerEntities = new();
    public List<MasterHumanoidEntityData> PlayerEntities => mPlayerEntities;

    private int mAIControllerIdCounter = -10;

    public void Clear()
    {
        mPlayerTable.Clear();
        mSessionIdByEntityType.Clear();
        mPlayerEntities.Clear();

        mAIControllerIdCounter = -10;
    }

    /// <summary>존재하는 Player들의 Entity의 Operator를 재설정합니다.</summary>
    public void BindOperator()
    {
        foreach (var p in mPlayerTable.Values)
        {
            p.Entity.BindOperator(p.SessionID);
        }
    }

    #region Getter

    public bool IsEmpty()
    {
        return mPlayerEntities.IsEmpty();
    }

    public bool HasAnyAlivePlayer()
    {
        foreach (var e in mPlayerEntities)
        {
            if (e.BindedClientID.Value >= 0)
            {
                return true;
            }
        }

        return false;
    }

    public bool TryGetPlayerCharacterSetByType(EntityType characterType, out PlayerCharacterSet playerCharacterSet)
    {
        foreach (var p in mPlayerTable.Values)
        {
            if (p.Entity.EntityType == characterType)
            {
                playerCharacterSet = p;
                return true;
            }
        }

        playerCharacterSet = null;
        return false;
    }

    public bool TryGetPlayerCharacterSetByID(int sessionID, out PlayerCharacterSet playerCharacterSet)
    {
        return mPlayerTable.TryGetValue(sessionID, out playerCharacterSet);
    }

    public bool TryGetPlayerCharacterSetByEntity(MasterHumanoidEntityData entity, out PlayerCharacterSet playerCharacterSet)
    {
        foreach (var p in mPlayerTable.Values)
        {
            if (p.Entity == entity)
            {
                playerCharacterSet = p;
                return true;
            }
        }

        playerCharacterSet = null;
        return false;
    }

    public bool TryGetPlayerEntityByID(int sessionID, out MasterHumanoidEntityData entity)
    {
        if (TryGetPlayerCharacterSetByID(sessionID, out var playerCharacterSet))
        {
            entity = playerCharacterSet.Entity;
            return true;
        }

        entity = null;
        return false;
    }

    //public bool TryGetSessionIdByType(EntityType characterType, out int sessionID)
    //{
    //    return mSessionIdByEntityType.TryGetValue(characterType, out sessionID);
    //}

    public bool TryGetPlayerCharacterSetByEntityID(int entityID, out PlayerCharacterSet playerCharacterSet)
    {
        foreach (var p in mPlayerTable.Values)
        {
            if (p.Entity.EntityID == entityID)
            {
                playerCharacterSet = p;
                return true;
            }
        }

        playerCharacterSet = null;
        return false;
    }

    public bool HasPlayerByID(int sessionID)
    {
        return mPlayerTable.ContainsKey(sessionID);
    }

    public bool HasPlayerByType(EntityType playerType)
    {
        foreach (var p in mPlayerTable.Values)
        {
            if (p.Entity.EntityType == playerType)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasPlayerByEntity(MasterHumanoidEntityData entity)
    {
        foreach (var p in mPlayerTable.Values)
        {
            if (p.Entity == entity)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    public bool TryAdd(int sessionID, MasterHumanoidEntityData entity)
    {
        if (HasPlayerByID(sessionID) || HasPlayerByType(entity.EntityType))
            return false;

        mPlayerEntities.Add(entity);
        mPlayerTable.Add(sessionID, new PlayerCharacterSet(sessionID, entity));
        mSessionIdByEntityType.Add(entity.EntityType, sessionID);

        return true;
    }

    public void Remove(int sessionID)
    {
        if (mPlayerTable.ContainsKey(sessionID))
        {
            removeInternal(mPlayerTable[sessionID]);
        }
    }

    public void Remove(EntityType type)
    {
        if (TryGetPlayerCharacterSetByType(type, out var player))
        {
            removeInternal(player);
        }
    }

    public void Remove(MasterHumanoidEntityData entity)
    {
        if (TryGetPlayerCharacterSetByEntity(entity, out var player))
        {
            removeInternal(player);
        }
    }

    private void removeInternal(PlayerCharacterSet playerCharacterSet)
    {
        mPlayerEntities.Remove(playerCharacterSet.Entity);
        mSessionIdByEntityType.Remove(playerCharacterSet.Entity.EntityType);
        mPlayerTable.Remove(playerCharacterSet.SessionID);
    }
}
