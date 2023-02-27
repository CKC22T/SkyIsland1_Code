using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using WebSocketSharp;
using Utils;

[Serializable]
public class RequestPacket
{
    public WebProtocol code;
    public Dictionary<string, object> param = new();
}

[Serializable]
public class ResponsePacket
{
    public WebProtocol code;
    public WebErrorCode error;
    public Dictionary<string, object> param = new();
}

public enum WebProtocol
{
    None = 0,
    Login,
    Logout,
    CreateRoom,
    LookUpRoom,
    Match,
    CancelMatch,
    ClientCount
}

public class WebSockManager : MonoSingleton<WebSockManager>
{
    private static readonly string NCP_IP = "49.50.161.86";
    private static readonly string LocalHost_IP = "127.0.0.1";

    private string IP = NCP_IP;
    private string PORT = "3000";
    private string SERVICE_NAME = "/ws";

    public WebSocket Socket { get; private set; }
    public bool IsConnected => Socket.IsAlive;

    private Dictionary<WebProtocol, Action<ResponsePacket>> responseCallbackHandle = new();
    private List<ResponsePacket> responseList = new();

    public Action OnOpenCallback = null;
    public Action OnCloseCallback = null;

    public bool IsOnOpen = false;
    public bool IsOnClose = false;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            Socket = new WebSocket("ws://" + IP + ":" + PORT + SERVICE_NAME);
            Socket.OnOpen += OnOpen;
            Socket.OnMessage += OnMessage;
            Socket.OnClose += OnClose;
            Socket.OnError += OnError;
            //Connect();
        }
        catch { }
    }

    #region WebSockAPISend Method

    public bool ContainsParam(ResponsePacket res, string paramKey)
    {
        if (!res.param.ContainsKey(paramKey))
        {
            Debug.Log($"Not Found Resposne Param [{paramKey}]");
            return false;
        }

        return true;
    }

    public void Login(string nickname, Action<ResponsePacket> responseCallback)
    {
        RequestPacket req = new RequestPacket();
        req.code = WebProtocol.Login;
        req.param["nickname"] = nickname;
        Send(JsonConvert.SerializeObject(req));
        AddResponseCallback(WebProtocol.Login, responseCallback);
    }

    public void Logout(Action<ResponsePacket> responseCallback)
    {
        RequestPacket req = new RequestPacket();
        req.code = WebProtocol.Logout;
        Send(JsonConvert.SerializeObject(req));
        AddResponseCallback(WebProtocol.Logout, responseCallback);
    }


    public void CreateRoom(Action<ResponsePacket> responseCallback)
    {
        RequestPacket req = new RequestPacket();
        req.code = WebProtocol.CreateRoom;
        Send(JsonConvert.SerializeObject(req));
        AddResponseCallback(WebProtocol.CreateRoom, responseCallback);
    }

    public void LookUpRoom(int roomCode, Action<ResponsePacket> responseCallback)
    {
        RequestPacket req = new RequestPacket();
        req.code = WebProtocol.LookUpRoom;
        req.param["roomCode"] = roomCode;
        Send(JsonConvert.SerializeObject(req));
        AddResponseCallback(WebProtocol.LookUpRoom, responseCallback);
    }

    public void Match(Action<ResponsePacket> responseCallback)
    {
        RequestPacket req = new RequestPacket();
        req.code = WebProtocol.Match;
        Send(JsonConvert.SerializeObject(req));
        AddResponseCallback(WebProtocol.Match, responseCallback);
    }

    public void CancelMatch(Action<ResponsePacket> responseCallback)
    {
        RequestPacket req = new RequestPacket();
        req.code = WebProtocol.CancelMatch;
        Send(JsonConvert.SerializeObject(req));
        AddResponseCallback(WebProtocol.CancelMatch, responseCallback);
    }

    private void AddResponseCallback(WebProtocol protocol, Action<ResponsePacket> responseCallback)
    {
        if(!Socket.IsAlive)
        {
            responseCallback?.Invoke(null);
            return;
        }

        responseCallbackHandle.TryAdd(protocol, responseCallback);
    }

    #endregion

    private void OnOpen(object sender, EventArgs e)
    {
        Debug.Log("OnOpen");
        IsOnOpen = true;
    }

    private void OnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log("OnMessage");
        Debug.Log($"Data : {e.Data}, RawData : {Encoding.UTF8.GetString(e.RawData)}");

        var res = JsonConvert.DeserializeObject<ResponsePacket>(e.Data);

        if (res.error != WebErrorCode.Success)
        {
            Debug.Log($"WebSocket Error : {res.error.ToString()}");
        }

        Debug.Log($"ResponseData : {res.code.ToString()}");

        if(res.code == WebProtocol.ClientCount)
        {
            if (!ContainsParam(res, "clientCount")) return;

            int clientCount = Convert.ToInt32(res.param["clientCount"]);
            GlobalNetworkCache.SetOnlineUserCount(clientCount);
            return;
        }

        responseList.Add(res);
    }

    private void Update()
    {
        while (responseList.Count > 0)
        {
            var res = responseList[0];
            responseCallbackHandle[res.code]?.Invoke(res);
            responseCallbackHandle.Remove(res.code);
            responseList.Remove(res);
        }

        if(IsOnOpen)
        {
            OnOpenCallback?.Invoke();
            IsOnOpen = false;
        }

        if(IsOnClose)
        {
            OnCloseCallback?.Invoke();
            IsOnClose = false;
        }
    }

    private void OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log("OnClose");
        Debug.Log(e.ToString());
        IsOnClose = true;
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        Debug.Log("OnError");
        Debug.Log($"Error Message : {e.Message}");
    }

    public void Connect()
    {
        try
        {
            if (Socket == null || !Socket.IsAlive)
            {
                //Socket.Connect();
                Socket.ConnectAsync();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void OnDisconnect()
    {
        try
        {
            if (Socket == null)
                return;

            if (Socket.IsAlive)
                Socket.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void Send(string msg)
    {
        if (!Socket.IsAlive) return;
        Debug.Log("Send Message : " + msg);
        try
        {
            Socket.Send(Encoding.UTF8.GetBytes(msg));
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
}