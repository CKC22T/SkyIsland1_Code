using Network.Client;
using System;
using System.Net;
using UnityEngine;

namespace Network
{
    public class ClientTcpSession
    {
        private ITcpConnector mTcpConnector;

        public NetworkStatistics TcpStatistics { get; } = new NetworkStatistics();
        private MasterClientNetworkService mMasterClient;

        private event Action<int, int> mOnReceivedUdpPortAndSessionID;

        public event Action OnConnectionComplete;
        public bool IsConnected => mTcpConnector.IsConnected;

        public ClientTcpSession(MasterClientNetworkService masterClient, Action onConnected, Action onDisconnected, Action<int, int> onReceivedUdpPortAndSessionID)
        {
            mMasterClient = masterClient;

            mTcpConnector = new TcpNetwork();

            mTcpConnector.SetNetworkStatistics(TcpStatistics);

            mTcpConnector.OnError += (e) => { Debug.Log(LogManager.GetLogMessage($"Client {mMasterClient.SessionID} : On error [{e}]", NetworkLogType.TcpClient, true)); };

            mTcpConnector.OnConnecting += () => { Debug.Log(LogManager.GetLogMessage($"Client {mMasterClient.SessionID} : On connecting...", NetworkLogType.TcpClient)); };
            mTcpConnector.OnDisconnecting += () => { Debug.Log(LogManager.GetLogMessage($"Client {mMasterClient.SessionID} : On disconnecting...", NetworkLogType.TcpClient)); };

            //mTcpConnector.OnSended += () => { Debug.Log(LogManager.GetLogMessage($"Client {mMasterClient.SessionID} : On sended!", NetworkLogType.TcpClient)); };
            mTcpConnector.OnReceived += onTcpReceived;

            mTcpConnector.OnConnected += onConnected;
            mTcpConnector.OnDisconnected += onDisconnected;
            mOnReceivedUdpPortAndSessionID += onReceivedUdpPortAndSessionID;
        }

        private void onTcpReceived(NetBuffer data)
        {
            if (!data.IsValidPacketType())
                return;

            var packetType = data.ReadPrimitivePacketType();

            switch (packetType)
            {
                case PrimitivePacketType.RESPONSE_SERVER_PROTOBUF:
                    mMasterClient.PushPacket(data.ReadResponse());
                    break;

                case PrimitivePacketType.RESPONSE_SERVER_MESSAGE:
                    Debug.Log(LogManager.GetLogMessage($"{data.ReadString()}", NetworkLogType.MasterClient));
                    break;

                case PrimitivePacketType.RESPONSE_SERVER_UDP_PORT_AND_SESSION_ID: // Received server's UDP address
                    {
                        // Bind server's UDP address, and request UDP connect to server.
                        int sessionID = data.ReadInt32();
                        int port = data.ReadInt32();
                        mOnReceivedUdpPortAndSessionID.Invoke(sessionID, port);
                    }
                    break;

                case PrimitivePacketType.RESPONSE_CONNECT_COMPLETED:
                    {
                        OnConnectionComplete?.Invoke();
                    }
                    break;

                case PrimitivePacketType.RESPONSE_GAME_FRAME_DATA:
                    {
                        ClientHandler.UpdateGameFrame(ref data);
                    }
                    break;

                default:
                    Debug.Log(LogManager.GetLogMessage($"Invalid received packet! Packet type number : {(int)packetType}", NetworkLogType.MasterClient, true));
                    break;
            }
        }

        public NetworkErrorCode TryConnect(IPEndPoint hostEndPoint)
        {
            var operationResult = mTcpConnector.TryConnect(hostEndPoint);

            if (operationResult != NetworkErrorCode.SUCCESS)
            {
                Debug.Log(LogManager.GetLogMessage($"Connect failed! Error code : {operationResult}", NetworkLogType.TcpClient));
            }

            return operationResult;
        }

        public void SendToServer(NetBuffer data)
        {
            var operationResult = mTcpConnector.PushSendBuffer(data);

            if (operationResult != NetworkErrorCode.SUCCESS)
            {
                Debug.Log(LogManager.GetLogMessage($"Sending failed! Error code : {operationResult}", NetworkLogType.TcpClient));
            }
        }

        public void ForceDisconnect()
        {
            var operationResult = mTcpConnector.Disconnect();

            if (operationResult != NetworkErrorCode.SUCCESS)
            {
                Debug.Log(LogManager.GetLogMessage($"Disconnect failed! Client currently not connected: {operationResult}", NetworkLogType.TcpClient));
            }
        }
    }
}
