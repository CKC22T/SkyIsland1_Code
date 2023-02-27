using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Network
{
    public interface ITcpAcceptor
    {
        public event Action<SocketError> OnError;

        public bool IsSocketDisposed { get; }
        public bool IsAcceptStarted { get; }
        public bool IsAccepting { get; }

        public event Action<Socket> OnAccepted;
        public event Action OnAcceptStarted;
        public event Action OnAcceptStarting;
        public event Action OnAcceptStopped;
        public event Action OnAcceptStopping;

        public NetworkErrorCode InitializeAcceptor(IPEndPoint endpoint);
        public void SetNetworkStatistics(NetworkStatistics networkStatistics);
        public NetworkErrorCode StartAccept();
        public NetworkErrorCode StopAccept();
        public NetworkErrorCode RestartAccept();
    }

    public interface ITcpStreamable
    {
        public event Action<SocketError> OnError;

        public string RemoteIPAddress { get; }
        public string RemoteEndPoint { get; }
        public string LocalIPAddress { get; }
        public string LocalEndPoint { get; }
        public bool IsConnected { get; }
        public bool IsConnecting { get; }

        public event Action OnConnecting;
        public event Action OnConnected;

        public event Action OnDisconnected;
        public event Action OnDisconnecting;

        public event Action<NetBuffer> OnReceived;
        public event Action OnSended;

        public void SetNetworkStatistics(NetworkStatistics networkStatistics);
        /// <summary>전송할 TCP 데이터를 버퍼에 전달합니다.</summary>
        /// <returns>
        ///NOT_CONNECTED<br/>
        ///SUCCESS<br/>
        /// </returns>
        public NetworkErrorCode PushSendBuffer(in NetBuffer data);
        /// <summary>연결을 해제합니다.</summary>
        /// <returns>
        ///NOT_CONNECTED<br/>
        ///SUCCESS<br/>
        /// </returns>
        public NetworkErrorCode Disconnect();
    }

    public interface ITcpConnector : ITcpStreamable
    {
        /// <summary>연결을 시도합니다. Client측에서 Server로의 연결을 시도할 때 호출합니다.</summary>
        /// <returns>
        ///ALREADY_CONNECTED<br/>
        ///STILL_CONNECTING<br/>
        ///WRONG_SOCKET_OPERATION<br/>
        ///SOCKET_WAS_CREATED<br/>
        ///SUCCESS<br/>
        /// </returns>
        public NetworkErrorCode TryConnect(IPEndPoint endPoint);
    }

    public interface ITcpSession : ITcpStreamable
    {
        public event Action<TcpNetwork> OnConnectedInternal;
        public event Action<TcpNetwork> OnDisconnectedInternal;

        public NetworkErrorCode BindSocketAndConnect(Socket socket);
    }

    public class TcpNetwork : ITcpAcceptor, ITcpConnector, ITcpSession, IDisposable
    {
        private NetworkStatistics mNetworkStatistics;

        public void SetNetworkStatistics(NetworkStatistics networkStatistics)
        {
            mNetworkStatistics = networkStatistics;
        }

        public string LocalEndPoint => mTcpSocket?.LocalEndPoint.ToString();
        public string RemoteEndPoint => mTcpSocket?.RemoteEndPoint.ToString();
        private Socket mTcpSocket;
        private EndPoint mEndpoint;

        public string RemoteIPAddress
        {
            get
            {
                if (mTcpSocket == null)
                {
                    return null;
                }

                var endPoint = mTcpSocket.RemoteEndPoint as IPEndPoint;

                return endPoint.Address.ToString();
            }
        }

        public string LocalIPAddress
        {
            get
            {
                if (mEndpoint == null)
                {
                    return null;
                }

                var endPoint = mEndpoint as IPEndPoint;

                return endPoint.Address.ToString();
            }
        }

        public bool IsSocketDisposed { get; private set; } = true;

        private SocketAsyncEventArgs mAcceptorEventArgs;

        public event Action<SocketError> OnError = null;

        private Socket createSocket()
        {
            return new Socket(mEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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
                case SocketAsyncOperation.Accept:
                    processAccept(e);
                    break;

                case SocketAsyncOperation.Connect:
                    processConnect(e);
                    break;

                case SocketAsyncOperation.Receive:
                    if (processReceive(e))
                    {
                        tryReceive();
                    }
                    break;

                case SocketAsyncOperation.Send:
                    if (processSend(e))
                    {
                        trySend();
                    }
                    break;

                default:
                    throw new ArgumentException
                        (LogManager.GetLogMessage("The last operation was undefined behavior.", NetworkLogType.TcpNetworkCore, true));
            }
        }

        #region Acceptor

        public bool IsAcceptStarted { get; private set; } = false;
        public bool IsAccepting { get; private set; } = false;
        private int mAcceptBacklog = 1024;

        public event Action<Socket> OnAccepted = null;
        public event Action OnAcceptStarting = null;
        public event Action OnAcceptStarted = null;
        public event Action OnAcceptStopping = null;
        public event Action OnAcceptStopped = null;

        public NetworkErrorCode InitializeAcceptor(IPEndPoint endpoint)
        {
            if (IsAcceptStarted)
                return NetworkErrorCode.ALREADY_STARTED;

            mEndpoint = endpoint;
            return NetworkErrorCode.SUCCESS;
        }

        public NetworkErrorCode StartAccept()
        {
            if (IsAcceptStarted)
            {
                Debug.Log(LogManager.GetLogMessage("Accept is already started", NetworkLogType.TcpNetworkCore, true));
                return NetworkErrorCode.ALREADY_STARTED;
            }

            // Create socket
            mTcpSocket = createSocket();

            // Setup dispose
            IsSocketDisposed = false;

            // Setup socket
            mTcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
            mTcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
            mTcpSocket.NoDelay = true;

            // Bind endpoint
            mTcpSocket.Bind(mEndpoint);

            // Check IP Endpoint
            if (mEndpoint.ToString() != mTcpSocket.LocalEndPoint.ToString())
            {
                Debug.Log(LogManager.GetLogMessage("Accept listener binded different EndPoint!", NetworkLogType.TcpNetworkCore));
                Debug.Log(LogManager.GetLogMessage($"EndPoint changed {this.mEndpoint} to {mTcpSocket.LocalEndPoint}", NetworkLogType.TcpNetworkCore));
            }

            mEndpoint = mTcpSocket.LocalEndPoint;

            // Call OnAcceptStarting callback
            OnAcceptStarting?.Invoke();

            // Start Listen
            mTcpSocket.Listen(mAcceptBacklog);

            // Call OnAcceptStarted callback
            OnAcceptStarted?.Invoke();

            // Setup acceptor event arg
            mAcceptorEventArgs = new SocketAsyncEventArgs();
            mAcceptorEventArgs.Completed += onAsyncOperationCompleted;

            // Set it's started
            IsAcceptStarted = true;

            // Start accepting
            startAccept(mAcceptorEventArgs);

            return NetworkErrorCode.SUCCESS;
        }

        public NetworkErrorCode StopAccept()
        {
            if (!IsAcceptStarted)
            {
                Debug.Log(LogManager.GetLogMessage("Accept is not started!", NetworkLogType.TcpNetworkCore, true));
                return NetworkErrorCode.IS_NOT_STARTED;
            }

            // Stop accepting new clients
            IsAccepting = false;

            mAcceptorEventArgs.Completed -= onAsyncOperationCompleted;

            // Call OnServerStopping callback
            OnAcceptStopping?.Invoke();

            try
            {
                mTcpSocket.Close();
                mTcpSocket.Dispose();
                mTcpSocket = null;

                mAcceptorEventArgs.Dispose();
                mAcceptorEventArgs = null;

                IsSocketDisposed = true;
            }
            catch (ObjectDisposedException) { }

            // Call OnServerStopped callback
            OnAcceptStopped?.Invoke();

            // Update accept state
            IsAcceptStarted = false;

            return NetworkErrorCode.SUCCESS;
        }

        public NetworkErrorCode RestartAccept()
        {
            var operation = StopAccept();

            if (operation != NetworkErrorCode.SUCCESS)
            {
                return operation;
            }

            while (IsAcceptStarted)
            {
                Thread.Yield();
            }

            return StartAccept();
        }

        private void startAccept(SocketAsyncEventArgs e)
        {
            if (!IsAcceptStarted || IsAccepting)
            {
                return;
            }

            IsAccepting = true;
            e.AcceptSocket = null;

            if (!mTcpSocket.AcceptAsync(e))
            {
                // Process Accept synchronous
                processAccept(e);
            }
        }

        private void processAccept(SocketAsyncEventArgs e)
        {
            IsAccepting = false;

            if (e.SocketError == SocketError.Success)
            {
                if (IsAcceptStarted)
                {
                    OnAccepted(e.AcceptSocket);
                }
                else
                {
                    e.AcceptSocket.Shutdown(SocketShutdown.Both);
                }
            }
            else
            {
                announcement(e.SocketError);
            }

            // Accept the next client connection
            if (IsAcceptStarted)
            {
                startAccept(e);
            }
        }

        #endregion

        #region Connect

        public bool IsConnected { get; private set; } = false;
        public bool IsConnecting { get; private set; } = false;
        private SocketAsyncEventArgs mConnectEventArg;
        private SocketAsyncEventArgs mReceiveEventArg;
        private SocketAsyncEventArgs mSendEventArg;

        public event Action OnConnecting;
        public event Action OnConnected;

        public event Action OnDisconnected;
        public event Action OnDisconnecting;

        public event Action<TcpNetwork> OnConnectedInternal;
        public event Action<TcpNetwork> OnDisconnectedInternal;

        public event Action<NetBuffer> OnReceived;
        public event Action OnSended;

        /// <summary>이미 연결이 완료된 Socket을 바인딩합니다. Server측에서 Client로 부터 연결 요청이 들어온 Socket을 대상으로 호출합니다.</summary>
        /// <param name="socket">연결이 완료된 Socket</param>
        /// <returns>
        ///ALREADY_CONNECTED<br/>
        ///STILL_CONNECTING<br/>
        ///WRONG_SOCKET_OPERATION<br/>
        ///SOCKET_WAS_CREATED<br/>
        ///SOCKET_WAS_DISPOSED_WHILE_CONNECTING<br/>
        ///SUCCESS<br/>
        /// </returns>
        public NetworkErrorCode BindSocketAndConnect(Socket socket)
        {
            // Error handling
            if (IsConnected)
            {
                Debug.Log(LogManager.GetLogMessage("Socket is already connected", NetworkLogType.TcpNetworkCore, true));
                return NetworkErrorCode.ALREADY_CONNECTED;
            }

            if (IsConnecting)
            {
                Debug.Log(LogManager.GetLogMessage("Socket still trying to connect", NetworkLogType.TcpNetworkCore, true));
                return NetworkErrorCode.STILL_CONNECTING;
            }

            if (IsAcceptStarted)
            {
                Debug.Log(LogManager.GetLogMessage("This TcpNetwork is currently running as acceptor!", NetworkLogType.TcpNetworkCore, true));
                return NetworkErrorCode.WRONG_SOCKET_OPERATION;
            }

            if (!IsSocketDisposed)
            {
                Debug.Log(LogManager.GetLogMessage("Socket doesn't disposed!", NetworkLogType.TcpNetworkCore, true));
                return NetworkErrorCode.SOCKET_WAS_CREATED;
            }

            // Bind socket
            mTcpSocket = socket;

            // Update the Tcp
            IsSocketDisposed = false;

            // Setup event args
            mReceiveEventArg = new SocketAsyncEventArgs();
            mReceiveEventArg.Completed += onAsyncOperationCompleted;
            mSendEventArg = new SocketAsyncEventArgs();
            mSendEventArg.Completed += onAsyncOperationCompleted;

            // Apply the option
            mTcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
            mTcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

            // Call OnConnecting callback
            OnConnecting?.Invoke();

            // Update the connected flag
            IsConnecting = false;
            IsConnected = true;

            // Try to receive something from the client
            tryReceive();

            // Check the socket disposed state: in some rare cases it might be disconnected while receiving!
            if (IsSocketDisposed)
            {
                Debug.LogError(LogManager.GetLogMessage("Connect fail! the socket was disposed!", NetworkLogType.TcpNetworkCore, true));
                return NetworkErrorCode.SOCKET_WAS_DISPOSED_WHILE_CONNECTING;
            }

            // Call OnConnected callback
            OnConnected?.Invoke();

            // Callback current Tcp network to the other, like server
            OnConnectedInternal?.Invoke(this);

            return NetworkErrorCode.SUCCESS;
        }

        public NetworkErrorCode TryConnect(IPEndPoint hostEndPoint)
        {
            // Error handling
            if (IsConnected)
            {
                Debug.Log(LogManager.GetLogMessage("Socket is already connected", NetworkLogType.TcpNetworkCore, true));
                return NetworkErrorCode.ALREADY_CONNECTED;
            }

            if (IsConnecting)
            {
                Debug.Log(LogManager.GetLogMessage("Socket still trying to connect", NetworkLogType.TcpNetworkCore, true));
                return NetworkErrorCode.STILL_CONNECTING;
            }

            if (IsAcceptStarted)
            {
                Debug.Log(LogManager.GetLogMessage("This TcpNetwork is currently running as acceptor!", NetworkLogType.TcpNetworkCore, true));
                return NetworkErrorCode.WRONG_SOCKET_OPERATION;
            }

            if (!IsSocketDisposed)
            {
                Debug.Log(LogManager.GetLogMessage("Socket doesn't disposed!", NetworkLogType.TcpNetworkCore, true));
                return NetworkErrorCode.SOCKET_WAS_CREATED;
            }

            // Setup EndPoint
            mEndpoint = new IPEndPoint(IPAddress.Loopback, 0);

            // Setup event args
            mConnectEventArg = new SocketAsyncEventArgs();
            mConnectEventArg.RemoteEndPoint = hostEndPoint;
            mConnectEventArg.Completed += onAsyncOperationCompleted;

            mReceiveEventArg = new SocketAsyncEventArgs();
            mReceiveEventArg.Completed += onAsyncOperationCompleted;

            mSendEventArg = new SocketAsyncEventArgs();
            mSendEventArg.Completed += onAsyncOperationCompleted;

            // Create a new socket
            mTcpSocket = createSocket();

            // Update the client socket disposed flag
            IsSocketDisposed = false;

            // Apply the dual mode (this option must be applied before connecting)
            if (mTcpSocket.AddressFamily == AddressFamily.InterNetworkV6)
            {
                mTcpSocket.DualMode = true;
            }

            // Call the client connecting handler
            OnConnecting?.Invoke();

            // Update the connecting flag
            IsConnecting = true;

            if (!mTcpSocket.ConnectAsync(mConnectEventArg))
            { 
                processConnect(mConnectEventArg);
            }

            return NetworkErrorCode.SUCCESS;
        }

        private bool mIsDisconnecting = false;

        public NetworkErrorCode Disconnect()
        {
            if (mIsDisconnecting)
            {
                return NetworkErrorCode.STILL_DISCONNECTING;
            }

            mIsDisconnecting = true;

            // Call OnDisconnecting callback
            OnDisconnecting?.Invoke();

            // Cancel connecting operation
            if (IsConnecting)
            {
                mTcpSocket?.Close();
                mTcpSocket?.Dispose();
                IsSocketDisposed = true;
                IsConnecting = false;
                Socket.CancelConnectAsync(mConnectEventArg);
            }

            //// If it's not connected
            //if (IsConnected == false && IsConnecting == false)
            //{
            //    IsSocketDisposed = true;
            //    Socket.CancelConnectAsync(mConnectEventArg);
            //    mTcpSocket.Close();
            //    mTcpSocket.Dispose();

            //    mIsDisconnecting = false;
            //    return NetworkErrorCode.NOT_CONNECTED;
            //}


            // Reset event args
            if (mConnectEventArg != null)
                mConnectEventArg.Completed -= onAsyncOperationCompleted;

            if (mReceiveEventArg != null)
                mReceiveEventArg.Completed -= onAsyncOperationCompleted;

            if (mSendEventArg != null)
                mSendEventArg.Completed -= onAsyncOperationCompleted;


            // Try disconnected
            try
            {
                try
                {
                    // Shundown the socket associated with the client
                    mTcpSocket?.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException) { }

                mTcpSocket?.Close();
                mTcpSocket?.Dispose();

                mConnectEventArg?.Dispose();
                mReceiveEventArg?.Dispose();
                mSendEventArg?.Dispose();

                // Update the client socket dispose flag
                IsSocketDisposed = true;
            }
            catch (ObjectDisposedException) { }

            // Update the connected flag
            IsConnected = false;

            // Update sending/receiving flags
            mIsReceiving = false;
            mIsSending = false;

            // Callback current Tcp network to the other, like server
            OnDisconnectedInternal?.Invoke(this);

            // Call OnDisconnected callback
            OnDisconnected?.Invoke();

            // Clear buffers
            mSendBufferQueue.Clear();
            mTempSendBuffer.Clear();
            mProcessingReceiveBuffer.Clear();

            mIsDisconnecting = false;

            return NetworkErrorCode.SUCCESS;
        }

        /// <summary>연결을 시도합니다. Client측에서 비동기로 연결을 시도할 때 호출합니다.</summary>
        private void processConnect(SocketAsyncEventArgs e)
        {
            IsConnecting = false;

            if (e.SocketError == SocketError.Success)
            {
                // Apply socket options
                mTcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
                mTcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

                // Update the connected flag
                IsConnected = true;

                // Try to receive something
                tryReceive();

                // Check the socket disposed state: in some rare cases it might be disconnected while receiving!
                if (IsSocketDisposed)
                {
                    return;
                }

                // Call OnConnected callback
                OnConnected?.Invoke();
            }
            else
            {
                mTcpSocket?.Close();
                mTcpSocket?.Dispose();
                IsSocketDisposed = true;

                OnError?.Invoke(e.SocketError);
                OnDisconnected?.Invoke();
            }
        }

        #endregion

        #region Sender

        private bool mIsSending = false;
        private ConcurrentQueue<NetBuffer> mSendBufferQueue = new ConcurrentQueue<NetBuffer>();
        private NetBuffer mTempSendBuffer = new NetBuffer();

        public NetworkErrorCode PushSendBuffer(in NetBuffer data)
        {
            if (!IsConnected)
                return NetworkErrorCode.NOT_CONNECTED;

            mSendBufferQueue.Enqueue(data);
            trySend();
            return NetworkErrorCode.SUCCESS;
        }

        private void trySend()
        {
            if (!IsConnected)
            {
                Debug.LogError(LogManager.GetLogMessage("Send failed. The socket wasn't connected!", NetworkLogType.TcpNetworkCore, true));
                return;
            }

            if (mIsSending)
            {
                return;
            }

            mIsSending = true;

            while(mSendBufferQueue.TryDequeue(out var sendBuffer))
            {
                mTempSendBuffer.Append(sendBuffer);
            }

            if (mTempSendBuffer.Size <= 0)
            {
                mIsSending = false;
                return;
            }

            try
            {

                mSendEventArg.SetBuffer(mTempSendBuffer.Buffer, 0, mTempSendBuffer.Size);
                if (!mTcpSocket.SendAsync(mSendEventArg))
                {
                    processSend(mSendEventArg);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception e)
            {
                trySend();
            }
        }

        /// <summary>송신에 성공하면 'true'를, 실패하면 'false'를 반환합니다.</summary>
        private bool processSend(SocketAsyncEventArgs e)
        {
            mTempSendBuffer.Clear();

            mIsSending = false;

            if (!IsConnected)
            {
                Debug.LogError(LogManager.GetLogMessage("Send failed. The socket wasn't connected!", NetworkLogType.TcpNetworkCore, true));
                return false;
            }

            mNetworkStatistics.AddTotalSendBytes(e.BytesTransferred);

            OnSended?.Invoke();

            // If client is valid, try to send again
            if (e.SocketError == SocketError.Success)
            {
                return true;
            }
            else
            {
                OnError?.Invoke(e.SocketError);
                Disconnect();
                return false;
            }
        }

        #endregion

        #region Receive

        private bool mIsReceiving = false;
        private NetBuffer mReceiveBuffer = new NetBuffer(8196);

        private void tryReceive()
        {
            if (!IsConnected)
            {
                Debug.LogError(LogManager.GetLogMessage("Receive failed. The socket wasn't connected!", NetworkLogType.TcpNetworkCore, true));
                return;
            }

            if (mIsReceiving)
            {
                Debug.LogError(LogManager.GetLogMessage("Receive failed. It's still receiving!", NetworkLogType.TcpNetworkCore, true));
                return;
            }
            
            bool hasBufferLeft = true;

            while(hasBufferLeft)
            {
                hasBufferLeft = false;

                try
                {
                    mIsReceiving = true;
                    mReceiveEventArg.SetBuffer(mReceiveBuffer.Buffer, 0, mReceiveBuffer.Capacity);
                    if (!mTcpSocket.ReceiveAsync(mReceiveEventArg))
                    {
                        hasBufferLeft = processReceive(mReceiveEventArg);
                    }
                }
                catch (ObjectDisposedException) { }
            }
        }

        private NetBuffer mProcessingReceiveBuffer = new NetBuffer();

        private bool processReceive(SocketAsyncEventArgs e)
        {
            if (!IsConnected)
            {
                Debug.LogError(LogManager.GetLogMessage("Receive failed. The socket wasn't connected!", NetworkLogType.TcpNetworkCore, true));
                return false;
            }

            int receivedSize = e.BytesTransferred;

            mReceiveBuffer.ForceSetSize(receivedSize);

            if (e.BytesTransferred == mReceiveBuffer.Capacity)
            {
                mReceiveBuffer.Reserve(mReceiveBuffer.Capacity * 2);
            }

            if (receivedSize > 0)
            {
                mNetworkStatistics.AddTotalReceiveBytes(receivedSize);

                mProcessingReceiveBuffer.Append(mReceiveBuffer.ReadBytes(receivedSize));

                for (int i = 0; i < ServerConfiguration.MaxReceivedNetBufferCount; i++)
                {
                    if (mProcessingReceiveBuffer.CanReadNetBuffer())
                    {
                        var callbackData = mProcessingReceiveBuffer.ReadNetBuffer();
                        mProcessingReceiveBuffer.ShiftToFrontByReadIndex();
                        try
                        {
                            OnReceived?.Invoke(callbackData);
                        }
                        catch (Exception error)
                        {
                            Debug.LogError(LogManager.GetLogMessage($"Cannot handle message : {error}", NetworkLogType.TcpNetworkCore, true));
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                mReceiveBuffer.Clear();
            }

            mIsReceiving = false;

            if (e.SocketError == SocketError.Success)
            {
                if (receivedSize > 0)
                {
                    return true;
                }
                else
                {
                    Disconnect();
                    return false;
                }
            }
            else
            {
                OnError?.Invoke(e.SocketError);
                Disconnect();
            }

            return false;
        }

        #endregion

        private void announcement(SocketError error)
        {
            // Skip disconnect errors
            if ((error == SocketError.ConnectionAborted) ||
                (error == SocketError.ConnectionRefused) ||
                (error == SocketError.ConnectionReset) ||
                (error == SocketError.OperationAborted) ||
                (error == SocketError.Shutdown))
                return;

            OnError?.Invoke(error);
        }

        #region Dispose implementation

        /// <summary>Disposed flag</summary>
        public bool IsDisposed { get; private set; }

        // Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposingManagedResources)
        {
            // Dispose

            // The idea here is that Dispose(Boolean) knows whether it is
            // being called to do explicit cleanup (the Boolean is true)
            // versus being called due to a garbage collection (the Boolean
            // is false). This distinction is useful because, when being
            // disposed explicitly, the Dispose(Boolean) method can safely
            // execute code using reference type fields that refer to other
            // objects knowing for sure that these other objects have not been
            // finalized or disposed of yet. When the Boolean is false,
            // the Dispose(Boolean) method should not execute code that
            // refer to reference type fields because those objects may
            // have already been finalized."

            if (!IsDisposed)
            {
                if (disposingManagedResources)
                {
                    // Dispose managed resources

                    if (IsAcceptStarted)
                    {
                        StopAccept();
                    }

                    Disconnect();
                }

                // Dispose unmanaged resources

                // Set large fields to null here...

                // Mark as disposed.
                IsDisposed = true;
            }
        }

        #endregion
    }
}
