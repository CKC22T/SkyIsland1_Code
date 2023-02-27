using CulterLib.Presets;
using CulterLib.UI.Controls;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLocalPopup : PopupWindow
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private TMP_InputField m_NameInput;
    [TabGroup("Component"), SerializeField] private TMP_InputField m_PortInput;
    [TabGroup("Component"), SerializeField] private Toggle m_PublicToggle;
    [TabGroup("Component"), SerializeField] private Control_Button m_CreateBtn;
    #endregion

    #region Event
    protected override void OnInitData()
    {
        base.OnInitData();
        m_CreateBtn.OnBtnClickFunc += (_btn) =>
        {
            m_CreateBtn.interactable = false;
            BlockingPopup.Instance.Open();
            GlobalManager.Instance.MatchingMgr.CreateRoom(new CreateRoomReq(), (code, id) =>
            {
                m_CreateBtn.interactable = true;
                BlockingPopup.Instance.Close();
                if (code == WebErrorCodeTemp.Success)
                {
                    //TODO : 실제 방에 연결
                    //RoomPopup.Instance.Open_Local(m_NameInput.text, int.Parse(m_PortInput.text));
                }
                else
                {
                    var mt = GlobalManager.Instance.DataMgr.GetTextTableData("Text_CreateLocalPop_Fail").GetText();
                    var st = GlobalManager.Instance.DataMgr.GetTextTableData($"Text_Error_{code}").GetText();
                    NotifyPopup.Instance.Open(mt, st, new NotifyPopup.SBtnData("Common_Ok", null));
                }
            });
        };
    }
    #endregion
}
