namespace Network
{
    public enum NetworkErrorCode
    {
        UNIDENTIFIED_ERROR = 0,
        SUCCESS = 1,

        STILL_CONNECTING,
        STILL_DISCONNECTING,

        ALREADY_CONNECTED,
        ALREADY_STARTED,

        IS_NOT_STARTED,

        NOT_CONNECTED,

        SOCKET_DISPOSED,
        SOCKET_WAS_CREATED,
        SOCKET_WAS_DISPOSED_WHILE_CONNECTING,
        SOCKET_BIND_FAIL,

        WRONG_SOCKET_OPERATION,
        WRONG_IP_ADDRESS,
        WRONG_PORT,
        WRONG_IP_ADDRESS_AND_PORT,
    }
}
