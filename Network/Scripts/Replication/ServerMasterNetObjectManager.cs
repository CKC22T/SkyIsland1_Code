using Network;
using Network.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class ServerMasterNetObjectManager : MonoSingleton<ServerMasterNetObjectManager>
{
    //[SerializeField] public List<MasterNetObject> PresetNetObjectList;
    [SerializeField] public List<Tuple<int, MasterNetObject>> NetObjectPresetByID;

    private MasterReplicationManager mReplicatorManager;

    private bool mIsInitialized = false;
    public void OnServerStarted()
    {
        mReplicatorManager = new MasterReplicationManager();

        foreach (var preset in NetObjectPresetByID)
        {
            preset.Item2.BindViaMasterManager(ForceCreateMasterReplicationObjectByID(preset.Item1));
        }

        mIsInitialized = true;
    }

    public MasterReplicationObject ForceCreateMasterReplicationObjectByID(int id)
    {
        return mReplicatorManager.ForceCreateMasterReplicationObjectByID(id);
    }

    public MasterReplicationObject CreateReplicatorObject()
    {
        return mReplicatorManager.CreateMasterReplicationObject();
    }

    //public void BindNewReplicator(MasterNetObject master)
    //{
    //    master.BindViaMasterManager(CreateReplicatorObject());
    //}

    public void SendInitialDataToClient(int sessionID)
    {
        List<NetBuffer> packetBuffer = new List<NetBuffer>();

        mReplicatorManager.AppendInitialDataPackets(ref packetBuffer);

        foreach (var packet in packetBuffer)
        {
            DedicatedServerManager.Instance.SendToClient_TCP(sessionID, packet, PrimitivePacketType.RESPONSE_GAME_FRAME_DATA);
        }
    }

    /// <summary>Reliable Data를 TCP 프로토콜로 전송합니다.</summary>
    public void SendReliableDataToAll()
    {
        List<NetBuffer> packetBuffer = new List<NetBuffer>();

        mReplicatorManager.AppendTcpPackets(ref packetBuffer);

        foreach (var packet in packetBuffer)
        {
            DedicatedServerManager.Instance.SendToAllClient_TCP(packet, PrimitivePacketType.RESPONSE_GAME_FRAME_DATA);
        }
    }

    /// <summary>Unreliable Data를 UDP 프로토콜로 전송합니다. UDP Heartbeat를 포함합니다.</summary>
    public void SendUnreliableDataToAll()
    {
        List<NetBuffer> packetBuffer = new List<NetBuffer>();

        mReplicatorManager.AppendUdpPackets(ref packetBuffer);

        foreach (var packet in packetBuffer)
        {
            DedicatedServerManager.Instance.SendToAllClient_UDP(packet, PrimitivePacketType.RESPONSE_GAME_FRAME_DATA);
        }
    }

    public void FixedUpdate()
    {
        if (mIsInitialized == false)
            return;

        SendReliableDataToAll();
        SendUnreliableDataToAll();
    }
}
