using CulterLib.UI.Controls;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LinkLocalPopup : PopupWindow
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private TMP_InputField m_IPInput;
    [TabGroup("Component"), SerializeField] private Control_Button m_LinkBtn;
    #endregion

    #region Event
    protected override void OnInitData()
    {
        base.OnInitData();
        m_LinkBtn.OnBtnClickFunc += (_btn) =>
        {
            //TODO : 실제 방에 연결
            //RoomPopup.Instance.Open_Local($"로컬방입니당", int.Parse(m_IPInput.text.Split(':')[1]));
        };
    }
    #endregion
}
