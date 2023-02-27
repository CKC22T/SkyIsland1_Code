using Network;
using Network.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;
using Utils;

public class WebServiceManager : MonoSingleton<WebServiceManager>
{
    public void TryStartServer(string ipAddress, int port)
    {
        mServerHostIP = ipAddress;
        mServerHostPort = port;
        mServerId = port - ServerConfiguration.ServerInitialPortNumber;

        DedicatedServerManager.Instance.TryStartServer(mServerHostIP, mServerHostPort);
        onServerOpend();
    }

    #region Web Service Operation

    private string mServerHostIP = "127.0.0.1";
    private int mServerId = 0;
    private int mServerHostPort = ServerConfiguration.ServerInitialPortNumber;
    [SerializeField] private string m_URL = "127.0.0.1:3000";

    struct Address
    {
        public string ip;
        public int port;

        public override string ToString() => $"[IP : {this.ip}/Port : {this.port}]";
    }

    struct Room
    {
        public int id;
        public string name;
        public int maxUser;
        public int curUser;
        Address addr;

        public override string ToString() => $"[ID : {this.id}/Name : {this.name}][Users : {this.curUser}/{this.maxUser}][IP : {this.addr.ip}/Port : {this.addr.port}]";
    }


    private void onServerOpend()
    {
        Address addr;
        addr.ip = mServerHostIP;
        addr.port = mServerHostPort;

        //var host = Dns.GetHostEntry(Dns.GetHostName());
        //foreach (var ip in host.AddressList)
        //{
        //    if (ip.AddressFamily == AddressFamily.InterNetwork)
        //    {
        //        Console.WriteLine("IP Address = " + ip.ToString());
        //        addr.ip = ip.ToString();
        //    }
        //}

        WebHttpManager.Instance.Post($"http://{m_URL}/open", addr, (err, json) =>
        {
            Debug.Log($"[WebHttp] (POST) Err[{err.ToString()}] json[{json}]");
        });
    }

    public void UpdateUser(int userCount)
    {
        Room room = new Room();
        room.id = mServerId;
        room.curUser = userCount;

        WebHttpManager.Instance.Put($"http://{m_URL}/open", room, (err, json) =>
        {
            Debug.Log($"[WebHttp] (PUT) Err[{err.ToString()}] json[{json}]");
        });
    }

    public void KillMySelf()
    {
        Room room = new Room();
        room.id = mServerId;

        WebHttpManager.Instance.Delete($"http://{m_URL}/open", room, (err, json) =>
        {
            Debug.Log($"[WebHttp] (DELETE) Err[{err.ToString()}] json[{json}]");
        });
    }

    #endregion
}
