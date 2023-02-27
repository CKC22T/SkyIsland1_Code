using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Utils;
using System;

#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public class ConnectionNotification : Singleton<ConnectionNotification>
{
    private const string discord_hook = "https://discord.com/api/webhooks/924219928129728553/T88zhHbnA82MVbAUzWVXAh715q6n8_ghgX7_EfLtctmmZlgzEvY6QELdXGwJEhxejXUq";
    private const string InitializeKey = "_INITIALIZEKEY";
    private const string NotifierKey = "_NOTIFIERKEY";

    private static string ConnectedMessage => $"{SystemInfo.deviceName}에서 {DateTime.Now}에 접속하였습니다.";
    private static string DisconnectedMessage => $"{SystemInfo.deviceName}에서 {DateTime.Now}에 종료하였습니다.";

    private static string TestMessage = $"웹후크 메시지 테스트";

    private const string MenuName = "디스코드 알림설정";

    private static string LightmapBakeID;

    [MenuItem(MenuName + "/TestSendWebHook")]
    public static void SendTestMessage()
    {
        SendConnectionMessage(TestMessage);
    }

    [MenuItem(MenuName + "/접속알림 켜기")]
    public static void SetConnectionNotifierToON()
    {
        PlayerPrefs.SetInt(NotifierKey, 1);
        Debug.Log("접속알람 : ON");
    }

    [MenuItem(MenuName + "/접속알림 끄기")]
    public static void SetConnectionNotifierToOFF()
    {
        PlayerPrefs.SetInt(NotifierKey, 0);
        Debug.Log("접속알람 : OFF");
    }



    [MenuItem(MenuName + "/라이트맵 굽기알림 켜기")]
    public static void LightmapNotificationON()
    {
        Lightmapping.bakeCompleted += Lightmapping_bakeCompleted;
        Lightmapping.bakeStarted += Lightmapping_bakeStarted;
        Debug.Log("라이트맵 굽기알림 : ON");
    }

    [MenuItem(MenuName + "/라이트맵 굽기알림 끄기")]
    public static void LightmapNotificationOFF()
    {
        Lightmapping.bakeCompleted -= Lightmapping_bakeCompleted;
        Lightmapping.bakeStarted -= Lightmapping_bakeStarted;
        Debug.Log("라이트맵 굽기알림 : OFF");
    }


    private static void Lightmapping_bakeStarted()
    {
        LightmapBakeID = GUID.Generate().ToString() + " at " + DateTime.UtcNow;
        SendConnectionMessage("라이트맵 굽기 시작 : " + LightmapBakeID);
        Debug.Log("라이트맵 굽기 시작 : " + LightmapBakeID);
    }

    private static void Lightmapping_bakeCompleted()
    {
        SendConnectionMessage(LightmapBakeID + $"의 라이트맵 굽기를 {DateTime.UtcNow}에 완료");
    }

    static ConnectionNotification()
    {
        if (PlayerPrefs.GetInt(NotifierKey, 0) != 1)
            return;

        if (!SessionState.GetBool(InitializeKey, false))
        {
            SessionState.SetBool(InitializeKey, true);

            SendConnectionMessage(ConnectedMessage);

            EditorApplication.quitting += EditorApplication_quitting;
        }
    }

    private static void EditorApplication_quitting()
    {
        if (PlayerPrefs.GetInt(NotifierKey, 0) != 1)
            return;

        SendConnectionMessage(DisconnectedMessage);
    }

    private static void SendConnectionMessage(string message)
    {
        var form = new WWWForm();
        form.AddField("content", message);
        var request = UnityWebRequest.Post(discord_hook, form);
        request.SendWebRequest();
    }
}


#endif