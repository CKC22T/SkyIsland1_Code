using Network;
using System;
using System.Collections.Generic;

[Serializable]
public class GameGlobalState : INetworkAssignable
{
    public int CheckPointNumber => CheckPointSystem.CheckPointNumber;

    [Sirenix.OdinInspector.ShowInInspector]
    public CheckPointSystem CheckPointSystem = new CheckPointSystem(ServerConfiguration.MAX_CHECK_POINT_COUNT);
    public NetIntData BridgeWaveCounter = new NetIntData(-1);
    public TurretState TurretState = new TurretState();
    public NetBooleanData LastCheckPointDoor = new NetBooleanData(false);
    public NetBooleanData BossBlockingRock = new NetBooleanData(false);

    public void InitializeDataAsMaster(in MasterReplicationObject assignee)
    {
        CheckPointSystem.InitializeDataAsMaster(assignee);
        assignee.AssignDataAsReliable(BridgeWaveCounter);
        TurretState.InitializeDataAsMaster(assignee);
        assignee.AssignDataAsReliable(LastCheckPointDoor);
        assignee.AssignDataAsReliable(BossBlockingRock);
    }

    public void InitializeDataAsRemote(in RemoteReplicationObject assignee)
    {
        CheckPointSystem.InitializeDataAsRemote(assignee);
        assignee.AssignDataAsReliable(BridgeWaveCounter);
        TurretState.InitializeDataAsRemote(assignee);
        assignee.AssignDataAsReliable(LastCheckPointDoor);
        assignee.AssignDataAsReliable(BossBlockingRock);
    }

    public void OnCheckPointReached(int checkPointIndex)
    {
        CheckPointSystem.CheckPoint(checkPointIndex);
    }

    public bool IsValidCheckPointIndex(int checkPointIndex)
    {
        return checkPointIndex >= CheckPointSystem.CheckPointNumber;
    }

    public void SetBridgeWaveCounter(int waveCounter)
    {
        BridgeWaveCounter.Value = waveCounter;
    }

    public void ResetData()
    {
        CheckPointSystem.ResetData();
        BridgeWaveCounter.Value = -1;
        TurretState.ResetData();
        LastCheckPointDoor.Value = false;
        BossBlockingRock.Value = false;
    }

    public void OnGameRestart()
    {
        BridgeWaveCounter.Value = -1;
        LastCheckPointDoor.Value = false;
        BossBlockingRock.Value = false;
    }
}
