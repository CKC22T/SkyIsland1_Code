using System.Collections.Generic;

namespace Network
{
    public enum NetworkLogType
    {
        None,
        TcpNetworkCore,
        TcpServer,
        TcpClient,
        UdpNetworkCore,
        UdpServer,
        UdpClient,
        Buffer,

        MasterServer,
        MasterClient,

        ServerSessionManager,
        ClientSessionManager,

        WorldManager,
        EntityManager,
        DetectorManager,
        LocatorManager,
        ItemObjectManager,
        CheckPointManager,
        BlockingWaveEventManager,
        BridgeWaveEventManager,
        CinemaManager,
        ServerStarter,
        WebServer,
    }

    public static class LogManager
    {
        private static Dictionary<NetworkLogType, string> LogPrefixTable = new()
        {
            { NetworkLogType.None, "[LOG]" },
            { NetworkLogType.TcpNetworkCore, "[TCP]" },
            { NetworkLogType.TcpServer, "[TCP:Server]" },
            { NetworkLogType.TcpClient, "[TCP:Client]" },
            { NetworkLogType.UdpNetworkCore, "[UDP]" },
            { NetworkLogType.UdpServer, "[UDP:Server]" },
            { NetworkLogType.UdpClient, "[UDP:Client]" },
            { NetworkLogType.Buffer, "[BUFFER]" },

            { NetworkLogType.MasterServer, "[Master Server]" },
            { NetworkLogType.MasterClient, "[Master Client]" },

            { NetworkLogType.ServerSessionManager, "[Server Session Manager]" },
            { NetworkLogType.ClientSessionManager, "[Client Session Manager]" },

            { NetworkLogType.WorldManager, "[World Manager]" },
            { NetworkLogType.EntityManager, "[Entity Manager]" },
            { NetworkLogType.DetectorManager, "[Detector Manager]" },
            { NetworkLogType.LocatorManager, "[Locator Manager]" },
            { NetworkLogType.ItemObjectManager, "[Item Object Manager]" },
            { NetworkLogType.CheckPointManager, "[Check Point Manager]" },
            { NetworkLogType.BlockingWaveEventManager, "[Blocking Wave Event Manager]" },
            { NetworkLogType.BridgeWaveEventManager, "[Bridge Wave Event Manager]" },
            { NetworkLogType.CinemaManager, "[Cinema Manager]" },
            { NetworkLogType.ServerStarter, "[Server Starter]" },
            { NetworkLogType.WebServer, "[Web Server]" },
        };

        private static readonly string mPrefixError = "[ERROR]";

        public static string GetLogMessage(string message, NetworkLogType logType = NetworkLogType.None, bool hasError = false)
        {
            if (hasError)
            {
                return $"{LogPrefixTable[logType]}{mPrefixError} : {message}";
            }
            else
            {
                return $"{LogPrefixTable[logType]} : {message}";
            }
        }
    }
}
