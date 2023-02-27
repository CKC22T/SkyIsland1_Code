using System;
using System.Net;
using System.Net.Sockets;

namespace Utils
{
    public static class NetworkExtension
    {
        public static void ParseIpAddressAndPort(this EndPoint endPoint, out string ipAddress, out int port)
        {
            var ipEndPoint = endPoint as IPEndPoint;
            ipAddress = ipEndPoint.Address.ToString();
            port = ipEndPoint.Port;
        }

        public static void ParseIpAddressAndPort(this IPEndPoint endPoint, out string ipAddress, out int port)
        {
            ipAddress = endPoint.Address.ToString();
            port = endPoint.Port;
        }

        public static bool TryParseIPAddressFromString(this string hostIPAddress, out IPAddress ipAddress)
        {
            if (IPAddress.TryParse(hostIPAddress, out ipAddress))
                return true;

            if (Uri.TryCreate(hostIPAddress, UriKind.Absolute, out var uri) && !string.IsNullOrEmpty(uri.Host))
            {
                ipAddress = Dns.GetHostAddresses(uri.Host)[0];
                return true;
            }
            
            return false;
        }

        /// <summary>IPEndPoint를 파싱합니다.</summary>`
        /// <param name="serverHostIpAddress">서버의 IP 주소입니다.</param>
        /// <param name="serverHostPort">서버의 Port 주소입니다.</param>
        /// <param name="hostEndPoint">파싱된 서버의 IPEndPoint 입니다.</param>
        /// <returns>파싱 성공 여부입니다.</returns>
        public static bool TryParseEndPoint(string serverHostIpAddress, int serverHostPort, out IPEndPoint hostEndPoint)
        {
            hostEndPoint = null;

            if (IsValidPort(serverHostPort) == false)
            {
                return false;
            }

            if (IPAddress.TryParse(serverHostIpAddress, out var hostAddress))
            {
                hostEndPoint = new IPEndPoint(hostAddress, serverHostPort);
                return true;
            }

            return false;
        }

        [System.Obsolete()]
        public static bool TryGetLocalIPAddress(out string address)
        {
            address = string.Empty;

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    address = ip.ToString();
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetLocalIPAddressViaConnection(out string address)
        {
            string localIP = string.Empty;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }

            address = localIP;
            return true;
        }


        /// <summary>해당 포트가 유효한 포트인지 검사합니다.</summary>
        /// <param name="port">판단할 포트입니다.</param>
        /// <returns>유효한 포트인 경우 'true'를 반환합니다.</returns>
        public static bool IsValidPort(int port) => port >= 0 || port <= 65535;
    }
}
