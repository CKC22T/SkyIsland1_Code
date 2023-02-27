using CKC2022;
using CulterLib.Presets;
using CulterLib.Types;
using CulterLib.UI.Controls;
using CulterLib.UI.Popups;
using Network.Client;
using Network.Packet;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPopup : PopupWindow
{
    public static RoomPopup Instance { get; private set; }
    #region Inspector
    [TabGroup("Component"), SerializeField] private RoomMine[] mMine;
    [TabGroup("Component"), SerializeField] private Button startButton;
    [TabGroup("Component"), SerializeField] private Button readyButton;
    [TabGroup("Component"), SerializeField] private Button unreadyButton;
    [TabGroup("Component"), SerializeField] private SystemLogUI systemLog;
    [TabGroup("Component"), SerializeField] private TextMeshProUGUI lobbyInfoText;
    [TabGroup("Component"), SerializeField] private RoomCountUI mRoomCountUI;
    [TabGroup("Component"), SerializeField] private List<GameObject> offUIList;
    #endregion

    [SerializeField] private float mGameStartDelay;

    #region Event
    protected override void OnInitSingleton()
    {
        base.OnInitSingleton();
        Instance = this;
    }

    private void Start()
    {
        lobbyInfoText.text = GlobalNetworkCache.GetLobbyInfo();
        ClientSessionManager.Instance.OnLobbyStart += GameStarting;

        if (ClientSessionManager.Instance.UserSessionData.IsCurrentlySquadLeader())
        {
            startButton.gameObject.SetActive(true);
            readyButton.gameObject.SetActive(false);
        }
        else
        {
            startButton.gameObject.SetActive(false);
            readyButton.gameObject.SetActive(true);
        }
        unreadyButton.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        ClientSessionManager.Instance.OnLobbyStart -= GameStarting;
    }

    private void FixedUpdate()
    {
#if UNITY_EDITOR
        //UI씬인 경우 return
        if (UiTestManager.Instance)
            return;
#endif
        if (Input.GetKeyDown(KeyCode.Alpha0))
            TitleManager.Instance.ToLobby();
        var userSessionData = ClientSessionManager.Instance.UserSessionData;

        //방장일때
        if (userSessionData.IsCurrentlySquadLeader())
        {
            startButton.interactable = true;
            foreach (var slot in userSessionData.SessionSlots.GetConnectedSlots())
            {
                if (slot.SessionID.Value == ClientSessionManager.Instance.SessionID)
                {
                    if (userSessionData.GetLobbyReadyStateByCharacterType(slot.SelectedCharacterType.Value) != CharacterState.Selected)
                    {
                        startButton.interactable = false;
                        break;
                    }
                }
                else
                {
                    if (userSessionData.GetLobbyReadyStateByCharacterType(slot.SelectedCharacterType.Value) != CharacterState.Readied)
                    {
                        startButton.interactable = false;
                        break;
                    }
                }
            }
        }
        else
        {
            readyButton.interactable = false;
            if (ClientSessionManager.Instance.TryGetMySessionSlotDataOrNull(out var slot))
            {
                var characterState = userSessionData.GetLobbyReadyStateByCharacterType(slot.SelectedCharacterType.Value);
                if (characterState == CharacterState.Selected ||
                    characterState == CharacterState.Readied)
                {
                    readyButton.interactable = true;
                }

                readyButton.gameObject.SetActive(characterState != CharacterState.Readied);
                unreadyButton.gameObject.SetActive(characterState == CharacterState.Readied);
            }
        }
    }
    #endregion
    #region Function
    //Public
    /// <summary>
    /// 캐릭터 선택 버튼들을 전부 숨깁니다.
    /// </summary>
    public void HideMine()
    {
        foreach (var v in mMine)
            v.gameObject.SetActive(false);
    }

    public void UpdateSlotState()
    {
        foreach (var slot in mMine)
        {
            slot.UpdateState();
        }
    }

    public void RoomExit()
    {
        ExitPopup.Instance.Open();
        //ClientNetworkManager.Instance.ForceDisconnect();
    }

    public void RoomReady()
    {
        ClientSessionManager.Instance.OperateLobby_Ready();
    }

    public void RoomUnReady()
    {
        ClientSessionManager.Instance.OperateLobby_Unready();
    }

    public void RoomStart()
    {
        ClientSessionManager.Instance.OperateLobby_StartAsSquadLeader();
    }

    public void GameStarting(Action onCallback)
    {
        StartCoroutine(startingRoutine());

        IEnumerator startingRoutine()
        {
            foreach(var ui in offUIList)
            {
                ui.SetActive(false);
            }


            mRoomCountUI.gameObject.SetActive(true);
            mRoomCountUI.Open(mGameStartDelay);
            foreach(var mine in mMine)
            {
                mine.StartState();
            }
            yield return new WaitForSeconds(mGameStartDelay);
            onCallback.Invoke();
        }
    }
    #endregion
}
