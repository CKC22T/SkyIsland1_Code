using CKC2022;
using CulterLib.Presets;
using CulterLib.Types;
using Network.Packet;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomMine : Mono_UI
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private GameObject mWakeup;
    [TabGroup("Component"), SerializeField] private LeaderTagUI mLeaderTag;
    [TabGroup("Component"), SerializeField] private MemberTagUI mMemberTag;
    [TabGroup("Component"), SerializeField] private EntityType characterType;
    [TabGroup("Component"), SerializeField] private Animator characterAnimator;
    #endregion

    #region Event
    protected override void OnInitData()
    {
        base.OnInitData();
        mWakeup.SetActive(false);
    }
    public void OnPointerEnter(BaseEventData data)
    {
        if (mLeaderTag.gameObject.activeSelf || mMemberTag.gameObject.activeSelf) return;
        mWakeup.gameObject.SetActive(true);
    }
    public void OnPointerExit(BaseEventData data)
    {
        mWakeup.gameObject.SetActive(false);
    }
    public void OnPointerDown(BaseEventData data)
    {
        mWakeup.gameObject.SetActive(false);
        ClientSessionManager.Instance.OperateLobby_SelectCharacter(characterType);

        GameSoundManager.Play(SoundType.UI_Lobby_Button, new SoundPlayData(Camera.allCameras[0].transform.position));
    }
    #endregion

    private bool isStarted = false;

    private void Start()
    {
        mWakeup.SetActive(false);
        mLeaderTag.gameObject.SetActive(false);
        mMemberTag.gameObject.SetActive(false);
        UpdateState();
    }

    private void FixedUpdate()
    {
        UpdateState();
    }

    public void UpdateState()
    {
        if (isStarted)
            return;

        var userSessionData = ClientSessionManager.Instance.UserSessionData;
        var slots = userSessionData.SessionSlots.GetConnectedSlots();
        foreach (var slot in slots)
        {
            if (slot.SelectedCharacterType.Value == characterType)
            {
                string currentUsername = userSessionData.GetUsernameByCharacterType(characterType);
                var characterState = userSessionData.GetLobbyReadyStateByCharacterType(characterType);

                characterAnimator.ResetTrigger("Deselect");
                characterAnimator.SetTrigger("Select");
                //방장일때
                if (userSessionData.IsSquadLeaderCharacter(characterType))
                {
                    mLeaderTag.gameObject.SetActive(true);
                    mMemberTag.gameObject.SetActive(false);
                    mLeaderTag.SetName(currentUsername);
                    return;
                }

                //맴버일때
                mLeaderTag.gameObject.SetActive(false);
                mMemberTag.gameObject.SetActive(true);
                mMemberTag.SetName(currentUsername);
                if (characterState == CharacterState.Readied)
                {
                    mMemberTag.Ready();
                }
                else
                {
                    mMemberTag.UnReady();
                }
                return;
            }
        }

        mLeaderTag.gameObject.SetActive(false);
        mMemberTag.gameObject.SetActive(false);
        characterAnimator.ResetTrigger("Select");
        characterAnimator.SetTrigger("Deselect");
    }

    public void StartState()
    {
        var slots = ClientSessionManager.Instance.UserSessionData.SessionSlots.GetConnectedSlots();
        foreach (var slot in slots)
        {
            if (slot.SelectedCharacterType.Value == characterType)
            {
                characterAnimator.ResetTrigger("Deselect");
                characterAnimator.ResetTrigger("Select");
                characterAnimator.SetTrigger("Start");

                isStarted = true;
                break;
            }
        }
    }
}
