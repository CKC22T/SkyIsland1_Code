using CulterLib.UI.Controls;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePopup : PopupWindow
{
    public static PausePopup Instance { get; private set; }

    #region Inspector
    [TabGroup("Component"), SerializeField] Control_Button m_OkBtn;
    [TabGroup("Component"), SerializeField] Control_Button m_MorePlayBtn;
    #endregion

    #region Event
    protected override void OnInitSingleton()
    {
        base.OnInitSingleton();
        Instance = this;
    }
    protected override void OnInitData()
    {
        base.OnInitData();

        m_OkBtn.OnBtnClickFunc += (btn) =>
        {   //
            ClientNetworkManager.Instance.ForceDisconnect();
        };
        m_MorePlayBtn.OnBtnClickFunc += (btn) =>
        {   //
            Close();
        };
    }
    #endregion
}
