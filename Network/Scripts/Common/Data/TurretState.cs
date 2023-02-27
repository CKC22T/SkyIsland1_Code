using Network;
using System;
using System.Collections.Generic;

public class TurretState : INetworkAssignable
{
    public List<NetBooleanData> IsDetected;

    public void InitializeDataAsRemote(in RemoteReplicationObject assignee)
    {
        IsDetected = new List<NetBooleanData>();
        for (int i = 0; i < ServerConfiguration.TurretCount; i++)
        {
            var turret = new NetBooleanData(false);
            IsDetected.Add(turret);
            assignee.AssignDataAsReliable(turret);
        }
    }

    public void InitializeDataAsMaster(in MasterReplicationObject assignee)
    {
        IsDetected = new List<NetBooleanData>();
        for (int i = 0; i < ServerConfiguration.TurretCount; i++)
        {
            var turret = new NetBooleanData(false);
            IsDetected.Add(turret);
            assignee.AssignDataAsReliable(turret);
        }
    }

    public void ResetData()
    {
        foreach (var i in IsDetected)
        {
            i.Value = false;
        }
    }

    public bool TryGetTurretState(int index, out bool isDetected)
    {
        if (index < IsDetected.Count && index >= 0)
        {
            isDetected = IsDetected[index].Value;
            return true;
        }

        isDetected = false;
        return false;
    }

    public bool TrySetTurretState(int index, bool isDetected)
    {
        if (index < IsDetected.Count && index >= 0)
        {
            IsDetected[index].Value = isDetected;
            return true;
        }

        return false;
    }
}
