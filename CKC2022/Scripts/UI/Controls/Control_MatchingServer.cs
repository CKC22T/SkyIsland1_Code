using CKC2022;
using CulterLib.Presets;
using CulterLib.Types;
using CulterLib.UI.Controls;
using Network.Client;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils;

public class Control_MatchingServer : Control_Button
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private TextMeshProUGUI m_ServerName;
    [TabGroup("Component"), SerializeField] private TextMeshProUGUI m_PlayerCnt;
    #endregion
    #region Value
    private RoomInfo m_Info;
    #endregion

    #region Event
    public override void OnBtnClick()
    {
        base.OnBtnClick();

        //TODO : 방에 연결되게 돌려놓기
        //RoomPopup.Instance.Open_Server(m_ServerName.text);
        NetworkExtension.TryParseEndPoint(m_Info.addr.ip, m_Info.addr.port, out var hostEP);
        if(!NetworkExtension.TryGetLocalIPAddressViaConnection(out var localAddress))
        {
            Debug.LogError("Fail TO Connect");
            return;
        }

        var ClientUdpPort = Random.Range(51000, 60000);
        ClientNetworkManager.Instance.TryConnectToServer(m_Info.addr.ip, m_Info.addr.port, localAddress, ClientUdpPort);
        //CharacterSelectPopup.Instance.Open();
    }

    #endregion

    #region Function
    public void SetServer(RoomInfo _info)
    {
        m_Info = _info;

        m_ServerName.text = _info.name;
        m_PlayerCnt.text = $"{_info.curUser}/{_info.maxUser}";
    }
    #endregion
}
