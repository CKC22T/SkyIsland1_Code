using CulterLib.UI.Popups;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameClearPopup : PopupWindow
{
    public static GameClearPopup Instance { get; private set; }

    #region Event
    protected override void OnInitSingleton()
    {
        base.OnInitSingleton();
        Instance = this;
    }
    #endregion
}
