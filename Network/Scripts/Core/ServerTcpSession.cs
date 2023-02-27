using System;
using System.Net.Sockets;
using UnityEngine;

namespace Network
{
    public class ServerTcpSession
    {
        public int SessionID { get; private set; } = -1;

        public string LocalEndPoint
        {
            get
            {
                if (mTcpSession == null || mTcpSession.LocalEndPoint == null)
                {
                    return null;
                }

                return mTcpSession.LocalEndPoint.ToString();
            }
        }
        public string RemoteEndPoint
        {
            get
            {
                if (mTcpSession == null || mTcpSession.RemoteEndPoint == null)
                {
                    return null;
                }

                return mTcpSession.RemoteEndPoint.ToString();
            }
        }
        public string LocalIPAddress => mTcpSession.LocalIPAddress;
        public string RemoteIPAddress => mTcpSession.RemoteIPAddress;

        private ITcpSession mTcpSession;

        private Action<int, NetBuffer> mOnReceivedInternalCallback;
        private Action<ServerTcpSession> mOnConnectedInternalCallback;
        private Action<int> mOnDisconnectedInternalCallback;

        public ServerTcpSession(int sessionID, NetworkStatistics tcpStatistics)
        {
            SessionID = sessionID;

            mTcpSession = new TcpNetwork();

            mTcpSession.SetNetworkStatistics(tcpStatistics);

            mTcpSession.OnError += (e) => { Debug.Log(LogManager.GetLogMessage($"Client {SessionID} : On error [{e}]", NetworkLogType.TcpServer, true)); };

            mTcpSession.OnConnecting += () => { Debug.Log(LogManager.GetLogMessage($"Client {SessionID} : On connecting...", NetworkLogType.TcpServer)); };
            mTcpSession.OnConnected += () => { Debug.Log(LogManager.GetLogMessage($"Client {SessionID} : On connected!", NetworkLogType.TcpServer)); };
            mTcpSession.OnDisconnecting += () => { Debug.Log(LogManager.GetLogMessage($"Client {SessionID} : On disconnecting...", NetworkLogType.TcpServer)); };
            mTcpSession.OnDisconnected += () => { Debug.Log(LogManager.GetLogMessage($"Client {SessionID} : On disconnected!", NetworkLogType.TcpServer)); };

            mTcpSession.OnDisconnectedInternal += OnDisconnectedInternal;
            mTcpSession.OnConnectedInternal += OnConnectedInternal;
            mTcpSession.OnReceived += OnRecieved;
        }

        public void BindInternalCallback(Action<int, NetBuffer> onReceived, Action<ServerTcpSession> onConnected, Action<int> onDisconnected)
        {
            mOnReceivedInternalCallback = onReceived;
            mOnConnectedInternalCallback = onConnected;
            mOnDisconnectedInternalCallback =  onDisconnected;
        }

        public void Connect(Socket socket)
        {
            mTcpSession.BindSocketAndConnect(socket);
        }

        public void ForceDisconnect()
        {
            mTcpSession.Disconnect();
        }

        public void Send(in NetBuffer data)
        {
            mTcpSession.PushSendBuffer(data);
        }

        private void OnConnectedInternal(TcpNetwork obj) => mOnConnectedInternalCallback.Invoke(this);

        private void OnRecieved(NetBuffer data)
        {
            mOnReceivedInternalCallback.Invoke(SessionID, data);
        }

        private void OnDisconnectedInternal(TcpNetwork network) => mOnDisconnectedInternalCallback(SessionID);
    }
}
