using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMasterNetObject : MasterNetObject
{
    // Reliable Data
    [Sirenix.OdinInspector.ShowInInspector]
    public readonly NetIntData IntData = new NetIntData(9997);
    [Sirenix.OdinInspector.ShowInInspector]
    public readonly NetIntData IntData2 = new NetIntData(1818);

    // Unreliable Data
    [Sirenix.OdinInspector.ShowInInspector]
    public readonly NetStringData StringData = new NetStringData("초기데이터");
    [Sirenix.OdinInspector.ShowInInspector]
    public readonly NetVector3Data Vector3Data = new NetVector3Data();

    public override void InitializeData(in MasterReplicationObject assignee)
    {
        assignee.AssignDataAsReliable(IntData);
        assignee.AssignDataAsReliable(IntData2);
        assignee.AssignDataAsUnreliable(StringData);
        assignee.AssignDataAsUnreliable(Vector3Data);
    }

    public Transform MasterTransform;

    [Sirenix.OdinInspector.Button]
    public void SetRandomValue()
    {
        IntData.Value = Random.RandomRange(0, 100);
        IntData2.Value = Random.RandomRange(0, 100);
        //StringData.Value = Random.RandomRange(10000, 9999999).ToString();
    }

    [Header("Test")]
    [SerializeField]
    private string StringDataForTest = "안녕하세요;";

    public void Update()
    {
        //Vector3Data.Value = MasterTransform.position;
        //StringData.Value = StringDataForTest;
    }
}
