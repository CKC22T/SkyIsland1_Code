public static class GlobalNetworkCache
{
    private static string mDisconnectedReason;

    public static void BindDisconnectReason(string reason) => mDisconnectedReason = reason;

    public static bool TryGetDisconnectedReason(out string reason)
    {
        if (string.IsNullOrEmpty(mDisconnectedReason) || string.IsNullOrWhiteSpace(mDisconnectedReason))
        {
            reason = "";
            return false;
        }

        reason = mDisconnectedReason;
        return true;
    }

    //Web
    private static int mOnlineUserCount = 0;

    public static void SetOnlineUserCount(int userCount) => mOnlineUserCount = userCount;

    public static int GetOnlineUserCount() => mOnlineUserCount;

    //Lobby
    private static string mLobbyInfo = "";
    public static void SetLobbyInfo(string lobbyInfo) => mLobbyInfo = lobbyInfo;
    public static string GetLobbyInfo() => mLobbyInfo;

    //Victory
    private static bool mOnVictoryCredit = false;
    public static void SetOnVictoryCredit(bool isOn) => mOnVictoryCredit = isOn;
    public static bool GetOnVictoryCredit() => mOnVictoryCredit;
}
