using Network.Packet;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Network
{
    public class MasterTcpServer
    {
        public ICollection<int> CurrentConnectedSession => mSessions.Keys;

        public bool IsAcceptStarted => mTcpAcceptor.IsAcceptStarted;
        private readonly MasterServerNetworkService mMasterServer;
        private readonly ITcpAcceptor mTcpAcceptor = new TcpNetwork();
        private readonly ConcurrentDictionary<int, ServerTcpSession> mSessions = new();
        public NetworkStatistics TcpStatistics { get; }  = new NetworkStatistics();

        public MasterTcpServer(MasterServerNetworkService masterServer)
        {
            mMasterServer = masterServer;
        }

        #region Server Operations

        public NetworkErrorCode Start(IPEndPoint hostIpEndpoint)
        {
            NetworkErrorCode operation = mTcpAcceptor.InitializeAcceptor(hostIpEndpoint);

            if (operation != NetworkErrorCode.SUCCESS)
            {
                return operation;
            }

            mTcpAcceptor.SetNetworkStatistics(TcpStatistics);

            mTcpAcceptor.OnAccepted += server_OnAccepted;

            mTcpAcceptor.OnAcceptStarting += () => { Debug.Log(LogManager.GetLogMessage($"Starting TCP server...", NetworkLogType.TcpServer)); };
            mTcpAcceptor.OnAcceptStarted += () => { Debug.Log(LogManager.GetLogMessage($"Started TCP server!", NetworkLogType.TcpServer)); };
            mTcpAcceptor.OnAcceptStopping += () => { Debug.Log(LogManager.GetLogMessage($"Stoping TCP server...", NetworkLogType.TcpServer)); };
            mTcpAcceptor.OnAcceptStopped += () => { Debug.Log(LogManager.GetLogMessage($"Stoped TCP server!", NetworkLogType.TcpServer)); };

            mTcpAcceptor.OnError += (e) => { Debug.LogWarning(LogManager.GetLogMessage($"Error occur : {e}", NetworkLogType.TcpServer, true)); };

            return mTcpAcceptor.StartAccept();
        }

        public NetworkErrorCode Stop()
        {
            lock (mSessions)
            {
                foreach (var session in mSessions.Values)
                {
                    session.ForceDisconnect();
                }
            }

            return mTcpAcceptor.StopAccept();
        }

        public bool ForceDisconnectSession(int sessionID)
        {
            lock (mSessions)
            {
                if (mSessions.TryRemove(sessionID, out var session))
                {
                    session.ForceDisconnect();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool HasConnection(int sessionID)
        {
            lock (mSessions)
            {
                return mSessions.ContainsKey(sessionID);
            }
        }

        #endregion

        #region Events

        private void server_OnAccepted(Socket connectedSessionSocket)
        {
            lock(mSessions)
            {
                foreach (var s in mSessions.Values)
                {
                    if (s.RemoteEndPoint == connectedSessionSocket.LocalEndPoint.ToString())
                    {
                        Debug.LogError(LogManager.GetLogMessage($"Duplicated connection request from session {s.SessionID} : {s.RemoteEndPoint}", NetworkLogType.TcpServer, true));
                        return;
                    }
                }
            }

            connectedSessionSocket.LocalEndPoint.ToString();

            ServerTcpSession session = new ServerTcpSession(mMasterServer.GetNewSessionID(), TcpStatistics);
            session.BindInternalCallback(tcpServer_OnReceived, tcpServer_OnConnectedInternal, tcpServer_OnDisconnectedInternal);

            // Session will be callback OnConnectedInternal if TCP connection is success.
            session.Connect(connectedSessionSocket);
        }

        private void tcpServer_OnConnectedInternal(ServerTcpSession session)
        {
            lock (mSessions)
            {
                // Check if it's validation connection.
                if (session.RemoteEndPoint == null)
                {
                    session.ForceDisconnect();
                    Debug.LogError(LogManager.GetLogMessage($"Already connected client {session.SessionID} request connection", NetworkLogType.TcpServer, true));
                    return;
                }

                // Check if it's already exsit client.
                foreach (var s in mSessions.Values)
                {
                    if (s.RemoteEndPoint == session.RemoteEndPoint)
                    {
                        Debug.LogError(LogManager.GetLogMessage($"Already connected client {session.SessionID} request connection from : {session.RemoteEndPoint}", NetworkLogType.TcpServer, true));
                        return;
                    }
                }

                // It's move to ServerSessionManager
                //// Check if server is full.
                //if (mSessions.Count >= ServerConfiguration.MAX_PLAYER)
                //{
                //    session.ForceDisconnect();
                //    Debug.Log(LogManager.GetLogMessage($"Server is full! Kick net connection client {session.SessionID}", NetworkLogType.TcpServer));
                //    return;
                //}

                // Add new session
                if (mSessions.TryAdd(session.SessionID, session))
                {
                    // Start UDP connect
                    mMasterServer.OnTcpConnectionSuccess(session);
                    Debug.Log(LogManager.GetLogMessage($"Client {session.SessionID} connected from : {session.RemoteEndPoint}", NetworkLogType.TcpServer));
                }
                else
                {
                    session.ForceDisconnect();
                    Debug.LogError(LogManager.GetLogMessage($"Already connected client {session.SessionID} request connection from : {session.RemoteEndPoint}", NetworkLogType.TcpServer, true));
                }
            }
        }

        private void tcpServer_OnReceived(int sessionID, NetBuffer data)
        {
            if (!data.IsValidPacketType())
                return;

            var packetType = data.ReadPrimitivePacketType();

            switch (packetType)
            {
                case PrimitivePacketType.REQUEST_CLIENT_PROTOBUF:
                    mMasterServer.PushPacket(sessionID, data.ReadRequest());
                    break;

                case PrimitivePacketType.REQUEST_CLIENT_MESSAGE:
                    Debug.Log(LogManager.GetLogMessage($"{data.ReadString()}", NetworkLogType.MasterServer));
                    break;

                case PrimitivePacketType.REQUEST_UDP_CONNECTION_COMPLETED:
                    int clientUdpPort = data.ReadInt32();

                    if (mSessions.ContainsKey(sessionID))
                    {
                        string ipAddress = mSessions[sessionID].RemoteIPAddress;
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), clientUdpPort);
                        mMasterServer.OnUdpConnectionSuccess(sessionID, endPoint);
                    }
                    else
                    {
                        ForceDisconnectSession(sessionID);
                    }
                    break;

                default:
                    break;
            }
        }

        private void tcpServer_OnDisconnectedInternal(int sessionID)
        {
            mMasterServer.ForceDisconnectSession(sessionID);
        }

        #endregion

        #region Sender

        public void SendToAll(NetBuffer data)
        {
            lock (mSessions)
            {
                foreach (var session in mSessions.Values)
                {
                    session.Send(data);
                }
            }
        }

        public void SendTo(int sessionID, NetBuffer data)
        {
            lock (mSessions)
            {
                if (mSessions.ContainsKey(sessionID))
                {
                    mSessions[sessionID].Send(data);
                }
            }
        }

        public void SendToAllExcept(int exceptSessionID, NetBuffer data)
        {
            lock (mSessions)
            {
                foreach (var session in mSessions.Values)
                {
                    if (session.SessionID != exceptSessionID)
                    {
                        session.Send(data);
                    }
                }
            }
        }

        #endregion
    }
}
