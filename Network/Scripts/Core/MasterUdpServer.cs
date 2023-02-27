using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace Network
{
    public class MasterUdpServer
    {
        public int LocalPort => mUdpServer.LocalPort;

        public bool IsStarted => mUdpServer.IsStarted;
        private readonly MasterServerNetworkService mMasterServer;
        private readonly IUdpServer mUdpServer = new UdpNetwork();
        private readonly ServerUdpSessionTable mUdpSessionTable = new ServerUdpSessionTable();

        public NetworkStatistics UdpStatistics { get; } = new NetworkStatistics();

        public MasterUdpServer(MasterServerNetworkService masterServer)
        {
            mMasterServer = masterServer;
        }

        #region Server Operations

        public NetworkErrorCode Start(IPEndPoint serverHostEndPoint)
        {
            if (mUdpServer.IsStarted)
            {
                return NetworkErrorCode.ALREADY_STARTED;
            }

            mUdpServer.SetNetworkStatistics(UdpStatistics);

            mUdpServer.OnReceived += server_OnReceived;

            mUdpServer.OnStarting += () => { Debug.Log(LogManager.GetLogMessage($"Starting UDP server...", NetworkLogType.UdpServer)); };
            mUdpServer.OnStarted += () => { Debug.Log(LogManager.GetLogMessage($"Started UDP server!", NetworkLogType.UdpServer)); };
            mUdpServer.OnStopping += () => { Debug.Log(LogManager.GetLogMessage($"Stopping UDP server...", NetworkLogType.UdpServer)); };
            mUdpServer.OnStopped += () => { Debug.Log(LogManager.GetLogMessage($"Stoped UDP server!", NetworkLogType.UdpServer)); };

            mUdpServer.OnError += (e) => { Debug.LogWarning(LogManager.GetLogMessage($"Error occur : {e}", NetworkLogType.UdpServer, true)); };

            return mUdpServer.StartServer(serverHostEndPoint);
        }

        public NetworkErrorCode Stop()
        {
            var operationResult = mUdpServer.StopServer();

            if (operationResult == NetworkErrorCode.SUCCESS)
            {
                mUdpSessionTable.Clear();
            }

            return operationResult;
        }

        public void BindUdpConnection(int sessionID, EndPoint udpSessionAddress)
        {
            mUdpSessionTable.Add(sessionID, udpSessionAddress);
        }

        public bool ForceDisconnectSession(int sessionID)
        {
            return mUdpSessionTable.TryRemove(sessionID);
        }

        public bool HasConnection(int sessionID)
        {
            return mUdpSessionTable.HasSession(sessionID);
        }

        #endregion

        #region Sender

        public void SendToClient_UDP(int sessionID, NetBuffer data)
        {
            if (mUdpSessionTable.TryFindEndPointByID(sessionID, out var to))
            {
                mUdpServer.SendAsync(to, data);
            }
        }

        public void SendToAllClient_UDP(NetBuffer data)
        {
            foreach (var sessionID in mUdpSessionTable.SessionIDs)
            {
                if (mUdpSessionTable.TryFindEndPointByID(sessionID, out var endPoint))
                {
                    mUdpServer.SendAsync(endPoint, data);
                }
            }
        }

        public void SendToAllClientExcept_UDP(int exceptSessionID, NetBuffer data)
        {
            foreach (var sessionID in mUdpSessionTable.SessionIDs)
            {
                if (exceptSessionID == sessionID)
                {
                    continue;
                }

                if (mUdpSessionTable.TryFindEndPointByID(sessionID, out var endPoint))
                {
                    mUdpServer.SendAsync(endPoint, data);
                }
            }
        }

        public void ForceSendToAll(NetBuffer data)
        {
            foreach (var endPoint in mUdpSessionTable.SessionEndPoints)
            {
                mUdpServer.SendAsync(endPoint, data);
            }
        }

        public void ForceSendTo(EndPoint to, NetBuffer data)
        {
            mUdpServer.SendAsync(to, data);
        }

        #endregion

        #region Events

        private void server_OnReceived(EndPoint from, NetBuffer data)
        {
            if (!data.IsValidPacketType())
                return;

            var packetType = data.ReadPrimitivePacketType();

            switch (packetType)
            {
                case PrimitivePacketType.REQUEST_CLIENT_PROTOBUF:
                    var receivedData = data.ReadRequest();
                    mMasterServer.PushPacket(receivedData.UserToken.UserId, receivedData);
                    break;

                case PrimitivePacketType.REQUEST_CLIENT_MESSAGE:
                    Debug.Log(LogManager.GetLogMessage($"[EndPoint : {from}] {data.ReadString()}", NetworkLogType.MasterServer));
                    break;

                case PrimitivePacketType.REQUEST_UDP_CONNECTION_CHECK:
                    {
                        Debug.Log(LogManager.GetLogMessage($"UDP connection request from : {from}", NetworkLogType.UdpServer));

                        Task connectionCheck = Task.Run(() =>
                        {
                            NetBuffer udpCheckedPacket = new NetBuffer(PrimitivePacketType.RESPONSE_UDP_CONNECTION_CHECKED);

                            int repeatTime = ServerConfiguration.UdpCheckTimeout / ServerConfiguration.UdpCheckDelay;

                            for (int i = 0; i < repeatTime; i++)
                            {
                                Task.Delay(ServerConfiguration.UdpCheckDelay).Wait();

                                if (mUdpSessionTable.TryFindSessionIdByEndPoint(from, out int id))
                                {
                                    break;
                                }

                                ForceSendTo(from, udpCheckedPacket);
                            }
                        });
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion
    }
}
