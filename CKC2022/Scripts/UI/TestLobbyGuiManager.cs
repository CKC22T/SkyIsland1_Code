using Network.Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TestLobbyGuiManager : MonoBehaviour
{
    [SerializeField] private List<TestCharacterSelectSlotUI> CharacterSelectSlot;
    [SerializeField] private List<TextMeshProUGUI> mConnectedUsername;
    [SerializeField] private TextMeshProUGUI mLastRecievedSystemMessage;

    private void Start()
    {
        //ClientSessionManager.Instance.LastOperationMessage.OnDataChanged += onSystemMessageChanged;
        ClientNetworkManager.Instance.OnSessionDisconnected += onDisconnected;
    }

    private void OnDestroy()
    {
        //ClientSessionManager.Instance.LastOperationMessage.OnDataChanged -= onSystemMessageChanged;
        ClientNetworkManager.Instance.OnSessionDisconnected -= onDisconnected;
    }

    public void FixedUpdate()
    {
        UserSessionData_Remote userSessionData = ClientSessionManager.Instance.UserSessionData;
        var sessionSlots = userSessionData.SessionSlots;

        foreach (var characterSlot in CharacterSelectSlot)
        {
            characterSlot.SetupBySlotData(sessionSlots.GetSlotOrNull(characterSlot.CharacterType));
        }

        int usernameIndex = 0;
        foreach (var sessionSlot in sessionSlots.GetConnectedSlots())
        {
            mConnectedUsername[usernameIndex].text = sessionSlot.Username.Value;
            usernameIndex++;
        }
    }

    public void OnGotoTitle()
    {
        ClientNetworkManager.Instance.ForceDisconnect();
    }

    private void onDisconnected()
    {
        SceneManager.LoadScene(GlobalSceneName.TitleSceneName);
    }

    private void onSystemMessageChanged(string systemMessage)
    {
        //mLastRecievedSystemMessage.text = systemMessage;
    }

    public void OnLobbyClick_SelectCharacter(EntityType selectedEntityType)
    {
        ClientSessionManager.Instance.OperateLobby_SelectCharacter(selectedEntityType);
    }

    public void OnLobbyClick_Ready()
    {
        ClientSessionManager.Instance.OperateLobby_Ready();
    }

    public void OnLobbyClick_Unready()
    {
        ClientSessionManager.Instance.OperateLobby_Unready();
    }

    public void OnLobbyClick_StartGameAsSquadLeader()
    {
        ClientSessionManager.Instance.OperateLobby_StartAsSquadLeader();
    }
}
