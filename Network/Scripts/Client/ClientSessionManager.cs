using System.Collections.Concurrent;

using Network;
using Network.Client;
using Network.Packet;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

using static Network.Packet.Request.Types;
using static Network.Packet.Response.Types;

public class ClientSessionManager : MonoSingleton<ClientSessionManager>
{
    #region Test

    [Sirenix.OdinInspector.Button]
    public void SelectCharacterGriffin()
    {
        OperateLobby_SelectCharacter(EntityType.kPlayerGriffin);
    }

    [Sirenix.OdinInspector.Button]
    public void SelectCharacterPoopu()
    {
        OperateLobby_SelectCharacter(EntityType.kPlayerPoopu);
    }

    [Sirenix.OdinInspector.Button]
    public void SelectCharacterClo()
    {
        OperateLobby_SelectCharacter(EntityType.kPlayerClo);
    }

    [Sirenix.OdinInspector.Button]
    public void ReadyToStartAsMember()
    {
        OperateLobby_Ready();
    }

    [Sirenix.OdinInspector.Button]
    public void UnreadyToStartAsMember()
    {
        OperateLobby_Unready();
    }

    [Sirenix.OdinInspector.Button]
    public void ReadyToStartAsSquadLeader()
    {
        OperateLobby_StartAsSquadLeader();
    }

    #endregion

    public UserSessionData_Remote UserSessionData;
    public GameGlobalState_Remote GameGlobalState;

    private ClientNetworkManager mClientNetworkManager;

    private Action<string> mServerSystemCallbackMessage;

    public event Action<string> OnSystemMessageCallback
    {
        add => mServerSystemCallbackMessage += value;
        remove => mServerSystemCallbackMessage -= value;
    }

    public event Action<PooriScriptType> OnPooriScriptCallback;

    public event Action OnPlayerEntityChanged
    {
        add => UserSessionData.OnPlayerEntityChanged += value;
        remove => UserSessionData.OnPlayerEntityChanged -= value;
    }

    public event Action<int> OnGameStart;

    public event Action OnDisconnected;

    public event Action<Action> OnLobbyStart;

    public event Action<(string ChatUsername, string ChatMessage)> OnChatMessage;

    [field : SerializeField] public int SessionID { get; private set; } = -1;
    [field : SerializeField] public bool IsConnected { get; private set; } = false;
    [field : SerializeField] public bool IsReadyToUpdateGame { get; private set; } = false;
    [field : SerializeField] public bool IsServerReady { get; private set; } = false;
    [field : SerializeField] public string Username { get; private set; } = "";

    private SessionRequestCommandData.Builder getRequestCommandDataBuilder(RequestCommandHandle commandHandle)
    {
        return SessionRequestCommandData.CreateBuilder()
            .SetCommand(commandHandle);
    }

    private Request.Builder getRequestWithCommand(SessionRequestCommandData.Builder commandData)
    {
        return mClientNetworkManager.GetRequestBuilder(RequestHandle.kRequestCommand)
            .SetSessionRequestCommandData(commandData);
    }

    public void InitializeByManager(ClientNetworkManager clientNetworkManager)
    {
        mClientNetworkManager = clientNetworkManager;

        // These callback invoke on the mono behavior thread.
        mClientNetworkManager.OnSessionConnected += onSessionConnected;
        mClientNetworkManager.OnSessionDisconnected += onSessionDisconnected;
    }

    private void onSessionConnected(int sessionID)
    {
        SessionID = sessionID;
    }

    private void onSessionDisconnected()
    {
        string reason = GlobalTable.GetSystemMessageByResult(ServerOperationResult.ServerClosed_Disconnected);
        disconnectAndReturnToTitle(reason);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            sendCheatCodeToServer(CheatCodeType.kRegenHp);
        }
    }

    public void ResetData()
    {
        SessionID = -1;
        IsConnected = false;
        IsReadyToUpdateGame = false;
        IsServerReady = false;
        mIsLoading = false;

        UserSessionData.ResetData();
    }

    #region Setter

    public void BindUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            username = GlobalDefaultString.DefaultUsername;
        }

        Username = username;
    }

    #endregion

    #region Getter

    public bool IsMine(int entityID)
    {
        if (UserSessionData.SessionSlots.TryGetSlot(SessionID, out var slot))
        {
            if (slot.EntityID.Value == entityID)
            {
                return true;
            }
        }

        return false;
    }

    public bool TryGetMyPlayerEntityID(out int myPlayerEntityID)
    {
        if (UserSessionData.SessionSlots.TryGetSlot(SessionID, out var slot))
        {
            myPlayerEntityID = slot.EntityID.Value;
            return true;
        }

        myPlayerEntityID = -1;
        return false;
    }

    public bool TryGetPlayersEntites(out List<int> playerEntities)
    {
        return UserSessionData.SessionSlots.TryGetPlayerEntityIDs(out playerEntities);
    }

    public bool TryGetMyInventory(out ItemInventory itemInventory)
    {
        if (TryGetMySessionSlotDataOrNull(out var slot))
        {
            itemInventory = slot.Inventory;
            return true;
        }

        itemInventory = null;
        return false;
    }

    public bool TryGetCheckPointPrograss(out int currentCheckPoint, out float prograss)
    {
        currentCheckPoint = GameGlobalState.GameGlobalState.CheckPointNumber;
        prograss = 0;

        if (!CheckPointManager.TryGetInstance(out var checkPointManager))
            return false;

        if (!UserSessionData.SessionSlots.TryGetSlot(SessionID, out var slot))
            return false;

        if (!ClientWorldManager.TryGetInstance(out var instance))
            return false;

        if (!instance.TryGetEntity(slot.EntityID.Value, out var entity))
            return false;

        return checkPointManager.TryGetPrograss(entity.Position.Value, currentCheckPoint, out prograss);
    }

    public bool TryGetCurrentPlayerEntityIDs(out List<int> playerEntityIDs)
    {
        return UserSessionData.SessionSlots.TryGetPlayerEntityIDs(out playerEntityIDs);
    }

    public bool TryGetMyEntityType(out EntityType myEntityType)
    {
        myEntityType = EntityType.kNoneEntityType;

        if (UserSessionData.SessionSlots.TryGetSlot(SessionID, out var slot))
        {
            myEntityType = slot.SelectedCharacterType.Value;
        }

        return myEntityType.IsPlayerEntity();
    }

    public bool AreReadyExceptMe()
    {
        return UserSessionData.SessionSlots.AreReadyExcept(SessionID);
    }

    public bool TryGetMySessionSlotDataOrNull(out SessionSlot slot)
    {
        return UserSessionData.SessionSlots.TryGetSlot(SessionID, out slot);
    }

    public bool IsMyCharacterAlive()
    {
        return UserSessionData.IsAlive(SessionID);
    }

    #endregion

    #region Handle session command

    public void HandleSessionCommand(Response responsePacket)
    {
        if (!responsePacket.HasSessionResponseCommandData)
            return;

        var commandData = responsePacket.SessionResponseCommandData;

        Action<SessionResponseCommandData, Response> handleProcess = commandData.Command switch
        {
            // Basic Server Operation
            ResponseCommandHandle.kCommandSessionDenied => handleSessionDeniedByServer,
            ResponseCommandHandle.kCommandServerOperationResult => handleServerOperationResult,

            // Lobby
            ResponseCommandHandle.kCommandSuccessToJoinTheGame => handleSuccessToJoinTheGame,

            // Scene Changing
            ResponseCommandHandle.kCommandServerSceneLoading => handleServerSceneLoading,
            ResponseCommandHandle.kCommandResponseInitialData => handleServerResponseInitialData,
            ResponseCommandHandle.kCommandStartGame => handleServerStartGame,

            // In Game

            // Chatting
            ResponseCommandHandle.kCommandChatMessage => handleChatMessage,

            // Poori Script
            ResponseCommandHandle.kCommandPooriScript => handlePooriScript,

            _ => errorHandle
        };

        handleProcess?.Invoke(commandData, responsePacket);

        void errorHandle(SessionResponseCommandData commandData, Response responseData)
        {
            Debug.LogError(LogManager.GetLogMessage($"Cannot handle {commandData.Command} packet from server.", NetworkLogType.MasterClient, true));
        }
    }

    /// <summary>서버와의 연결을 종료하고 타이틀 씬으로 돌아갑니다.</summary>
    /// <param name="disconnectReason">접속 종료 이유</param>
    private void disconnectAndReturnToTitle(string disconnectReason)
    {
        OnDisconnected?.Invoke();

        if (!mClientNetworkManager.IsConnected)
        {
            mClientNetworkManager.ForceDisconnect();
        }

        ResetData();

        GlobalNetworkCache.BindDisconnectReason(disconnectReason);

        if(GlobalNetworkCache.GetOnVictoryCredit())
        {
            AsyncSceneLoader.Instance.AutoSceneChange(GlobalSceneName.VictoryCreditScene, LoadingOptionType.BlackFade);
        }
        else
        {
            AsyncSceneLoader.Instance.AutoSceneChange(GlobalSceneName.TitleSceneName, LoadingOptionType.BlackFade);
        }
    }

    private void handleProcessFailed(string handleFailedReason)
    {
        Debug.LogError(LogManager.GetLogMessage(handleFailedReason, NetworkLogType.ClientSessionManager, true));
        string disconnectReason =
            GlobalTable.GetSystemMessageByResult(ServerOperationResult.ClientClosed_UnexpectedErrorOccurred);

        disconnectAndReturnToTitle(disconnectReason);
    }

    private void handleSessionDeniedByServer(SessionResponseCommandData commandData, Response responseData)
    {
        if (commandData.HasDeniedReason)
        {
            Debug.Log(LogManager.GetLogMessage($"{commandData.DeniedReason}", NetworkLogType.ClientSessionManager));
        }

        disconnectAndReturnToTitle(commandData.DeniedReason);
    }

    private void handleServerOperationResult(SessionResponseCommandData commandData, Response responseData)
    {
        if (!commandData.HasServerOperationResultCode)
            return;

        string serverSystemCallbackMessage = GlobalTable.GetSystemMessageByCommandData(commandData);

        Debug.Log(LogManager.GetLogMessage($"Server Operation Result : {serverSystemCallbackMessage}", NetworkLogType.ClientSessionManager));

        mServerSystemCallbackMessage?.Invoke(serverSystemCallbackMessage);
    }

    private void handleSuccessToJoinTheGame(SessionResponseCommandData commandData, Response responseData)
    {
        var usernameCommandData = getRequestCommandDataBuilder(RequestCommandHandle.kCommandLobby_BindUsername)
            .SetUsername(Username);
        var usernameResponseData = getRequestWithCommand(usernameCommandData).Build();

        mClientNetworkManager.SendToServerViaTcp(usernameResponseData);

        BookOpener.Instance.Open(() =>
        {   //책 다보고나면 씬전환
            AsyncSceneLoader.Instance.AutoSceneChange(GlobalSceneName.LobbySceneName, LoadingOptionType.BlackFade);
        });
        //SceneManager.LoadScene(GlobalSceneName.LobbySceneName);
    }

    private bool mIsLoading = false;

    private void handleServerSceneLoading(SessionResponseCommandData commandData, Response responseData)
    {
        if (mIsLoading)
        {
            return;
        }

        if (!commandData.HasChangeMapName)
        {
            Debug.LogWarning(LogManager.GetLogMessage($"There is no map change data!", NetworkLogType.ClientSessionManager, true));

            string reason = GlobalTable.GetSystemMessageByResult(ServerOperationResult.ServerClosed_SceneChangeError);

            disconnectAndReturnToTitle(reason);
            return;
        }

        string mapName = commandData.ChangeMapName;

        mIsLoading = true;

        if(OnLobbyStart == null)
        {
            AsyncSceneLoader.Instance.SceneChange(GlobalSceneName.ClientSceneName, LoadingOptionType.BlackFade, () =>
            {
                ClientWorldManager.Instance.TryLoadScene(mapName, () =>
                {
                    sendCommandToServer(RequestCommandHandle.kCommandSceneChanging_SceneLoadComplete);
                    mIsLoading = false;
                });
            });
            return;
        }

        OnLobbyStart?.Invoke(() =>
        {
            AsyncSceneLoader.Instance.SceneChange(GlobalSceneName.ClientSceneName, LoadingOptionType.Prograss, () =>
            {
                ClientWorldManager.Instance.TryLoadScene(mapName, () =>
                {
                    sendCommandToServer(RequestCommandHandle.kCommandSceneChanging_SceneLoadComplete);
                    mIsLoading = false;
                });
            });
        });
        //SceneManager.LoadSceneAsync(GlobalSceneName.ClientSceneName).completed += (operation)=>
        //{
        //    ClientWorldManager.Instance.TryLoadScene(mapName, ()=>
        //    {
        //        sendCommandToServer(RequestCommandHandle.kCommandSceneChanging_SceneLoadComplete);
        //        mIsLoading = false;
        //    });
        //};
    }

    private void handleServerResponseInitialData(SessionResponseCommandData commandData, Response responseData)
    {
        Debug.Log(LogManager.GetLogMessage($"Send to server i'm ready!", NetworkLogType.ClientSessionManager));
        sendCommandToServer(RequestCommandHandle.kCommandSceneChanging_ReadyToStart);
    }

    private void handleServerStartGame(SessionResponseCommandData commandData, Response responseData)
    {
        Debug.Log(LogManager.GetLogMessage($"Server start the game!", NetworkLogType.ClientSessionManager));
        OnGameStart?.Invoke(SessionID);
        AsyncSceneLoader.Instance.SceneChangeEnd();

        if (!ServerConfiguration.IsJoinInGame)
        {
            ServerConfiguration.TriggerStartInitialCutscene = true;
        }

        ServerConfiguration.IsJoinInGame = true;
    }

    private void handleChatMessage(SessionResponseCommandData commandData, Response responseData)
    {
        if (!commandData.HasChatMessage || !commandData.HasChatUsername)
        {
            Debug.LogError(LogManager.GetLogMessage($"잘못된 채팅 메세지", NetworkLogType.ClientSessionManager, true));
            return;
        }

        string username = commandData.ChatUsername;
        string chatMessage = commandData.ChatMessage;

        OnChatMessage?.Invoke((username, chatMessage));
    }

    private void handlePooriScript(SessionResponseCommandData commandData, Response responseData)
    {
        if (!commandData.HasPooriScriptCode)
        {
            Debug.LogError(LogManager.GetLogMessage($"There is no poori sciprt code!", NetworkLogType.ClientSessionManager, true));
            return;
        }

        var pooriCode = (PooriScriptType)commandData.PooriScriptCode;
        OnPooriScriptCallback?.Invoke(pooriCode);
    }

    private void handleSomething(SessionResponseCommandData commandData, Response responseData)
    {

    }

    #endregion

    #region Session Operation

    private void sendCheatCodeToServer(CheatCodeType cheatType)
    {
        var commandData = getRequestCommandDataBuilder(RequestCommandHandle.kCheatRequest)
            .SetRequestCheatCode(cheatType);
        var requestData = getRequestWithCommand(commandData).Build();

        mClientNetworkManager.SendToServerViaTcp(requestData);
    }

    public ServerOperationResult DisconnectFromServer()
    {
        if (!mClientNetworkManager.IsConnected)
        {
            return ServerOperationResult.NotConnectedToServer;
        }

        mClientNetworkManager.ForceDisconnect();
        return ServerOperationResult.Success;
    }

    /// <summary>캐릭터를 선택합니다. 이미 선택한 캐릭터를 선택하면 선택을 취소합니다.</summary>
    /// <param name="selectedCharacter">선택한 캐릭터의 EntityType 입니다.</param>
    public void OperateLobby_SelectCharacter(EntityType selectedCharacter)
    {
        var commandData = getRequestCommandDataBuilder(RequestCommandHandle.kCommandLobby_SelectCharacter)
            .SetSelectedCharacter(selectedCharacter);
        var requestData = getRequestWithCommand(commandData);

        mClientNetworkManager.SendToServerViaTcp(requestData.Build());
    }

    ///// <summary>캐릭터 선택을 취소합니다.</summary>
    //public void OperateLobby_CancelCharacterSelection()
    //{
    //    var commandData = getRequestCommandDataBuilder(RequestCommandHandle.kCommandLobby_SelectCharacter)
    //        .SetSelectedCharacter(EntityType.kNoneEntityType);
    //    var requestData = getRequestWithCommand(commandData);

    //    mClientNetworkManager.SendToServerViaTcp(requestData.Build());
    //}

    /// <summary>서버에게 준비 상태를 요청합니다.</summary>
    public void OperateLobby_Ready()
    {
        var commandData = getRequestCommandDataBuilder(RequestCommandHandle.kCommandLobby_ReadyState)
            .SetReadyState(true);
        var requestData = getRequestWithCommand(commandData);

        mClientNetworkManager.SendToServerViaTcp(requestData.Build());
    }

    /// <summary>서버에게 준비 해제를 요청합니다.</summary>
    public void OperateLobby_Unready()
    {
        var commandData = getRequestCommandDataBuilder(RequestCommandHandle.kCommandLobby_ReadyState)
            .SetReadyState(false);
        var requestData = getRequestWithCommand(commandData);

        mClientNetworkManager.SendToServerViaTcp(requestData.Build());
    }

    /// <summary>방장으로서 서버에게 게임 시작을 요청합니다. 서버가 게임을 시작하고 씬 변경을 명령합니다..</summary>
    public void OperateLobby_StartAsSquadLeader()
    {
        // 자기 자신을 제외한 모든 플레이어가 준비되었는 지를 검사합니다.
        if (!AreReadyExceptMe())
        {
            return;
        }

        sendCommandToServer(RequestCommandHandle.kCommandLobby_StartAsSquadLeader);
    }

    private void sendCommandToServer(RequestCommandHandle commandHandle)
    {
        var commandData = getRequestCommandDataBuilder(commandHandle);
        var requestData = getRequestWithCommand(commandData);

        mClientNetworkManager.SendToServerViaTcp(requestData.Build());
    }

    public void SendChatMessage(string message)
    {
        var commandData = getRequestCommandDataBuilder(RequestCommandHandle.kCommandChat_SendMessage)
            .SetChatMessage(message)
            .SetUsername(Username);
        var requestData = getRequestWithCommand(commandData);

        mClientNetworkManager.SendToServerViaTcp(requestData.Build());
    }

    #endregion
}
