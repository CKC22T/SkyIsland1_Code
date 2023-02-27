using Network;
using Network.Client;
using Network.Packet;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

public class ClientNetworkManager : MonoSingleton<ClientNetworkManager>
{
    #region Test Code

    public string ServerHostIP = "127.0.0.1";
    public int ServerHostPort = 50000;

    [Sirenix.OdinInspector.Button(Name = "Bind Random Client Port")]
    public void Test_BindRandomPort()
    {
        ClientUdpPort = Random.Range(51000, 60000);
    }

    [Sirenix.OdinInspector.Button(Name = "SetClientIP")]
    public void Test_SetClientIP()
    {
        NetworkExtension.TryGetLocalIPAddressViaConnection(out ClientIPAddress);
    }

    public int ClientUdpPort = 40000;
    public string ClientIPAddress = "127.0.0.1";

    [Sirenix.OdinInspector.Button(Name = "Connect To Server")]
    public void Test_ConnectToServer()
    {
        var operation = TryConnectToServer(ServerHostIP, ServerHostPort, ClientIPAddress, ClientUdpPort);

        switch (operation)
        {
            case NetworkErrorCode.WRONG_IP_ADDRESS:
                Debug.Log(LogManager.GetLogMessage($"Wrong IP address : {ServerHostIP}", NetworkLogType.MasterClient, true));
                break;

            case NetworkErrorCode.WRONG_PORT:
                Debug.Log(LogManager.GetLogMessage($"Wrong Port : {ServerHostPort}", NetworkLogType.MasterClient, true));
                break;

            case NetworkErrorCode.WRONG_IP_ADDRESS_AND_PORT:
                Debug.Log(LogManager.GetLogMessage($"Wrong IP address and port, IP : {ServerHostIP}, Port : {ServerHostPort}", NetworkLogType.MasterClient, true));
                break;
        }
    }

    [Sirenix.OdinInspector.Button(Name = "Disconnect From Server")]
    public void Test_ForceDisconnect()
    {
        ForceDisconnect();
    }

    public string Message = "Message from client.";
    [Sirenix.OdinInspector.Button(Name = "Send Message via TCP")]
    public void Test_SendMessageViaTCP()
    {
        mClientNetworkService.SendMessageToServerViaTCP($"[Sent from : {mClientNetworkService.SessionID} via TCP] {Message}");
    }

    [Sirenix.OdinInspector.Button(Name = "Send Message via UDP")]
    public void Test_SendMessageViaUDP()
    {
        mClientNetworkService.SendMessageToServerViaUDP($"[Sent from : {mClientNetworkService.SessionID} via UDP] {Message}");
    }

    //[Sirenix.OdinInspector.Button]
    //public void Test_RequestInitialDataToServer()
    //{
    //    RequestInitialDataToServer();
    //}

    [Sirenix.OdinInspector.Button]
    public void Test_ClearWorldManager()
    {
        ClientWorldManager.Instance?.Clear();
    }

    public ulong udp_sentByte = 0;
    public ulong udp_receivedByte = 0;

    #endregion

    [SerializeField] private ClientSessionManager mClientSessionManager;

    private MasterClientNetworkService  mClientNetworkService;

    public bool IsConnected => mClientNetworkService.IsConnected;
    private ulong mPacketID = 0;

    private void Start()
    {
        mClientNetworkService = new MasterClientNetworkService(onConnectionCompleted);
        mClientNetworkService.OnSessionDisconnected += onSessionDisconnected;

        mClientSessionManager.InitializeByManager(this);
    }

    //protected override void Initialize()
    //{
    //}

    private Response mPacket;
    public void Update()
    {
        if (mOnSessionConnected != null)
        {
            mOnSessionConnected.Invoke(mClientNetworkService.SessionID);
            mOnSessionConnected = null;
        }

        if (mOnSessionDisconnected != null)
        {
            mOnSessionDisconnected.Invoke();
            mOnSessionDisconnected = null;
        }

        while (mClientNetworkService.TryPopPacket(out mPacket))
        {
            handleResponsePacket(mPacket);
        }

        udp_sentByte = mClientNetworkService.TotalUdpSentBytes;
        udp_receivedByte = mClientNetworkService.TotalUdpReceivedBytes;
    }

    #region Events

    public event Action<int> OnSessionConnected;
    private Action<int> mOnSessionConnected;
    public event Action OnSessionDisconnected;
    private Action mOnSessionDisconnected;

    private object mStateLock = new object();

    private void onSessionDisconnected()
    {
        mOnSessionDisconnected = null;
        mOnSessionDisconnected += OnSessionDisconnected;
    }

    private void onSessionConnected()
    {
        mOnSessionConnected = null;
        mOnSessionConnected += OnSessionConnected;
    }

    #endregion

    #region Handler

    private void handleResponsePacket(Response responsePacket)
    {
        //Debug.Log(LogManager.GetLogMessage($"Handle packet : {responsePacket.Handle}", NetworkLogType.MasterClient));

        Action<Response> handleProcess = responsePacket.Handle switch
        {
            ResponseHandle.kServerClosed => ClientHandler.ServerClosed,

            ResponseHandle.kResponseCommand => mClientSessionManager.HandleSessionCommand,

            // Update Entities
            ResponseHandle.kUpdateEntitySpawnData => ClientHandler.UpdateEntitySpawnData,
            ResponseHandle.kUpdateEntityStatesData => ClientHandler.UpdateEntityStatesData,
            ResponseHandle.kUpdateEntityTransformData => ClientHandler.UpdateEntityTransformData,

            // Update Entity Action Handling
            ResponseHandle.kUpdateEntityActionData => ClientHandler.UpdateEntityActionData,

            // Detector Action Handling
            ResponseHandle.kUpdateDetectorActionData => ClientHandler.UpdateDetectorActionData,

            // Update Locator
            ResponseHandle.kUpdateLocatorStateData => ClientHandler.UpdateLocatorStateData,
            ResponseHandle.kUpdateLocatorActionData => ClientHandler.UpdateLocatorActionData,

            // Update Item Object
            ResponseHandle.kUpdateItemObjectStateData => ClientHandler.UpdateItemObjectStateData,
            ResponseHandle.kUpdateItemObjectActionData => ClientHandler.UpdateItemObjectActionData,

            // Remote Call Cinema
            ResponseHandle.kRemotePlayCinema => ClientHandler.RemoteCallCinema,

            ResponseHandle.kResponseError => ClientHandler.ErrorListener,
            _ => errorHandle
        };

        handleProcess.Invoke(responsePacket);

        void errorHandle(Response responsePacket)
        {
            Debug.LogError(LogManager.GetLogMessage($"Cannot handle {responsePacket.Handle} packet from server.", NetworkLogType.MasterClient, true));
        }
    }

    #endregion

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        ForceDisconnect();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ForceDisconnect();
    }

    #region Client Operation

    public NetworkErrorCode TryConnectToServer(EndPoint hostServerEndPoint, EndPoint clientEndPoint)
    {
        return mClientNetworkService.TryConnect(hostServerEndPoint, clientEndPoint);
    }

    public NetworkErrorCode TryConnectToServer(string serverIP, int serverPort, string clientIP, int clientPort)
    {
        return mClientNetworkService.TryConnect(serverIP, serverPort, clientIP, clientPort);
    }

    public void ForceDisconnect()
    {
        mClientNetworkService?.ForceDisconnect();
    }

    private void onConnectionCompleted()
    {
        // Connection failed
        if (mClientNetworkService.IsConnected == false)
        {
            ForceDisconnect();
            return;
        }

        // Call action
        onSessionConnected();

        Debug.Log(LogManager.GetLogMessage("Client connect to server Success!", NetworkLogType.MasterClient));
    }

    #endregion

    #region Packet

    public Request.Builder GetRequestBuilder(RequestHandle requestHandle)
    {
        mPacketID++;
        var userToken = UserTokenData.CreateBuilder().SetUserId(mClientNetworkService.SessionID);
        return Request.CreateBuilder()
            .SetHandle(requestHandle)
            .SetPakcetId(mPacketID)
            .SetUserToken(userToken);
    }

    #endregion

    #region Sender

    public void SendPrimitiveActionToServer(PrimitivePacketType actionType)
    {
        mClientNetworkService.ForceSendToServerViaTCP(new NetBuffer(actionType));
    }

    public void SendToServerViaTcp(Request data)
    {
        mClientNetworkService.SendToServerViaTCP(data);
    }

    public void SendToServerViaUdp(Request data)
    {
        mClientNetworkService.SendToServerViaUDP(data);
    }

    #endregion
}
