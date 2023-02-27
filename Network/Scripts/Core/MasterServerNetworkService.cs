using Network.Packet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Network
{
    public class MasterServerNetworkService
    {
        public string HostIpAddress { get; private set; } = "127.0.0.1";
        public int HostPort { get; private set; } = 50000;

        public event Action<int> OnSessionDisconnected;
        public event Action<int> OnSessionConnected;

        public bool IsStarted => (mTcpServer.IsAcceptStarted && mUdpServer.IsStarted);

        private MasterTcpServer mTcpServer;
        private MasterUdpServer mUdpServer;

        private int mSessionIdNumber = 0;
        public int GetNewSessionID() => mSessionIdNumber++;

        private ConcurrentStack<Request> mPacketStack = new();
        private ConcurrentStack<int> mSessionIdStack = new();

        public ICollection<int> CurrentConnectedSession => mTcpServer.CurrentConnectedSession;

        public MasterServerNetworkService()
        {
            mTcpServer = new MasterTcpServer(this);
            mUdpServer = new MasterUdpServer(this);
        }

        public bool TryPopPacket(out int sessionID, out Request packet)
        {
            bool isPoppedPacket = mPacketStack.TryPop(out packet);
            bool isPoppedSession = mSessionIdStack.TryPop(out sessionID);
            return isPoppedPacket && isPoppedSession;
        }

        public void PushPacket(int sessionID, Request packet)
        {
            mPacketStack.Push(packet);
            mSessionIdStack.Push(sessionID);
        }

        public NetworkErrorCode StartServer(string hostIpAddress, int hostPortPort)
        {
            HostIpAddress = hostIpAddress;
            HostPort = hostPortPort;

            var operationTcpServer = mTcpServer.Start(new IPEndPoint(IPAddress.Parse(HostIpAddress), HostPort));
            var operationUdpServer = mUdpServer.Start(new IPEndPoint(IPAddress.Any, HostPort));

            if (operationTcpServer == NetworkErrorCode.SUCCESS && operationUdpServer == NetworkErrorCode.SUCCESS)
            {
                return NetworkErrorCode.SUCCESS;
            }
            else
            {
                if (operationTcpServer != operationUdpServer)
                {
                    Debug.Log(LogManager.GetLogMessage($"Server start error!\nTCP server error code : {operationTcpServer}\nUDP server error code : {operationUdpServer}", NetworkLogType.MasterServer, true));
                }
                else
                {
                    Debug.Log(LogManager.GetLogMessage($"Server start error!\nError code : {operationTcpServer}"));
                }
            }

            return operationTcpServer;
        }

        public NetworkErrorCode StopServer()
        {
            var operationTcpServer = mTcpServer.Stop();
            var operationUdpServer = mUdpServer.Stop();

            if (operationTcpServer == NetworkErrorCode.SUCCESS && operationUdpServer == NetworkErrorCode.SUCCESS)
            {
                return NetworkErrorCode.SUCCESS;
            }
            else
            {
                if (operationTcpServer != operationUdpServer)
                {
                    Debug.Log(LogManager.GetLogMessage($"Server stop error!\nTCP server error code : {operationTcpServer}\nUDP server error code : {operationUdpServer}", NetworkLogType.MasterServer, true));
                }
                else
                {
                    Debug.Log(LogManager.GetLogMessage($"Server stop error!\nError code : {operationTcpServer}", NetworkLogType.MasterServer, true));
                }
            }

            return operationTcpServer;
        }

        public bool ForceDisconnectSession(int sessionID)
        {
            // 아마 Session의 연결이 종료되면 TCP UDP 모두 두번 호출할듯, 테스트 필요
            bool operationTcp = mTcpServer.ForceDisconnectSession(sessionID);
            bool operationUdp = mUdpServer.ForceDisconnectSession(sessionID);

            if (operationTcp)
            {
                Debug.Log(LogManager.GetLogMessage($"Session {sessionID} : Force TCP disconnected!", NetworkLogType.MasterServer));
            }
            else
            {
                Debug.Log(LogManager.GetLogMessage($"TCP Force Disconnect error! There is no session {sessionID}!", NetworkLogType.MasterServer, true));
                return false;
            }

            if (operationUdp)
            {
                Debug.Log(LogManager.GetLogMessage($"Session {sessionID} : Force UDP disconnected!", NetworkLogType.MasterServer));
            }
            else
            {
                Debug.Log(LogManager.GetLogMessage($"UDP Force Disconnect error! There is no session {sessionID}!", NetworkLogType.MasterServer, true));
                return false;
            }

            OnSessionDisconnected?.Invoke(sessionID);

            return true;
        }

        public bool HasConnection(int sessionID)
        {
            return mTcpServer.HasConnection(sessionID) && mUdpServer.HasConnection(sessionID);
        }

        #region Sender

        public void SendToClient_TCP(int sessionID, in NetBuffer data) => mTcpServer.SendTo(sessionID, data);
        public void SendToAllClient_TCP(in NetBuffer data) => mTcpServer.SendToAll(data);
        public void SendToAllClientExcept_TCP(int exceptSessionID, in NetBuffer data) => mTcpServer.SendToAllExcept(exceptSessionID, data);

        public void SendToClient_UDP(int sessionID, in NetBuffer data) => mUdpServer.SendToClient_UDP(sessionID, data);
        public void SendToAllClient_UDP(in NetBuffer data) => mUdpServer.SendToAllClient_UDP(data);
        public void SendToAllClientExcept_UDP(int exceptSessionID, in NetBuffer data) => mUdpServer.SendToAllClientExcept_UDP(exceptSessionID, data);

        public void SendMessageToAllViaTCP(string message)
        {
            var data = new NetBuffer(PrimitivePacketType.RESPONSE_SERVER_MESSAGE);
            data.Append(message);
            mTcpServer.SendToAll(data);
        }

        public void SendMessageToAllViaUDP(string message)
        {
            var data = new NetBuffer(PrimitivePacketType.RESPONSE_SERVER_MESSAGE);
            data.Append(message);
            mUdpServer.ForceSendToAll(data);
        }

        #endregion

        #region Session connection handling

        // Send server udp port and session id to client via TCP
        public void OnTcpConnectionSuccess(ServerTcpSession session)
        {
            NetBuffer serverUdpPortAndSessionID = new NetBuffer(PrimitivePacketType.RESPONSE_SERVER_UDP_PORT_AND_SESSION_ID);
            serverUdpPortAndSessionID.Append(session.SessionID);
            serverUdpPortAndSessionID.Append(mUdpServer.LocalPort);
            session.Send(serverUdpPortAndSessionID);
        }

        public void OnUdpConnectionSuccess(int sessionID, EndPoint clientUdpEndPoint)
        {
            Debug.Log(LogManager.GetLogMessage($"UDP connect success session {sessionID}. EndPoint : {clientUdpEndPoint}", NetworkLogType.MasterServer));
            mUdpServer.BindUdpConnection(sessionID, clientUdpEndPoint);

            NetBuffer connectCompletedPacket = new NetBuffer(PrimitivePacketType.RESPONSE_CONNECT_COMPLETED);
            mTcpServer.SendTo(sessionID, connectCompletedPacket);

            OnSessionConnected?.Invoke(sessionID);
        }

        #endregion

        #region Statistics

        // TCP Statistics
        public ulong bps_CurrentTcp => mTcpServer.TcpStatistics.Current_bps;
        public ulong bps_CurrentTcpSent => mTcpServer.TcpStatistics.CurrentSent_bps;
        public ulong bps_CurrentTcpReceived => mTcpServer.TcpStatistics.CurrentRecieved_bps;

        public ulong bps_LastTCP => mTcpServer.TcpStatistics.LastTotal_bps;
        public ulong bps_LastTcpSent => mTcpServer.TcpStatistics.LastSent_bps;
        public ulong bps_LastTcpReceived => mTcpServer.TcpStatistics.LastReceived_bps;

        public ulong TotalTcpBytes => mTcpServer.TcpStatistics.TotalBytes;
        public ulong TotalTcpSentBytes => mTcpServer.TcpStatistics.TotalSentBytes;
        public ulong TotalTcpReceivedBytes => mTcpServer.TcpStatistics.TotalReceivedBytes;

        // UDP Statistics
        public ulong bps_CurrentUDP => mUdpServer.UdpStatistics.Current_bps;
        public ulong bps_CurrentUdpSent => mUdpServer.UdpStatistics.CurrentSent_bps;
        public ulong bps_CurrentUdpReceived => mUdpServer.UdpStatistics.CurrentRecieved_bps;

        public ulong bps_LastUDP => mUdpServer.UdpStatistics.LastTotal_bps;
        public ulong bps_LastUdpSent => mUdpServer.UdpStatistics.LastSent_bps;
        public ulong bps_LastUdpReceived => mUdpServer.UdpStatistics.LastReceived_bps;

        public ulong TotalUdpBytes => mUdpServer.UdpStatistics.TotalBytes;
        public ulong TotalUdpSentBytes => mUdpServer.UdpStatistics.TotalSentBytes;
        public ulong TotalUdpReceivedBytes => mUdpServer.UdpStatistics.TotalReceivedBytes;

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
