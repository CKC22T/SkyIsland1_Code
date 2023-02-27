using Network.Common;
using Network.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;


namespace Network.Server
{
    public class DedicatedServerManager : LocalSingleton<DedicatedServerManager>
    {
        #region Test Code

        public ulong TotalNetworkReceivedBytes;
        public ulong TotalNetworkSentBytes;
        public ulong TotalNetworkBytes;

        public ulong bps_current_kb;

        [Sirenix.OdinInspector.Button]
        public void ForceDisconnectServer()
        {
            mServerNetworkService.StopServer();
        }

        //[Header("Server Address")]
        //public string TestHostIPAddress = "127.0.0.1";
        //public int TestHostPort = 50000;

        //[Sirenix.OdinInspector.Button(Name = "Start Server")]
        //public void Test_StartServer()
        //{
        //    TryStartServer(TestHostIPAddress, TestHostPort);
        //}

        //[Sirenix.OdinInspector.Button(Name = "Stop Server")]
        //public void Test_StopServer()
        //{
        //    StopServer();
        //}

        //[Sirenix.OdinInspector.Button(Name = "Kick Session")]
        //public void Test_KickSession()
        //{
        //    KickPlayer(KickSessionID);
        //}
        //public int KickSessionID = 0;

        //public string Message = "Message from server.";
        //[Sirenix.OdinInspector.Button(Name = "Send To All Message Via TCP")]
        //public void Test_SendToAllMessageViaTCP()
        //{
        //    mServerNetworkService.SendMessageToAllViaTCP(Message);
        //}

        //[Sirenix.OdinInspector.Button(Name = "Send To All Message Via UDP")]
        //public void Test_SendToAllMessageViaUDP()
        //{
        //    mServerNetworkService.SendMessageToAllViaUDP(Message);
        //}

        #endregion

        [SerializeField] private ServerSessionManager mServerSessionManager;
        [SerializeField] private bool mIsServerReadyToUpdateGame = false;

        public bool IsStarted => mServerNetworkService.IsStarted;
        private MasterServerNetworkService mServerNetworkService = new MasterServerNetworkService();
        private ulong mPacketID = 0;

        public event Action<int> OnSessionConnected;
        public event Action<int> OnSessionDisconnected;

        public void Start()
        {
            mServerSessionManager.InitializeByManager(this);

            Debug.Log(LogManager.GetLogMessage($"Server set framerate to {ServerConfiguration.SERVER_NETWORK_TICK}", NetworkLogType.MasterServer));

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = ServerConfiguration.SERVER_PHYSICS_TICK * 2;

            ServerWorldManager.Instance.OnServerStarted();

            mServerNetworkService.OnSessionConnected += (sessionID) => { mConnectedClientID.Enqueue(sessionID); };
            mServerNetworkService.OnSessionDisconnected += (sessionID) => { mDisconnectedClientID.Enqueue(sessionID); };
        }

        private void OnApplicationQuit()
        {
            StopServer();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopServer();
        }

        private Queue<int> mConnectedClientID = new();
        private Queue<int> mDisconnectedClientID = new();

        private int mRequestSessionId;
        private Request mRequestPacket;
        private Queue<(int SessionID, float Delay)> mKickQueue = new Queue<(int SessionID, float Delay)>();

        public void FixedUpdate()
        {
            // Handle packet
            while (mServerNetworkService.TryPopPacket(out mRequestSessionId, out mRequestPacket))
            {
                handleRequestPacket(mRequestSessionId, mRequestPacket);
            }

            // Handle kick
            while (!mKickQueue.IsEmpty())
            {
                var kickInfo = mKickQueue.Dequeue();

                StartCoroutine(disconnectSession(kickInfo.SessionID, kickInfo.Delay));
            }

            while (!mConnectedClientID.IsEmpty())
            {
                int connectedClientID = mConnectedClientID.Dequeue();
                OnSessionConnected(connectedClientID);
            }

            while (!mDisconnectedClientID.IsEmpty())
            {
                int DisconnectedClientID = mDisconnectedClientID.Dequeue();
                OnSessionDisconnected(DisconnectedClientID);
            }

            // Test code
            bps_current_kb = mServerNetworkService.bps_TotalLast / 1024;

            TotalNetworkReceivedBytes = mServerNetworkService.TotalNetworkReceivedBytes;
            TotalNetworkSentBytes = mServerNetworkService.TotalNetworkSentBytes;
            TotalNetworkBytes = mServerNetworkService.TotalNetworkBytes;
        }

        #region Handler

        private void handleRequestPacket(int sessionID, Request requestPacket)
        {
            Action<int, Request> handleProcess = requestPacket.Handle switch
            {
                RequestHandle.kRequestCommand => mServerSessionManager.HandleSessionCommand,

                // User input send
                RequestHandle.kUpdateInput => ServerHandle.UpdateInput,
                RequestHandle.kRequestWeaponEquipWeapon => ServerHandle.RequestWeaponEquipWeapon,

                // Inventory
                RequestHandle.kRequestItemObjectObtainData => ServerHandle.RequestItemObjectObtain,
                RequestHandle.kRequestItemObjectDropWeapon => ServerHandle.RequestItemObjectDropWeapon,
                RequestHandle.kRequestSwapWeapon => ServerHandle.RequestSwapWeapon,
                RequestHandle.kRequestTryObtainCheckPointItem => ServerHandle.RequestTryObtainCheckPointItem,

                _ => errorHandle
            };

            handleProcess.Invoke(sessionID, requestPacket);

            void errorHandle(int fromSession, Request requestPacket)
            {
                Debug.LogError(LogManager.GetLogMessage($"Cannot handle {requestPacket.Handle} packet from session : {fromSession}.", NetworkLogType.MasterServer, true));
            }
        }

        #endregion

        #region Server Operation
        public void TryStartServer(string hostIPAddress, int hostPort)
        {
            var startOperation = mServerNetworkService.StartServer(hostIPAddress, hostPort);

            if (startOperation == NetworkErrorCode.SUCCESS)
            {
                //StartCoroutine(initialize());
            }
            else
            {
                Debug.LogError(LogManager.GetLogMessage($"Server starting error! Error code : {startOperation}", NetworkLogType.MasterServer, true));
            }
        }

        public void StopServer()
        {
            if (mServerNetworkService.StopServer() == NetworkErrorCode.SUCCESS)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        public void KickPlayer(int sessionID, float delay)
        {
            mKickQueue.Enqueue((sessionID, delay));
        }

        private IEnumerator disconnectSession(int sessionID, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (mServerNetworkService.HasConnection(sessionID))
            {
                mServerNetworkService.ForceDisconnectSession(sessionID);
            }
        }

        #endregion

        #region Packet

        public Response.Builder GetBaseResponseBuilder(ResponseHandle responseHandle)
        {
            mPacketID++;
            return Response.CreateBuilder().SetHandle(responseHandle).SetPakcetId(mPacketID);
        }

        #endregion

        #region Sender

        // Sender via TCP
        public void SendToClient_TCP(in int sessionID, in NetBuffer data, in PrimitivePacketType packetType) => mServerNetworkService.SendToClient_TCP(sessionID, new NetBuffer(packetType, data));
        public void SendToClient_TCP(int sessionID, Response data) => mServerNetworkService.SendToClient_TCP(sessionID, new NetBuffer(data));
        public void SendToAllClient_TCP(in NetBuffer data, in PrimitivePacketType packetType) => mServerNetworkService.SendToAllClient_TCP(new NetBuffer(packetType, data));
        public void SendToAllClient_TCP(Response data) => mServerNetworkService.SendToAllClient_TCP(new NetBuffer(data));
        public void SendToAllClientExcept_TCP(in int exceptSessionID, in NetBuffer data, in PrimitivePacketType packetType) => mServerNetworkService.SendToAllClientExcept_TCP(exceptSessionID, new NetBuffer(packetType, data));
        public void SendToAllClientExcept_TCP(int exceptSessionID, Response data) => mServerNetworkService.SendToAllClientExcept_TCP(exceptSessionID, new NetBuffer(data));

        // Sender via UDP
        public void SendToClient_UDP(in int sessionID, in NetBuffer data, in PrimitivePacketType packetType) => mServerNetworkService.SendToClient_UDP(sessionID, new NetBuffer(packetType, data));
        public void SendToClient_UDP(int sessionID, Response data) => mServerNetworkService.SendToClient_UDP(sessionID, new NetBuffer(data));
        public void SendToAllClient_UDP(in NetBuffer data, in PrimitivePacketType packetType) => mServerNetworkService.SendToAllClient_UDP(new NetBuffer(packetType, data));
        public void SendToAllClient_UDP(Response data) => mServerNetworkService.SendToAllClient_UDP(new NetBuffer(data));
        public void SendToAllClientExcept_UDP(in int exceptSessionID, in NetBuffer data, in PrimitivePacketType packetType) => mServerNetworkService.SendToAllClientExcept_UDP(exceptSessionID, new NetBuffer(packetType, data));
        public void SendToAllClientExcept_UDP(int exceptSessionID, Response data) => mServerNetworkService.SendToAllClientExcept_UDP(exceptSessionID, new NetBuffer(data));

        #endregion
    }
}
