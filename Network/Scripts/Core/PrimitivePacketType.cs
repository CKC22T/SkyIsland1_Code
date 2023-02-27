namespace Network
{
    public enum PrimitivePacketType : byte
    {
        INVALID_PACKET_TYPE_START = 0,

        #region Packet Protocol

        /// <summary>Send protobuf packet from server.</summary>
        RESPONSE_SERVER_PROTOBUF,
        /// <summary>Send protobuf packet from client.</summary>
        REQUEST_CLIENT_PROTOBUF,

        /// <summary>Message packets</summary>
        RESPONSE_SERVER_MESSAGE,
        REQUEST_CLIENT_MESSAGE,

        #endregion

        #region UDP Connection Validation Check

        /// <summary>Server send UDP port and client's session id via TCP</summary>
        RESPONSE_SERVER_UDP_PORT_AND_SESSION_ID,
        /// <summary>Client send check packet to server via UDP</summary>
        REQUEST_UDP_CONNECTION_CHECK,
        /// <summary>Server send connection checked packet to client via UDP</summary>
        RESPONSE_UDP_CONNECTION_CHECKED,
        /// <summary>Client send connection completed packet to server via TCP</summary>
        REQUEST_UDP_CONNECTION_COMPLETED,
        /// <summary>This session is completely connected</summary>
        RESPONSE_CONNECT_COMPLETED,

        #endregion

        RESPONSE_GAME_FRAME_DATA,
        REQUEST_CLIENT_INPUT_DATA,

        INVALID_PAKCET_TYPE_END
    }

    public static class PrimitivePacketTypeExtension
    {
        private const byte START_PACKET_TYPE = (byte)PrimitivePacketType.INVALID_PACKET_TYPE_START;
        private const byte END_PACKET_TYPE = (byte)PrimitivePacketType.INVALID_PAKCET_TYPE_END;

        public static bool IsValidPacketType(this in int packetType)
            => (packetType > START_PACKET_TYPE && packetType < END_PACKET_TYPE);

        public static bool IsValidPacketType(this in byte packetType)
            => (packetType > START_PACKET_TYPE && packetType < END_PACKET_TYPE);

        public static bool IsValidPacketType(this PrimitivePacketType packetType)
        {
            int type = (int)packetType;
            return (type > START_PACKET_TYPE && type < END_PACKET_TYPE);
        }
    }
}