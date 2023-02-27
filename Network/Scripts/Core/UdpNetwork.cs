using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

namespace Network
{
    public interface IUdpServer
    {
        public int LocalPort { get; }

        public event Action OnStarting;
        public event Action OnStarted;
        public event Action OnStopping;
        public event Action OnStopped;

        public event Action<EndPoint, NetBuffer> OnReceived;
        public event Action OnSended;
        public event Action<SocketError> OnError;

        public bool IsStarted { get; }

        public void SetNetworkStatistics(NetworkStatistics networkStatistics);
        /// <summary>서버를 시작합니다.</summary>
        /// <param name="serverIpEndPoint">UDP 송수신을 위한 Server의 Endpoint 입니다. 기본값은 IPAddress.Any, Port = 0 입니다.</param>
        public NetworkErrorCode StartServer(IPEndPoint serverIpEndPoint = null);
        public NetworkErrorCode StopServer();
        public void SendAsync(EndPoint destEndpoint, NetBuffer data);
    }

    public interface IUdpSession
    {
        public int LocalPort { get; }

        public bool IsConnected { get; }

        public event Action OnConnecting;
        public event Action OnConnected;
        public event Action OnDisconnecting;
        public event Action OnDisconnected;

        public event Action<EndPoint, NetBuffer> OnReceived;
        public event Action OnSended;
        public event Action<SocketError> OnError;

        public void SetNetworkStatistics(NetworkStatistics networkStatistics);
        /// <summary>UDP 송수신을 위한 연결을 설정합니다.</summary>
        /// <param name="clientIpEndPoint">클라이언트의 IP EndPoint입니다. 기본값은 루프백 주소입니다.</param>
        /// <returns></returns>
        public NetworkErrorCode TryConnectSetup(IPEndPoint clientIpEndPoint = null);
        public NetworkErrorCode Disconnect();
        public void SendAsync(EndPoint destEndpoint, NetBuffer data);
    }

    public class UdpNetwork : IUdpServer, IUdpSession, IDisposable
    {
        public int LocalPort { get; private set; }

        private NetworkStatistics mNetworkStatistics;

        public void SetNetworkStatistics(NetworkStatistics networkStatistics)
        {
            mNetworkStatistics = networkStatistics;
        }

        //private EndPoint mMulticaseEndPoint;
        private Socket mUdpSocket;

        public event Action<SocketError> OnError = null;

        public bool IsSocketDisposed { get; private set; } = true;

        private Socket createSocket()
        {
            return new Socket(mEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        }

        private void onAsyncOperationCompleted(object sender, SocketAsyncEventArgs e)
        {
            // Async 구문이 비동기적으로 실행된 뒤 완료된 시점에서 호출된다.
            // 동기적으로 실행된 경우 Process를 별도로 실행한다.

            if (IsSocketDisposed)
            {
                return;
            }

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom:
                    processReceiveFrom(e);
                    break;

                case SocketAsyncOperation.SendTo:
                    processSendTo(e);
                    break;

                default:
                    throw new ArgumentException
                        (LogManager.GetLogMessage("The last operation was undefined behavior.", NetworkLogType.TcpNetworkCore, true));
            }
        }

        #region Server Side

        private EndPoint mEndpoint;
        public bool IsStarted { get; private set; }

        public event Action OnStarting = null;
        public event Action OnStarted = null;
        public event Action OnStopping = null;
        public event Action OnStopped = null;

        public NetworkErrorCode StartServer(IPEndPoint serverIpEndPoint = null)
        {
            if (IsStarted)
            {
                Debug.Log(LogManager.GetLogMessage("UDP connection is already started", NetworkLogType.UdpNetworkCore));
                return NetworkErrorCode.ALREADY_STARTED;
            }

            mEndpoint = serverIpEndPoint ?? new IPEndPoint(IPAddress.Any, 0);

            // Setup send receive event arg
            mSendEventArg = new SocketAsyncEventArgs();
            mSendEventArg.Completed += onAsyncOperationCompleted;
            mReceiveEventArg = new SocketAsyncEventArgs();
            mReceiveEventArg.Completed += onAsyncOperationCompleted;

            // Create a new server socket
            mUdpSocket = createSocket();

            // Update the socket disposed flag
            IsSocketDisposed = false;

            // Setup socket
            mUdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); // Server side
            mUdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false); // Server side

            if (mUdpSocket.AddressFamily == AddressFamily.InterNetworkV6)
                mUdpSocket.DualMode = true;

            // Bind the server socket to the endpoint
            mUdpSocket.Bind(mEndpoint);

            // When UDP socket binded, It will be bind random port by OS
            mEndpoint = mUdpSocket.LocalEndPoint;

            Debug.Log(LogManager.GetLogMessage($"UDP Binded : {mUdpSocket.LocalEndPoint}", NetworkLogType.UdpNetworkCore));

            // Reasign UDP port
            LocalPort = (mEndpoint as IPEndPoint).Port;

            // Call OnStarting callback
            OnStarting?.Invoke();

            // Set it's started
            IsStarted = true;

            // Prepare receive endpoint
            mReceiveEndpoint = new IPEndPoint((mEndpoint.AddressFamily == AddressFamily.InterNetworkV6) ? IPAddress.IPv6Any : IPAddress.Any, 0);

            // Start receiving UDP data from anywhere
            tryReceiving();

            // Call OnStarted callback
            OnStarted?.Invoke();

            return NetworkErrorCode.SUCCESS;
        }

        public NetworkErrorCode StopServer()
        {
            if (!IsStarted)
            {
                Debug.Log(LogManager.GetLogMessage("UDP server is not started!", NetworkLogType.UdpNetworkCore, true));
                return NetworkErrorCode.IS_NOT_STARTED;
            }

            // Reset event args
            mReceiveEventArg.Completed -= onAsyncOperationCompleted;
            mSendEventArg.Completed -= onAsyncOperationCompleted;

            // Call OnStopping callback
            OnStopping?.Invoke();

            try
            {
                // Close the server socket
                mUdpSocket.Close();

                // Dispose the server socket
                mUdpSocket.Dispose();

                // Dispose event arguments
                mReceiveEventArg.Dispose();
                mSendEventArg.Dispose();

                // Update the dispose flag
                IsSocketDisposed = true;
            }
            catch (ObjectDisposedException) { }

            // Update the started flag
            IsStarted = false;

            // Update sending/receiving flag
            mIsReceiving = false;

            // Call OnStopped callback
            OnStopped?.Invoke();

            return NetworkErrorCode.SUCCESS;
        }

        public NetworkErrorCode Restart()
        {
            var operation = StopServer();

            if (operation != NetworkErrorCode.SUCCESS)
            {
                return operation;
            }

            return StartServer();
        }

        #endregion

        #region Client Side

        public bool IsConnected { get; private set; }

        public event Action OnConnecting = null;
        public event Action OnConnected = null;
        public event Action OnDisconnecting = null;
        public event Action OnDisconnected = null;

        public NetworkErrorCode TryConnectSetup(IPEndPoint clientIpEndPoint = null)
        {
            if (IsConnected)
            {
                Debug.Log(LogManager.GetLogMessage("It's already connected", NetworkLogType.UdpNetworkCore, true));
                return NetworkErrorCode.ALREADY_CONNECTED;
            }

            mEndpoint = clientIpEndPoint ?? new IPEndPoint(IPAddress.Loopback, 40000);

            // Setup send receive event arg
            mSendEventArg = new SocketAsyncEventArgs();
            mSendEventArg.Completed += onAsyncOperationCompleted;
            mReceiveEventArg = new SocketAsyncEventArgs();
            mReceiveEventArg.Completed += onAsyncOperationCompleted;

            // Create a new server socket
            mUdpSocket = createSocket();

            // Update the client socket disposed flag
            IsSocketDisposed = false;

            // Apply the option: reuse address
            mUdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            // Apply the option: exclusive address use
            mUdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
            // Apply the option: dual mode (this option must be applied before recieving/sending)
            if (mUdpSocket.AddressFamily == AddressFamily.InterNetworkV6)
                mUdpSocket.DualMode = true;

            // Call the client connecting handler
            OnConnecting?.Invoke();

            try
            {
                //var endpoint = new IPEndPoint((mEndpoint.AddressFamily == AddressFamily.InterNetworkV6) ? IPAddress.IPv6Any : IPAddress.Any, 0);
                //mUdpSocket.Bind(endpoint);
                mUdpSocket.Bind(mEndpoint);

                mEndpoint = mUdpSocket.LocalEndPoint;

                // Reasign UDP port
                LocalPort = (mEndpoint as IPEndPoint).Port;

                Debug.Log(LogManager.GetLogMessage($"UDP Binded : {mUdpSocket.LocalEndPoint}", NetworkLogType.UdpNetworkCore));
                //mUdpSocket.Bind(mEndpoint); Use when I need to use multi
            }
            catch (SocketException e)
            {
                OnError?.Invoke(e.SocketErrorCode);

                // Call the client disconnecting handler
                OnDisconnecting?.Invoke();

                // Reset event args
                mReceiveEventArg.Completed -= onAsyncOperationCompleted;
                mSendEventArg.Completed -= onAsyncOperationCompleted;

                mUdpSocket.Close();

                mUdpSocket.Dispose();

                // Dispose event arguments
                mReceiveEventArg.Dispose();
                mSendEventArg.Dispose();

                // Call the client disconnected handler
                OnDisconnected?.Invoke();

                return NetworkErrorCode.SOCKET_BIND_FAIL;
            }

            // Prepare receive endpoint
            mReceiveEndpoint = new IPEndPoint((mEndpoint.AddressFamily == AddressFamily.InterNetworkV6) ? IPAddress.IPv6Any : IPAddress.Any, LocalPort);

            // Update connected flag
            IsConnected = true;

            // Set it's started
            IsStarted = true;

            // Try receiving
            tryReceiving();

            OnConnected?.Invoke();

            return NetworkErrorCode.SUCCESS;
        }

        public NetworkErrorCode Disconnect()
        {
            if (!IsConnected)
                return NetworkErrorCode.NOT_CONNECTED;

            // Call the client disconnecting handler
            OnDisconnecting?.Invoke();

            // Reset event args
            mReceiveEventArg.Completed -= onAsyncOperationCompleted;
            mSendEventArg.Completed -= onAsyncOperationCompleted;

            try
            {
                mUdpSocket.Close();

                mUdpSocket.Dispose();

                mReceiveEventArg.Dispose();
                mSendEventArg.Dispose();
            }
            catch (ObjectDisposedException) { }

            // Update the connected flag
            IsConnected = false;

            // Update sending/receiving flags
            mIsReceiving = false;

            // Dispose event arguments
            mReceiveEventArg.Dispose();
            mSendEventArg.Dispose();

            // Call the client disconnected handler
            OnDisconnected?.Invoke();

            return NetworkErrorCode.SUCCESS;
        }

        #endregion

        #region Sender

        public event Action OnSended = null;
        private SocketAsyncEventArgs mSendEventArg;

        //private ConcurrentQueue<(EndPoint sendTo, NetBuffer data)> mSendBufferQueue = new();
        //private NetBuffer mTempSendBuffer = new NetBuffer();

        public void SendAsync(EndPoint destEndpoint, NetBuffer data)
        {
            //mSendBufferQueue.Enqueue((destEndpoint, data));
            trySend(destEndpoint, data);
        }

        private void trySend(EndPoint destEndpoint, NetBuffer data)
        {
            if (!IsStarted && !IsConnected)
            {
                Debug.LogError(LogManager.GetLogMessage("Send failed. The socket wasn't connected!", NetworkLogType.UdpNetworkCore, true));
                return;
            }

            if (data == null)
            {
                return;
            }

            //while (mSendBufferQueue.TryDequeue(out var sendBuffer))
            //{
            //    mTempSendBuffer.Append(sendBuffer);
            //}

            //if (mTempSendBuffer.Size <= 0)
            //{
            //    return;
            //}

            NetBuffer testBuffer = new NetBuffer();
            testBuffer.Append(data);

            try
            {
                mSendEventArg.RemoteEndPoint = destEndpoint;
                //mSendEventArg.SetBuffer(mTempSendBuffer.Buffer, 0, mTempSendBuffer.Size);
                mSendEventArg.SetBuffer(testBuffer.Buffer, 0, testBuffer.Size);

                mUdpSocket.SendTo(testBuffer.Buffer, testBuffer.Size, SocketFlags.None, destEndpoint);
                //if (!mUdpSocket.SendToAsync(mSendEventArg))
                //{
                //    processSendTo(mSendEventArg);
                //}

                mNetworkStatistics.AddTotalSendBytes(testBuffer.Size);
            }
            catch (ObjectDisposedException) { }
        }

        private void processSendTo(SocketAsyncEventArgs e)
        {
            //mTempSendBuffer.Clear();

            if (!IsStarted && !IsConnected)
            {
                Debug.LogError(LogManager.GetLogMessage("Send failed. The socket wasn't connected!", NetworkLogType.UdpNetworkCore, true));
                return;
            }

            OnSended?.Invoke();

            // Check for error
            if (e.SocketError != SocketError.Success)
            {
                OnError?.Invoke(e.SocketError);

                return;
            }
        }

        #endregion

        #region Receiving

        public event Action<EndPoint, NetBuffer> OnReceived = null;
        private EndPoint mReceiveEndpoint;
        private SocketAsyncEventArgs mReceiveEventArg;
        private bool mIsReceiving;
        private NetBuffer mReceiveBuffer = new NetBuffer();

        /// <summary>UDP 데이터를 수신받습니다. 한 번만 수신 받습니다.</summary>
        private void tryReceiving()
        {
            if (mIsReceiving)
                return;

            if (!IsStarted && !IsConnected)
                return;

            try
            {
                // Async receive with the receive handler
                mIsReceiving = true;
                mReceiveEventArg.RemoteEndPoint = mReceiveEndpoint;
                mReceiveEventArg.SetBuffer(mReceiveBuffer.Buffer, 0, mReceiveBuffer.Capacity);
                if (!mUdpSocket.ReceiveFromAsync(mReceiveEventArg))
                {
                    processReceiveFrom(mReceiveEventArg);
                }
            }
            catch (ObjectDisposedException) { }
        }

        private void processReceiveFrom(SocketAsyncEventArgs e)
        {
            mIsReceiving = false;

            if (!IsStarted && !IsConnected)
                return;

            if (e.SocketError != SocketError.Success)
            {
                OnError?.Invoke(e.SocketError);

                // Restart receiving
                if (IsStarted)
                {
                    tryReceiving();
                }
                return;
            }

            int size = e.BytesTransferred;

            // Update statistic
            mNetworkStatistics.AddTotalReceiveBytes(size);

            mReceiveBuffer.ResetRead();
            NetBuffer receivedData = mReceiveBuffer.PopNetBuffer();

            try
            {
                OnReceived?.Invoke(e.RemoteEndPoint, receivedData);
            }
            catch (Exception error)
            {
                Debug.LogError(LogManager.GetLogMessage($"OnReceive Error : {error}", NetworkLogType.UdpNetworkCore, true));
            }

            // Restart receiving
            if (IsStarted)
            {
                tryReceiving();
            }
        }

        #endregion

        #region Dispose

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposingManagedResources)
        {
            if (!IsDisposed)
            {
                if (disposingManagedResources)
                {
                    Disconnect();
                    StopServer();
                }

                IsDisposed = true;
            }
        }

        #endregion
    }
}
