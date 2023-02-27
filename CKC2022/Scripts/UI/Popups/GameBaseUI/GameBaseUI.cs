using CKC2022;
using CKC2022.GameData.Data;
using CKC2022.Input;
using CulterLib.Presets;
using CulterLib.Types;
using CulterLib.UI.Popups;
using CulterLib.Utils;
using Network.Client;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils;
using static CharacterUI;

public class GameBaseUI : PopupWindow
{
    public static GameBaseUI Instance { get; private set; }

    #region Inspector

    [TabGroup("Component"), SerializeField] GameObject playerListUI;
    [TabGroup("Component"), SerializeField] CheckPointUI checkPointUI;

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
        if (UiTestManager.Instance)
            return; //UI씬인 경우 return
#endif

        //이벤트 초기화
        ClientSessionManager.Instance.UserSessionData.OnPlayerEntityChanged += onPlayerEntityChanged;
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (UiTestManager.Instance)
            return; //UI씬인 경우 return
#endif

    }
    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (UiTestManager.Instance)
            return; //UI씬인 경우 return
#endif

        //단축키
        checkPointUI.SetActiveAreaName(Input.GetKey(KeyCode.Tab));
        playerListUI.SetActive(Input.GetKey(KeyCode.Tab));
        if (Input.GetKeyDown(KeyCode.Escape) && UIManager.Instance.PopMgr.OpenedPopup[UIManager.Instance.PopMgr.OpenedPopup.Count - 1].IsBasePopup)
            PausePopup.Instance.Open();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();

#if UNITY_EDITOR
        if (UiTestManager.Instance)
            return; //UI씬인 경우 return
#endif

        //이벤트 제거
        if (ClientSessionManager.IsQuitting == false)
            ClientSessionManager.Instance.UserSessionData.OnPlayerEntityChanged -= onPlayerEntityChanged;
    }

    //Data Event
    private void onPlayerEntityChanged()
    {
        if (!ClientSessionManager.Instance.IsMyCharacterAlive())
            ObserverPopup.Instance.Open();
        else
            ObserverPopup.Instance.Close();
    }
    #endregion
}