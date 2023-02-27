using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;

using System.Linq;
using System.Linq.Expressions;


    
public class ClientConnectWindow : OdinEditorWindow
{
    [MenuItem("Window/ClientConnectWindow", false, 2)]
    static void Init()
    {
        ClientConnectWindow window = CreateInstance<ClientConnectWindow>();
        window.Show();
    }


    [Sirenix.OdinInspector.Button(Name = "Bind Random Client Port")]   public void Test_BindRandomPort()
       => ClientNetworkManager.Instance.Test_BindRandomPort();

    [Sirenix.OdinInspector.Button(Name = "SetClientIP")]    public void Test_SetClientIP()
      => ClientNetworkManager.Instance.Test_SetClientIP();

    [Sirenix.OdinInspector.Button(Name = "Connect To Server")]    public void Test_ConnectToServer()
      => ClientNetworkManager.Instance.Test_ConnectToServer();

    [Sirenix.OdinInspector.Button(Name = "Disconnect From Server")]    public void Test_ForceDisconnect()
      => ClientNetworkManager.Instance.Test_ForceDisconnect();

    [Sirenix.OdinInspector.Button(Name = "Send Message via TCP")]    public void Test_SendMessageViaTCP()
      => ClientNetworkManager.Instance.Test_SendMessageViaTCP();

    [Sirenix.OdinInspector.Button(Name = "Send Message via UDP")]    public void Test_SendMessageViaUDP()
      => ClientNetworkManager.Instance.Test_SendMessageViaUDP();

}

#else
public class ClientConnectWindow : MonoBehaviour
{
}
#endif
