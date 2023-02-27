using Network.Client;
using System;
using System.Net;
using UnityEngine;

namespace Network
{
    public class ClientUdpSession
    {
        public int RemotePort => mHostUdpPort;
        public int LocalPort => mUdpClient.LocalPort;

        private IUdpSession mUdpClient;

        public NetworkStatistics UdpStatistics { get; } = new NetworkStatistics();
        private MasterClientNetworkService mMasterClient;

        private Action mOnReceivedUdpConnectionChecked;

        private string mHostIpAddress;
        private int mHostUdpPort;
        private EndPoint mHostUdpEndPoint;

        public bool IsUdpConnectionChecked { get; private set; } = false;

        public ClientUdpSession(MasterClientNetworkService masterClient, Action onConnected, Action onDisconnected, Action onReceivedUdpConnectionChecked)
        {
            mMasterClient = masterClient;
            mHostIpAddress = mMasterClient.HostIpAddress;

            mUdpClient = new UdpNetwork();

            mUdpClient.SetNetworkStatistics(UdpStatistics);

            mUdpClient.OnError += (e) => { Debug.Log(LogManager.GetLogMessage($"Client {mMasterClient.SessionID} : On error [{e}]", NetworkLogType.UdpClient, true)); };

            mUdpClient.OnConnecting += () => { Debug.Log(LogManager.GetLogMessage($"Client {mMasterClient.SessionID} : On connecting...", NetworkLogType.UdpClient)); };
            mUdpClient.OnDisconnecting += () => { Debug.Log(LogManager.GetLogMessage($"Client {mMasterClient.SessionID} : On disconnecting...", NetworkLogType.UdpClient)); };

            mUdpClient.OnReceived += onUdpReceived;

            mUdpClient.OnConnected += onConnected;
            mUdpClient.OnDisconnected += onDisconnected;
            mOnReceivedUdpConnectionChecked += onReceivedUdpConnectionChecked;
        }

        public void ConnectSetup(IPEndPoint clientUdpEndPoint)
        {
            mUdpClient.TryConnectSetup(clientUdpEndPoint);
        }

        public void BindHostIPAddress(string hostIpAddress, int hostUdpPort)
        {
            mHostIpAddress = hostIpAddress;
            mHostUdpPort = hostUdpPort;
            mHostUdpEndPoint = new IPEndPoint(IPAddress.Parse(mHostIpAddress), mHostUdpPort);
        }

        public void SendToServer(NetBuffer data)
        {
            if (IsUdpConnectionChecked)
            {
                mUdpClient.SendAsync(mHostUdpEndPoint, data);
            }
        }

        public void ForceSendToServer(NetBuffer data)
        {
            mUdpClient.SendAsync(mHostUdpEndPoint, data);
        }

        private void onUdpReceived(EndPoint from, NetBuffer data)
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
                    Debug.Log(LogManager.GetLogMessage($"{data.ReadString()}", NetworkLogType.UdpClient));
                    break;

                case PrimitivePacketType.RESPONSE_UDP_CONNECTION_CHECKED:

                    Debug.Log(LogManager.GetLogMessage($"Server UDP connection check from : {from}", NetworkLogType.UdpClient));

                    if (IsUdpConnectionChecked)
                    {
                        break;
                    }

                    IsUdpConnectionChecked = true;
                    mOnReceivedUdpConnectionChecked.Invoke();
                    break;

                case PrimitivePacketType.RESPONSE_GAME_FRAME_DATA:
                    {
                        ClientHandler.UpdateGameFrame(ref data);
                    }
                    break;

                default:
                    break;
            }
        }

        public NetworkErrorCode ForceDisconnect()
        {
            var operationResult = mUdpClient.Disconnect();

            if (operationResult != NetworkErrorCode.SUCCESS)
            {
                Debug.Log(LogManager.GetLogMessage($"Disconnect failed! Client currently not connected: {operationResult}", NetworkLogType.UdpClient));
            }

            IsUdpConnectionChecked = false;

            return operationResult;
        }
    }
}
