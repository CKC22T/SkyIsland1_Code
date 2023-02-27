using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Utils;

namespace Network.Server
{
    public enum ServerMode
    {
        NOT_STARTED = 0,
        USER_MODE = 1,
        WEB_CONNECTION_MODE = 2,
        SINGLE_MODE = 3,
    }

    public class ServerStarter : MonoSingleton<ServerStarter>
    {
        [SerializeField] private bool ForceStartByAddressFromEditor = false;
        [SerializeField] private string ForceInputServerIpAddress = "127.0.0.1";
        [SerializeField] private int ForceInputServerPort = ServerConfiguration.ServerInitialPortNumber;
        [SerializeField] private bool mIsTestMode = false;
        public bool IsTestMode => mIsTestMode;
        /// <summary>서버의 동작 모드 입니다.</summary>
        [Sirenix.OdinInspector.ShowInInspector] public ServerMode ServerMode { get; private set; } = ServerMode.NOT_STARTED;
        [Sirenix.OdinInspector.ShowInInspector] public IPAddress ServerIpAddress { get; private set; } = null;
        [Sirenix.OdinInspector.ShowInInspector] public string ServerIpString
        {
            get
            {
                if (ServerIpAddress == null)
                {
                    return "There is no IP address";
                }
                else
                {
                    return ServerIpAddress.ToString();
                }
            }
        }
            
        [Sirenix.OdinInspector.ShowInInspector] public int ServerPort { get; private set; }

        protected override void Awake()
        {
            startServerProcess();
        }

        /// <summary>Start server process</summary>
        private void startServerProcess()
        {
            ServerConfiguration.IS_SERVER = true;

            if (ForceStartByAddressFromEditor)
            {
                Debug.Log(LogManager.GetLogMessage($"Server force started at : {ForceInputServerIpAddress}:{ForceInputServerPort}", NetworkLogType.ServerStarter));
                ServerIpAddress = IPAddress.Parse(ForceInputServerIpAddress);
                ServerPort = ForceInputServerPort;
                startServerAsUserMode();
                return;
            }

            // Arguments from windows
            var arguments = Environment.GetCommandLineArgs();

            // Set host IP address
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ServerIpAddress = ip;
                    break;
                }
            }

            if (ServerIpAddress == null)
            {
                Debug.LogError(LogManager.GetLogMessage($"There is no internet IP address! Current IP {ServerIpString}", NetworkLogType.ServerStarter, true));
            }

            Debug.Log(LogManager.GetLogMessage($"Currnet server host IP address is : {ServerIpString}", NetworkLogType.ServerStarter));

            // Check input arguments
            if (arguments.Length != 3)
            {
                Debug.Log(LogManager.GetLogMessage($"Server start by user.", NetworkLogType.ServerStarter));
                startServerAsUserMode();
                return;
            }

            // Set inputs from environment caller
            string inputServerModeString = arguments[1];
            string inputServerPortString = arguments[2];

            // Check it's operation
            if (!int.TryParse(inputServerModeString, out int startOperation))
            {
                Debug.LogError(LogManager.GetLogMessage($"Server start operation parse error! Current input server start operation : {inputServerModeString}", NetworkLogType.ServerStarter, true));
                return;
            }

            // Check it's integer
            if (!int.TryParse(inputServerPortString, out int bindedHostPortOffset))
            {
                Debug.LogError(LogManager.GetLogMessage($"Port parse error! Current input port numer : {inputServerPortString}", NetworkLogType.ServerStarter, true));
                bindedHostPortOffset = 0;
                //return;
            }

            // Check port validation
            if (!NetworkExtension.IsValidPort(bindedHostPortOffset))
            {
                Debug.LogError(LogManager.GetLogMessage($"Invalid port number, you can't use port number {bindedHostPortOffset}", NetworkLogType.ServerStarter, true));
                return;
            }

            // Check server start mode option and start server by option
            if ((ServerMode)startOperation == ServerMode.WEB_CONNECTION_MODE)
            {
                ServerPort = ServerConfiguration.ServerInitialPortNumber + bindedHostPortOffset;
                startServerAsWebConnectionMode();
            }
            else if((ServerMode)startOperation == ServerMode.SINGLE_MODE)
            {
                ServerPort = bindedHostPortOffset;
                startServerAsSingleMode();
            }
            else if ((ServerMode)startOperation == ServerMode.USER_MODE)
            {
                ServerPort = bindedHostPortOffset;
                startServerAsUserMode();
            }
            else
            {
                Debug.LogError(LogManager.GetLogMessage($"Wrong start option! current option : {inputServerModeString}", NetworkLogType.ServerStarter, true));
                startServerAsUserMode();
                return;
            }

            // Add more branches if you wanna start some other mode...
        }

        /// <summary>Start server with offical web connection</summary>
        private void startServerAsWebConnectionMode()
        {
            StartCoroutine(startServer());

            IEnumerator startServer()
            {
                yield return new WaitForFixedUpdate();
                Debug.Log(LogManager.GetLogMessage("Server start as Web connection mode", NetworkLogType.ServerStarter));

                ServerMode = ServerMode.WEB_CONNECTION_MODE;

                WebServiceManager.Instance.TryStartServer(ServerIpString, ServerPort);
            }
        }

        /// <summary>Start server as user custom mode</summary>
        private void startServerAsUserMode()
        {
            StartCoroutine(startServer());

            IEnumerator startServer()
            {
                yield return new WaitForFixedUpdate();
                Debug.Log(LogManager.GetLogMessage("Server start as user mode", NetworkLogType.ServerStarter));

                //ServerPort = ForceInputServerPort;
                ServerMode = ServerMode.USER_MODE;

                DedicatedServerManager.Instance.TryStartServer(ServerIpString, ServerPort);

                //Test Code
                //yield return new WaitForSeconds(10.0f);
                //DedicatedServerManager.Instance.Test_ForceSpawnPlayer();
            }
        }

        private void startServerAsSingleMode()
        {
            StartCoroutine(startServer());

            IEnumerator startServer()
            {
                yield return new WaitForFixedUpdate();
                Debug.Log(LogManager.GetLogMessage("Server start as user mode", NetworkLogType.ServerStarter));

                //ServerPort = ServerConfiguration.ServerInitialPortNumber;
                ServerMode = ServerMode.USER_MODE;

                DedicatedServerManager.Instance.TryStartServer("127.0.0.1", ServerPort);

                //Test Code
                //yield return new WaitForSeconds(10.0f);
                //DedicatedServerManager.Instance.Test_ForceSpawnPlayer();
            }
        }
    }
}
