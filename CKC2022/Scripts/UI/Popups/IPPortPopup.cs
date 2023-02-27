using CKC2022;
using CulterLib.Presets;
using CulterLib.UI.Controls;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using Utils;

public class IPPortPopup : PopupWindow
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private TMP_InputField mIpPortInput;
    [TabGroup("Component"), SerializeField] private Control_Button mConnectBtn;
    #endregion

    #region Event
    protected override void OnInitData()
    {
        base.OnInitData();

        //이벤트 초기화
        mConnectBtn.OnBtnClickFunc += (btn) =>
        {
                    };
    }
    #endregion
}
