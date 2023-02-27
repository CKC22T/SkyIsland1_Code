using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRemoteNetObject : RemoteNetObject
{
    // Reliable Data
    [Sirenix.OdinInspector.ShowInInspector]
    public readonly NetIntData IntData = new NetIntData();
    [Sirenix.OdinInspector.ShowInInspector]
    public readonly NetIntData IntData2 = new NetIntData();

    // Unreliable Data
    [Sirenix.OdinInspector.ShowInInspector]
    public readonly NetStringData StringData = new NetStringData();
    [Sirenix.OdinInspector.ShowInInspector]
    public readonly NetVector3Data Vector3Data = new NetVector3Data();

    public override void InitializeData(in RemoteReplicationObject assignee)
    {
        assignee.AssignDataAsReliable(IntData);
        assignee.AssignDataAsReliable(IntData2);
        assignee.AssignDataAsUnreliable(StringData);
        assignee.AssignDataAsUnreliable(Vector3Data);
    }

    public Transform RemoteTransform;

    public void Update()
    {
        RemoteTransform.position = Vector3Data.Value;
    }
}
