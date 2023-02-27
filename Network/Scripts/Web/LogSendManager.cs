using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

[System.Serializable]
public class LogPacket
{
    public string logType;
    public string targetType;
    public int targetCode;
    public string logJsonData; // json.string
}

public class LogSendManager : MonoSingleton<LogSendManager>
{
    [SerializeField] private string url = "127.0.0.1:3010";

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Log(LogPacket logPacket, Action<WebErrorCode, string> onEnd = null)
    {
        WebHttpManager.Instance.Post($"http://{url}/log", logPacket, (err, json) =>
        {
            Debug.Log($"LogManager  Result : {err.ToString()} Data : {json}");
            onEnd?.Invoke(err, json);
        });
    }

    private void Log(string logType, string targetType, int targetCode, string logJsonData, Action<WebErrorCode, string> onEnd = null)
    {
        LogPacket logPacket = new LogPacket();
        logPacket.logType = logType;
        logPacket.targetType = targetType;
        logPacket.targetCode = targetCode;
        logPacket.logJsonData = logJsonData;

        WebHttpManager.Instance.Post($"http://{url}/log", logPacket, (err, json) =>
        {
            Debug.Log($"LogManager  Result : {err.ToString()} Data : {json}");
            onEnd?.Invoke(err, json);
        });
    }

    public void UserLog(string logType, int userCode, string logJsonData, Action<WebErrorCode, string> onEnd = null)
    {
        Log(logType, "User", userCode, logJsonData, onEnd);
    }

    public void ServerLog(string logType, int serverCode, string logJsonData, Action<WebErrorCode, string> onEnd = null)
    {
        Log(logType, "Server", serverCode, logJsonData, onEnd);
    }


    private struct JoinDB
    {
        public int id;
        public float time;
    }
    public void JoinTime(int id, float time, Action<WebErrorCode, string> onEnd = null)
    {
        var joinData = new JoinDB();
        joinData.id = id;
        joinData.time = time;

        WebHttpManager.Instance.Post($"http://{url}/joinTime", joinData, (err, json) =>
        {
            Debug.Log($"LogManager  Result : {err.ToString()} Data : {json}");
            onEnd?.Invoke(err, json);
        });
    }
}
