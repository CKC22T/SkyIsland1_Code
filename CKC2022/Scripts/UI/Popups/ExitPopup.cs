using CulterLib.UI.Controls;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitPopup : PopupWindow
{
    public static ExitPopup Instance{get; private set; }
    #region Inspector
    [TabGroup("Component"), SerializeField] private Control_Button mExitBtn;
    [TabGroup("Component"), SerializeField] private Control_Button mCancelBtn;
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
#if UNITY_EDITOR
        //UI씬인 경우 return
        if (UiTestManager.Instance)
            return;
#endif
        mExitBtn.OnBtnClickFunc += (btn) =>
        {
            //메인화면으로는 어떻게??
            ClientNetworkManager.Instance.ForceDisconnect();
        };
        mCancelBtn.OnBtnClickFunc += (btn) =>
        {
            Close();
        };
    }
    #endregion
}
