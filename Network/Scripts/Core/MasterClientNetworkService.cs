using Network.Packet;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

namespace Network
{
    public class MasterClientNetworkService
    {
        public string HostIpAddress { get; private set; } = "127.0.0.1";
        public int HostPort { get; private set; } = 50000;

        public bool IsConnected { get; private set; } = false;

        public int SessionID { get; private set; } = 0;

        private ClientTcpSession mTcpClient;
        private ClientUdpSession mUdpClient;

        private readonly ConcurrentStack<Response> mPacketStack = new();

        public event Action OnSessionConnected;
        public event Action OnSessionDisconnected;

        /// <summary>유닛 테스트용 코드</summary>
        public event Action<MasterClientNetworkService> OnTCPConnected;

        /// <summary>유닛 테스트용 코드 </summary>
        public MasterClientNetworkService(Action<MasterClientNetworkService> onConnectionCompleted)
        {
            //mTcpClient = new ClientTcpSession(this, onConnected, onTcpDisconnected, onReceivedUdpPortAndSessionID);
            //mUdpClient = new ClientUdpSession(this, onConnected, onUdpDisconnected, onReceivedUdpConnectionChecked);

            mTcpClient = new ClientTcpSession(this, onConnected, ForceDisconnect, onReceivedUdpPortAndSessionID);
            mUdpClient = new ClientUdpSession(this, onConnected, ForceDisconnect, onReceivedUdpConnectionChecked);

            OnTCPConnected += onConnectionCompleted;
            mTcpClient.OnConnectionComplete += OnConnectionComplete;
        }
        
        public MasterClientNetworkService(Action onConnectionCompleted)
        {
            //mTcpClient = new ClientTcpSession(this, onConnected, onTcpDisconnected, onReceivedUdpPortAndSessionID);
            //mUdpClient = new ClientUdpSession(this, onConnected, onUdpDisconnected, onReceivedUdpConnectionChecked);

            mTcpClient = new ClientTcpSession(this, onConnected, ForceDisconnect, onReceivedUdpPortAndSessionID);
            mUdpClient = new ClientUdpSession(this, onConnected, ForceDisconnect, onReceivedUdpConnectionChecked);

            mTcpClient.OnConnectionComplete += onConnectionCompleted;
        }

        public NetworkErrorCode TryConnect(string hostIPAddress, int hostPort, string clientIP, int clientPort)
        {
            bool wrongIPAddress = false;
            bool wrongPort = false;

            if (!hostIPAddress.TryParseIPAddressFromString(out var hostAddress))
            {
                wrongIPAddress = true;
            }
            if (!clientIP.TryParseIPAddressFromString(out var clientAddress))
            {
                wrongIPAddress = true;
            }

            wrongPort |= !NetworkExtension.IsValidPort(hostPort);
            wrongPort |= !NetworkExtension.IsValidPort(clientPort);

            if (wrongIPAddress && wrongPort)
            {
                return NetworkErrorCode.WRONG_IP_ADDRESS_AND_PORT;
            }
            else if (wrongIPAddress)
            {
                return NetworkErrorCode.WRONG_IP_ADDRESS;
            }
            else if (wrongPort)
            {
                return NetworkErrorCode.WRONG_PORT;
            }

            HostIpAddress = hostIPAddress;
            HostPort = hostPort;

            //mUdpClient.ConnectSetup(new IPEndPoint(IPAddress.Loopback, clientPort));

            IPEndPoint hostIPEndPoint = new IPEndPoint(hostAddress, hostPort);
            IPEndPoint clientEndPoint = new IPEndPoint(clientAddress, clientPort);

            return TryConnect(hostIPEndPoint, clientEndPoint);
        }

        public NetworkErrorCode TryConnect(EndPoint hostIPEndPoint, EndPoint clientIPEndPoint)
        {
            if (clientIPEndPoint is IPEndPoint && hostIPEndPoint is IPEndPoint)
            {
                mUdpClient.ConnectSetup(clientIPEndPoint as IPEndPoint);
                return mTcpClient.TryConnect(hostIPEndPoint as IPEndPoint);
            }
            else
            {
                return NetworkErrorCode.WRONG_IP_ADDRESS_AND_PORT;
            }
        }

        #region Sender

        public void ForceSendToServerViaTCP(NetBuffer data)
        {
            if (mTcpClient.IsConnected)
            {
                mTcpClient.SendToServer(data);
            }
            else
            {
                Debug.Log(LogManager.GetLogMessage($"Sending TCP data failed! You are not connected!", NetworkLogType.MasterClient, true));
            }
        }

        public void SendToServerViaTCP(Request data)
        {
            if (mTcpClient.IsConnected)
            {
                mTcpClient.SendToServer(new NetBuffer(data));
            }
            else
            {
                Debug.Log(LogManager.GetLogMessage($"Sending TCP data failed! You are not connected!", NetworkLogType.MasterClient, true));
            }
        }

        public void SendToServerViaUDP(Request data)
        {
            //if (mServerUdpEndPoint == null)
            //{
            //    Debug.Log(LogManager.GetLogMessage($"Sending UDP data failed! You don't know server's UDP address!", NetworkLogType.MasterClient, true));
            //}

            mUdpClient.SendToServer(new NetBuffer(data));
        }

        public void SendMessageToServerViaTCP(string message)
        {
            var data = new NetBuffer(PrimitivePacketType.REQUEST_CLIENT_MESSAGE);
            data.Append(message);
            mTcpClient.SendToServer(data);
        }

        public void SendMessageToServerViaUDP(string message)
        {
            var data = new NetBuffer(PrimitivePacketType.REQUEST_CLIENT_MESSAGE);
            data.Append(message);
            mUdpClient.SendToServer(data);
        }

        #endregion

        #region Events

        /// <summary>유닛 테스트용 코드</summary>
        private void OnConnectionComplete()
        {
            OnTCPConnected?.Invoke(this);
        }

        public void ForceDisconnect()
        {
            mPacketStack.Clear();
            mTcpClient.ForceDisconnect();
            mUdpClient.ForceDisconnect();
            OnSessionDisconnected?.Invoke();
        }

        public void PushPacket(Response responsePacket)
        {
            mPacketStack.Push(responsePacket);
        }

        public bool TryPopPacket(out Response packet)
        {
            return mPacketStack.TryPop(out packet);
        }

        private void onConnected()
        {
            Debug.Log(LogManager.GetLogMessage($"Client {SessionID} : On connected...", NetworkLogType.MasterClient));
        }

        //private void onTcpDisconnected()
        //{
        //    Debug.Log(LogManager.GetLogMessage($"Client TCP disconnected!", NetworkLogType.MasterClient));
        //    mPacketStack.Clear();
        //    mUdpClient.ForceDisconnect();
        //    IsConnected = false;
        //    OnSessionDisconnected?.Invoke();
        //}

        //private void onUdpDisconnected()
        //{
        //    Debug.Log(LogManager.GetLogMessage($"Client UDP disconnected!", NetworkLogType.MasterClient));
        //    mPacketStack.Clear();
        //    mTcpClient.ForceDisconnect();
        //    IsConnected = false;
        //    OnSessionDisconnected?.Invoke();
        //}

        private void onReceivedUdpPortAndSessionID(int sessionID, int udpPort)
        {
            SessionID = sessionID;

            mUdpClient.BindHostIPAddress(HostIpAddress, udpPort);

            Task sendRequestToServer = Task.Run(() =>
            {
                int repeatTime = ServerConfiguration.UdpCheckTimeout / ServerConfiguration.UdpCheckDelay;

                NetBuffer packet = new NetBuffer(PrimitivePacketType.REQUEST_UDP_CONNECTION_CHECK);
                packet.Append(sessionID);

                for (int i = 0; i < repeatTime; i++)
                {
                    mUdpClient.ForceSendToServer(packet);

                    Task.Delay(ServerConfiguration.UdpCheckDelay).Wait();

                    if (mUdpClient.IsUdpConnectionChecked)
                    {
                        break;
                    }
                }

                if (mUdpClient.IsUdpConnectionChecked == false)
                {
                    // UDP connection failed, force disconnect.
                    ForceDisconnect();
                }
            });
        }

        private void onReceivedUdpConnectionChecked()
        {
            NetBuffer udpConnectionCompletedPacket = new NetBuffer(PrimitivePacketType.REQUEST_UDP_CONNECTION_COMPLETED);
            udpConnectionCompletedPacket.Append(mUdpClient.LocalPort);
            mTcpClient.SendToServer(udpConnectionCompletedPacket);

            // Connection complete
            IsConnected = true;
            OnSessionConnected?.Invoke();

            Debug.Log(LogManager.GetLogMessage($"Client {SessionID} : UDP connect success!", NetworkLogType.MasterClient));
        }

        #endregion

        #region Statistics

        // TCP Statistics
        public ulong bps_CurrentTcp => mTcpClient.TcpStatistics.Current_bps;
        public ulong bps_CurrentTcpSent => mTcpClient.TcpStatistics.CurrentSent_bps;
        public ulong bps_CurrentTcpReceived => mTcpClient.TcpStatistics.CurrentRecieved_bps;

        public ulong bps_LastTCP => mTcpClient.TcpStatistics.LastTotal_bps;
        public ulong bps_LastTcpSent => mTcpClient.TcpStatistics.LastSent_bps;
        public ulong bps_LastTcpReceived => mTcpClient.TcpStatistics.LastReceived_bps;

        public ulong TotalTcpBytes => mTcpClient.TcpStatistics.TotalBytes;
        public ulong TotalTcpSentBytes => mTcpClient.TcpStatistics.TotalSentBytes;
        public ulong TotalTcpReceivedBytes => mTcpClient.TcpStatistics.TotalReceivedBytes;

        // UDP Statistics
        public ulong bps_CurrentUDP => mUdpClient.UdpStatistics.Current_bps;
        public ulong bps_CurrentUdpSent => mUdpClient.UdpStatistics.CurrentSent_bps;
        public ulong bps_CurrentUdpReceived => mUdpClient.UdpStatistics.CurrentRecieved_bps;

        public ulong bps_LastUDP => mUdpClient.UdpStatistics.LastTotal_bps;
        public ulong bps_LastUdpSent => mUdpClient.UdpStatistics.LastSent_bps;
        public ulong bps_LastUdpReceived => mUdpClient.UdpStatistics.LastReceived_bps;

        public ulong TotalUdpBytes => mUdpClient.UdpStatistics.TotalBytes;
        public ulong TotalUdpSentBytes => mUdpClient.UdpStatistics.TotalSentBytes;
        public ulong TotalUdpReceivedBytes => mUdpClient.UdpStatistics.TotalReceivedBytes;

        // Total Statistics
        public ulong bps_TotalCurrent => bps_CurrentTcp + bps_CurrentUDP;
        public ulong bps_TotalCurrentSent => bps_CurrentTcpSent + bps_CurrentUdpSent;
        public ulong bps_TotalCurrentReceived => bps_CurrentTcpReceived + bps_CurrentUdpReceived;

        public ulong bps_TotalLast => bps_LastTCP + bps_LastUDP;
        public ulong bps_TotalLastSent => bps_LastTcpSent + bps_LastUdpSent;
        public ulong bps_TotalLastReceived => bps_LastTcpReceived + bps_LastUdpReceived;

        public ulong TotalNetworkBytes => TotalNetworkReceivedBytes + TotalNetworkSentBytes;
        public ulong TotalNetworkSentBytes => TotalTcpSentBytes + TotalUdpSentBytes;
        public ulong TotalNetworkReceivedBytes => TotalTcpReceivedBytes + TotalUdpReceivedBytes;

        #endregion
    }
}
