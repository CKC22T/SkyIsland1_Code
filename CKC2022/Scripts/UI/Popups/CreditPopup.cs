using CulterLib.UI.Popups;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditPopup : PopupWindow
{
    public static CreditPopup Instance { get; private set; }

    #region Event
    protected override void OnInitSingleton()
    {
        base.OnInitSingleton();
        Instance = this;
    }
    #endregion
}
