using Network;
using Network.Packet;
using Network.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using static Network.Packet.Request.Types;
using static Network.Packet.Response.Types;

public enum ServerState
{
    None = 0,
    Lobby,
    SceneLoading,
    SceneLoadCompleted,
    GamePlayScene,
}

public class ServerSessionManager : MonoSingleton<ServerSessionManager>
{
    [Sirenix.OdinInspector.Button]
    public void SetupMapToGamePlayScene()
    {
        GameMapSceneName = GlobalSceneName.GamePlayScene;
    }

    [Sirenix.OdinInspector.Button]
    public void SetupMapToTestPlayScene()
    {
        GameMapSceneName = GlobalSceneName.TestPlayScene;
    }

    public string GameMapSceneName = GlobalSceneName.GamePlayScene;

    public UserSessionData_Master UserSessionData;
    public GameGlobalState_Master GameGlobalState;

    private DedicatedServerManager mDedicatedServerManager;

    public ServerState CurrentServerState { get; private set; } = ServerState.None;

    #region Initializer

    private bool mIsInitialized = false;

    public void InitializeByManager(DedicatedServerManager server)
    {
        if (mIsInitialized)
            return;

        mDedicatedServerManager = server;

        mDedicatedServerManager.OnSessionConnected += onSessionConnected;
        mDedicatedServerManager.OnSessionDisconnected += onSessionDisconnected;

        mIsInitialized = true;

        CurrentServerState = ServerState.Lobby;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (!mIsInitialized)
            return;

        mDedicatedServerManager.OnSessionConnected -= onSessionConnected;
        mDedicatedServerManager.OnSessionDisconnected -= onSessionDisconnected;

        mIsInitialized = false;
    }

    #endregion

    private void resetInGameData()
    {
        GameGlobalState.GameGlobalState.ResetData();
        UserSessionData.SessionSlots.ResetInventory();
    }

    private SessionResponseCommandData.Builder getResponseCommandDataBuilder(ResponseCommandHandle commandHandle)
    {
        return SessionResponseCommandData.CreateBuilder()
            .SetCommand(commandHandle);
    }

    private Response.Builder getResponseWithCommand(SessionResponseCommandData.Builder commandData)
    {
        return mDedicatedServerManager.GetBaseResponseBuilder(ResponseHandle.kResponseCommand)
            .SetSessionResponseCommandData(commandData);
    }

    #region Session Management

    private void onSessionConnected(int connectedSessionID)
    {
        if (UserSessionData.SessionSlots.Contains(connectedSessionID))
            return;

        // Check if server is full.
        if (!UserSessionData.SessionSlots.HasExtraSlot())
        {
            DenySession(connectedSessionID, "Server is full");
            return;
        }

        // Check if it's allowed to join the game
        if (CurrentServerState == ServerState.Lobby)
        {
            var commandDataBuilder = getResponseCommandDataBuilder(ResponseCommandHandle.kCommandSuccessToJoinTheGame);
            var responsePacket = getResponseWithCommand(commandDataBuilder);

            mDedicatedServerManager.SendToClient_TCP(connectedSessionID, responsePacket.Build());

            ServerMasterNetObjectManager.Instance.SendInitialDataToClient(connectedSessionID);

            UserSessionData.Connected(connectedSessionID);
            return;
        }

        // Check if server is already started // TODO : Skip if server was test mode
        if (CurrentServerState == ServerState.GamePlayScene)
        {
            DenySession(connectedSessionID, "Game session was already started");
        }
        // Check if server unknown state
        else
        {
            DenySession(connectedSessionID, "Unknown reason");
        }

        return;
    }

    private void onSessionDisconnected(int disconnectedSessionID)
    {
        UserSessionData.Disconnected(disconnectedSessionID);

        // 남아있는 사람이 없으면 종료
        if (UserSessionData.SessionSlots.IsAllSlotEmpty())
        {
            string reason = GlobalTable.GetSystemMessageByResult(ServerOperationResult.ServerClosed_AllSessionDisconnected);
            StopServer(reason);
            return;
        }

        // 방장이 나가면 종료
        if (UserSessionData.SessionSlots.IsSquadLeader(disconnectedSessionID))
        {
            string reason = GlobalTable.GetSystemMessageByResult(ServerOperationResult.ServerClosed_SquadLeaderDisconnected);
            StopServer(reason);
            return;
        }
    }

    public void DenySession(int sessionID, string denyReason)
    {
        string reason = $"Disconnected from server.\n{denyReason}";

        var commandDataBuilder = getResponseCommandDataBuilder(ResponseCommandHandle.kCommandSessionDenied)
            .SetDeniedReason(reason);
        var responsePacket = getResponseWithCommand(commandDataBuilder);

        mDedicatedServerManager.SendToClient_TCP(sessionID, responsePacket.Build());
        mDedicatedServerManager.KickPlayer(sessionID, 5.0f);
    }

    public void StopServer(string stopReason)
    {
        StartCoroutine(stopServerAndQuitApplication(stopReason));
    }

    private IEnumerator stopServerAndQuitApplication(string stopReason)
    {
        var sessionIDs = UserSessionData.SessionSlots.GetConnectedSessionIDs();

        foreach (var sessionID in sessionIDs)
        {
            DenySession(sessionID, stopReason);
        }

        yield return new WaitForSeconds(0.5f);
        DedicatedServerManager.Instance.StopServer();
    }

    #endregion

    #region Handle session command

    public void HandleSessionCommand(int sessionID, Request requestPacket)
    {
        if (mDedicatedServerManager == null)
            return;

        if (!requestPacket.HasSessionRequestCommandData)
            return;

        var commandData = requestPacket.SessionRequestCommandData;

        Action<int, SessionRequestCommandData> handleProcess = commandData.Command switch
        {
            // Lobby
            RequestCommandHandle.kCommandLobby_BindUsername => handleLobby_BindUsername,
            RequestCommandHandle.kCommandLobby_SelectCharacter => handleLobby_SelectCharacter,
            RequestCommandHandle.kCommandLobby_ReadyState => handleLobby_ReadyState,
            RequestCommandHandle.kCommandLobby_StartAsSquadLeader => handleLobby_StartAsSquadLeader,

            // Scene Changing
            RequestCommandHandle.kCommandSceneChanging_SceneLoadComplete => handleSceneChanging_SceneLoadComplete,
            RequestCommandHandle.kCommandSceneChanging_ReadyToStart => handleSceneChanging_ReadyToStart,

            // Chatting
            RequestCommandHandle.kCommandChat_SendMessage => handleChat_RequestChatMessage,

            // Cheatting
            RequestCommandHandle.kCheatRequest => handleCheat,

            _ => errorHandle
        };

        handleProcess.Invoke(sessionID, commandData);

        void errorHandle(int fromSession, SessionRequestCommandData commandData)
        {
            Debug.LogError(LogManager.GetLogMessage($"Cannot handle {commandData.Command} packet from session : {fromSession}.", NetworkLogType.MasterServer, true));
        }
    }

    private void handleCheat(int sessionID, SessionRequestCommandData commandData)
    {
        if (!commandData.HasRequestCheatCode)
        {
            return;
        }

        var cheatCode = commandData.RequestCheatCode;

        switch (cheatCode)
        {
            case CheatCodeType.kNone:
                break;

            case CheatCodeType.kRegenHp:
                if (ServerPlayerCharacterManager.TryGetInstance(out var playerManager))
                {
                    playerManager.TryRegenHp(sessionID);
                }
                break;

            default:
                break;
        }
    }

    private void handleLobby_BindUsername(int sessionID, SessionRequestCommandData commandData)
    {
        if (!commandData.HasUsername)
        {
            Debug.Log(LogManager.GetLogMessage($"There is no username on packet sent by session {sessionID}!", NetworkLogType.ServerSessionManager, true));
            return;
        }

        if (UserSessionData.SessionSlots.TryGetSlot(sessionID, out var slot))
        {
            slot.Username.Value = commandData.Username;
        }
    }

    private void handleLobby_SelectCharacter(int sessionID, SessionRequestCommandData commandData)
    {
        // TODO : 테스트 모드일 때는 난입 허용
        if ((CurrentServerState != ServerState.Lobby) ||
            (!commandData.HasSelectedCharacter) ||
            (!UserSessionData.SessionSlots.TryGetSlot(sessionID, out var slot)))
        {
            sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_CannotSelectCharacter);
            return;
        }

        var currentSessionState = slot.SessionState.Value;
        var selectedCharacter = commandData.SelectedCharacter;
        bool isAlreadySelectedCharacter = UserSessionData.SessionSlots.IsSelected(selectedCharacter);

        switch (currentSessionState)
        {
            case UserSessionState.Lobby_NotSelected:

                if (!selectedCharacter.IsPlayerEntity())
                {
                    sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_WrongCharacterType);
                    return;
                }

                if (isAlreadySelectedCharacter)
                {
                    sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_AlreaySelectedCharacter);
                    return;
                }

                slot.SelectedCharacterType.Value = selectedCharacter;
                slot.SessionState.Value = UserSessionState.Lobby_Selected;
                sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_CharacterSelected);
                break;

            case UserSessionState.Lobby_Selected:

                if (!selectedCharacter.IsPlayerEntity())
                {
                    sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_WrongCharacterType);
                    return;
                }

                // Unselect character, When user select same character
                if (selectedCharacter == EntityType.kNoneEntityType || selectedCharacter == slot.SelectedCharacterType.Value)
                {
                    slot.SelectedCharacterType.Value = EntityType.kNoneEntityType;
                    slot.SessionState.Value = UserSessionState.Lobby_NotSelected;
                    sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_UnselectCharacter);
                    return;
                }

                if (isAlreadySelectedCharacter)
                {
                    sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_AlreaySelectedCharacter);
                    return;
                }

                // Change selection
                slot.SelectedCharacterType.Value = selectedCharacter;
                sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_ChangeCharacter);
                break;

            case UserSessionState.Lobby_ReadyToStart:
                sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_CannotSelectCharacterWhenReady);
                break;

            default:
                sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_CannotSelectCharacter);
                break;
        }
    }

    private void handleLobby_ReadyState(int sessionID, SessionRequestCommandData commandData)
    {
        if ((CurrentServerState != ServerState.Lobby) ||
            (!commandData.HasReadyState) ||
            (!UserSessionData.SessionSlots.TryGetSlot(sessionID, out var slot)))
        {
            sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_CannotReady);
            return;
        }

        bool isReady = commandData.ReadyState;
        var currentSessionState = slot.SessionState.Value;

        // Client trying to ready
        if (isReady)
        {
            switch (currentSessionState)
            {
                case UserSessionState.Lobby_NotSelected:
                    sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_CannotReadyUntilSelectCharacter);
                    break;

                case UserSessionState.Lobby_Selected:
                    slot.SessionState.Value = UserSessionState.Lobby_ReadyToStart;
                    sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_ReadyToStart);
                    break;

                case UserSessionState.Lobby_ReadyToStart:
                    // Do nothing
                    break;

                default:
                    sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_CannotReady);
                    break;
            }
        }
        // Client trying to unready
        else
        {
            switch (currentSessionState)
            {
                case UserSessionState.Lobby_NotSelected:
                case UserSessionState.Lobby_Selected:
                    // Do nothing
                    break;

                case UserSessionState.Lobby_ReadyToStart:
                    slot.SessionState.Value = UserSessionState.Lobby_Selected;
                    sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_Unready);
                    break;

                default:
                    sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_CannotReady);
                    break;
            }
        }
    }

    private void handleLobby_StartAsSquadLeader(int sessionID, SessionRequestCommandData commandData)
    {
        if (CurrentServerState != ServerState.Lobby)
        {
            sendServerOperationResultToClient(sessionID, ServerOperationResult.Error);
            return;
        }

        if (!UserSessionData.SessionSlots.IsSquadLeader(sessionID))
        {
            sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_OnlySuqadLeaderCanStartTheGame);
            return;
        }

        if (!UserSessionData.SessionSlots.AreSelectCharacter())
        {
            sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_CannotStartUntilEveryUserSelectCharacter);
            return;
        }

        // 자기 자신을 제외한 모든 플레이어가 준비되었는 지를 검사합니다.
        if (UserSessionData.SessionSlots.AreReadyExcept(sessionID))
        {
            startLoadInGame();
            return;
        }
        else
        {
            sendServerOperationResultToClient(sessionID, ServerOperationResult.Lobby_SomeUserAreNotReady);
            return;
        }
    }

    private void handleSceneChanging_SceneLoadComplete(int sessionID, SessionRequestCommandData commandData)
    {
        Debug.Log(LogManager.GetLogMessage($"Client {sessionID} has been load the game map. Send world initial data to session!", NetworkLogType.ServerSessionManager));
        sendInitialDataToClient(sessionID);
    }

    private void handleSceneChanging_ReadyToStart(int sessionID, SessionRequestCommandData commandData)
    {
        if (UserSessionData.SessionSlots.TryGetSlot(sessionID, out var slot))
        {
            if (slot.SessionState.Value != UserSessionState.SceneChanging_Loading)
            {
                return;
            }

            slot.SessionState.Value = UserSessionState.SceneChanging_ReadyToStart;
        }

        // 모든 플레이어가 준비완료되었으면 게임을 시작
        if (UserSessionData.SessionSlots.AreSceneChanging_ReadyToStart())
        {
            StartCoroutine(StartTheGame());
        }
    }

    public IEnumerator StartTheGame()
    {
        yield return new WaitUntil(new Func<bool>(() => CurrentServerState == ServerState.SceneLoadCompleted));

        if (ServerPlayerCharacterManager.TryGetInstance(out var manager))
        {
            manager.OnPlayerReachCheckPoint(GameGlobalState.GameGlobalState.CheckPointNumber);
            Debug.Log(LogManager.GetLogMessage($"Game Start", NetworkLogType.ServerSessionManager));

            // 초기 데이터 재전송
            sendInitialDataToAll();
            UserSessionData.SessionSlots.UpdateAllSessionState(UserSessionState.InGame_Playing);
            sendCommandToAll(ResponseCommandHandle.kCommandStartGame);
            CurrentServerState = ServerState.GamePlayScene;

            if (!ServerConfiguration.IsJoinInGame)
            {
                ServerConfiguration.TriggerStartInitialCutscene = true;
            }

            ServerConfiguration.IsJoinInGame = true;
        }
        else
        {
            string message = $"There is no ServerPlayerCharacterManager!"; ;
            Debug.LogError(LogManager.GetLogMessage($"", NetworkLogType.ServerSessionManager, true));

            StopServer("Server doesn't have \"ServerPlayerCharacterManager\"!");
        }
    }

    private void handleChat_RequestChatMessage(int sessionID, SessionRequestCommandData commandData)
    {
        if (!commandData.HasChatMessage || !commandData.HasUsername)
        {
            return;
        }

        var responseCommandData = getResponseCommandDataBuilder(ResponseCommandHandle.kCommandChatMessage)
            .SetChatMessage(commandData.ChatMessage)
            .SetChatUsername(commandData.Username);
        var requestData = getResponseWithCommand(responseCommandData);

        mDedicatedServerManager.SendToAllClient_TCP(requestData.Build());
    }

    #endregion

    #region Session Operation

    public void BroadcasePooriScriptToClients(PooriScriptType pooriScriptType)
    {
        var commandData = getResponseCommandDataBuilder(ResponseCommandHandle.kCommandPooriScript)
            .SetPooriScriptCode((int)pooriScriptType);
        var responseData = getResponseWithCommand(commandData).Build();

        mDedicatedServerManager.SendToAllClient_TCP(responseData);
    }

    public void TryObtainCheckPointWeapon(int sessionID, int checkPointNumber)
    {
        if (!UserSessionData.SessionSlots.TryGetSlot(sessionID, out var slot))
        {
            return;
        }

        var checkPointSystem = GameGlobalState.GameGlobalState.CheckPointSystem;

        if (!checkPointSystem.TryGetCheckPointWeapon(checkPointNumber, out var checkPointWeaponItem))
        {
            return;
        }

        if (!slot.Inventory.HasEmptyWeaponSlot())
        {
            sendServerOperationResultToClient(sessionID, ServerOperationResult.Inventory_FullInventory);
            return;
        }

        if (slot.Inventory.HasWeapon(checkPointWeaponItem))
        {
            sendServerOperationResultToClient(sessionID, ServerOperationResult.Inventory_YouAlreadyHaveThisWeapon);
            return;
        }

        var pickedItem = GameGlobalState.GameGlobalState.CheckPointSystem.PickUpWeapon(checkPointNumber);
        TryObtainWeapon(sessionID, pickedItem);
    }

    public void setPlayerEquipWeapon(int sessionID)
    {
        if (ServerPlayerCharacterManager.TryGetInstance(out var manager))
        {
            if (UserSessionData.SessionSlots.TryGetEquipItem(sessionID, out var equipItem))
            {
                manager.SwapWeapon(sessionID, equipItem);
            }
        }
    }

    public bool TryObtainWeapon(int sessionID, ItemType weaponType)
    {
        if (CurrentServerState != ServerState.GamePlayScene)
        {
            return false;
        }

        var result = UserSessionData.SessionSlots.ObtainItemType(sessionID, weaponType);

        if (result == ServerOperationResult.Inventory_SuccessObtainWeapon)
        {
            setPlayerEquipWeapon(sessionID);

            // Send operation result to client
            var commandData = getResponseCommandDataBuilder(ResponseCommandHandle.kCommandServerOperationResult)
                .SetServerOperationResultCode((int)result)
                .SetObtainedItemType(weaponType);
            var responseData = getResponseWithCommand(commandData).Build();

            mDedicatedServerManager.SendToClient_TCP(sessionID, responseData);

            return true;
        }
        else
        {
            sendServerOperationResultToClient(sessionID, result);

            return false;
        }
    }

    public void DropWeapon(int sessionID)
    {
        if (CurrentServerState != ServerState.GamePlayScene)
        {
            return;
        }

        if (!ServerPlayerCharacterManager.TryGetInstance(out var playerManager))
        {
            return;
        }

        if (playerManager.TryGetPlayerEntityBySessionID(sessionID, out var playerEntity))
        {

            var slots = UserSessionData.SessionSlots;

            var result = slots.DropWeaponItem(sessionID, out var droppedItem);

            if (result == ServerOperationResult.Inventory_SuccessDropWeapon)
            {
                setPlayerEquipWeapon(sessionID);

                if (ItemObjectManager.TryGetInstance(out var manager))
                {
                    Vector3 spawnForce = playerEntity.transform.forward * ServerConfiguration.DropForcePower;
                    Vector3 itemPosition = playerEntity.Position.Value + ServerConfiguration.HumanoidWeaponDropPosition;

                    manager.CreateItemObjectAsMaster(droppedItem, itemPosition, Quaternion.identity, spawnForce);
                }

                // Send operation result to client
                var commandData = getResponseCommandDataBuilder(ResponseCommandHandle.kCommandServerOperationResult)
                    .SetServerOperationResultCode((int)result)
                    .SetDroppedWeaponItemType(droppedItem);
                var responseData = getResponseWithCommand(commandData);

                mDedicatedServerManager.SendToClient_TCP(sessionID, responseData.Build());
            }
        }
    }

    public void DropAllWeapon(int sessionID)
    {
        if (CurrentServerState != ServerState.GamePlayScene)
        {
            return;
        }

        var droppedWeaponList = UserSessionData.SessionSlots.DropAllWeaponItem(sessionID);

        if (!ServerPlayerCharacterManager.TryGetInstance(out var playerManager))
        {
            return;
        }

        if (playerManager.TryGetPlayerEntityBySessionID(sessionID, out var playerEntity))
        {
            if (!ItemObjectManager.TryGetInstance(out var itemObjectManager))
            {
                return;
            }

            Vector3 playerPosition = playerEntity.Position.Value;

            StartCoroutine(dropItem(itemObjectManager, droppedWeaponList, playerPosition));
        }
    }

    public IEnumerator dropItem(ItemObjectManager itemObjectManager, List<ItemType> droppedWeaponList, Vector3 playerPosition)
    {
        foreach (var droppedWeapon in droppedWeaponList)
        {
            yield return new WaitForSeconds(0.3f);
            Vector3 itemPosition = playerPosition + ServerConfiguration.HumanoidWeaponDropPosition;
            // Make random direction
            Vector3 forceDirection = (VectorExtension.GetRandomDirectionXZ() + Vector3.up).normalized;
            Vector3 spawnForce = forceDirection * ServerConfiguration.DropForcePower;
            itemObjectManager.CreateItemObjectAsMaster(droppedWeapon, itemPosition, Quaternion.identity, spawnForce);
        }
    }

    public void SwapWeapon(int sessionID, int weaponNumber)
    {
        if (!UserSessionData.SessionSlots.TryGetSlot(sessionID, out var slot))
        {
            return;
        }

        slot.Inventory.SwapWeapon(weaponNumber);
        setPlayerEquipWeapon(sessionID);
    }

    /// <summary>모든 무기를 드랍합니다.</summary>
    public void OnPlayerDie(int sessionID)
    {
        DropAllWeapon(sessionID);
    }

    public void OnPlayerReachCheckPoint(int checkPointIndex)
    {
        if (!GameGlobalState.GameGlobalState.IsValidCheckPointIndex(checkPointIndex))
        {
            return;
        }

        GameGlobalState.GameGlobalState.OnCheckPointReached(checkPointIndex);
        UserSessionData.SessionSlots.SaveAllInventory();

        if (ServerPlayerCharacterManager.TryGetInstance(out var manager))
        {
            manager.OnPlayerReachCheckPoint(checkPointIndex);

            if (checkPointIndex <= 1)
            {
                return;
            }

            // Give default weapon to unarmed players
            if (UserSessionData.SessionSlots.TryGetUnarmedPlayerIdQueue(out var unarmedPlayerIdQueue))
            {
                while (!unarmedPlayerIdQueue.IsEmpty())
                {
                    int unarmedPlayerSessionID = unarmedPlayerIdQueue.Dequeue();
                    TryObtainWeapon(unarmedPlayerSessionID, ServerConfiguration.PlayerDefaultWeapon);
                }
            }
        }
        else
        {
            Debug.LogError(LogManager.GetLogMessage($"There is no ServerPlayerCharacterManager", NetworkLogType.ServerSessionManager, true));
        }
    }

    public void RestartAtLsatCheckPoint()
    {
        if (CurrentServerState != ServerState.GamePlayScene)
        {
            return;
        }

        // TODO : Show gameover message
        StartCoroutine(restart());
    }

    private IEnumerator restart()
    {
        yield return new WaitForSeconds(ServerConfiguration.GameOverCutsceneDelay);

        // Provide default weapon if player doesn't have weapon
        //UserSessionData.SessionSlots.ProvideDefaultWeapon(ServerConfiguration.PlayerDefaultWeapon);
        GameGlobalState.GameGlobalState.OnGameRestart();

        ChangeMapTo(GameMapSceneName);
    }

    private bool mIsLoading = false;

    /// <summary>맵을 바꿉니다. 클라이언트에게도 맵 변경을 명령합니다.</summary>
    public void ChangeMapTo(string sceneName)
    {
        if (mIsLoading)
        {
            return;
        }

        CurrentServerState = ServerState.SceneLoading;

        // Send server scene loading command
        var commandData = getResponseCommandDataBuilder(ResponseCommandHandle.kCommandServerSceneLoading)
            .SetChangeMapName(sceneName);
        var responseData = getResponseWithCommand(commandData).Build();

        mDedicatedServerManager.SendToAllClient_TCP(responseData);

        UserSessionData.SessionSlots.UpdateAllSessionState(UserSessionState.SceneChanging_Loading);
        UserSessionData.SessionSlots.LoadAllInventory();

        mIsLoading = true;

        // TODO : Load and initiailzie server
        ServerWorldManager.Instance.TryLoadScene(sceneName, () =>
        {
            CurrentServerState = ServerState.SceneLoadCompleted;
            mIsLoading = false;
        });
    }

    /// <summary>게임을 불러옵니다.</summary>
    private void startLoadInGame()
    {
        if (CurrentServerState != ServerState.Lobby)
        {
            string message = $"The server isn't lobby state, but trying to start the game. State : {CurrentServerState}";
            Debug.LogError(LogManager.GetLogMessage(message, NetworkLogType.ServerSessionManager, true));

            string reason = GlobalTable.GetSystemMessageByResult(ServerOperationResult.ServerSceneLoad_MapLoadingFail);
            StopServer(reason);

            return;
        }

        // 기존 데이터를 초기화합니다.
        resetInGameData();

        // AI 유저에게 캐릭터를 바인딩합니다.
        UserSessionData.SessionSlots.GiveEntityTypeToAI();

        ChangeMapTo(GameMapSceneName);
    }

    private void sendServerOperationResultToClient(int sessionID, ServerOperationResult serverOperationResult)
    {
        // Print Debug Log
        string debugMessage = $"Session {sessionID} : {GlobalTable.GetSystemMessageByResult(serverOperationResult)}";
        Debug.Log(LogManager.GetLogMessage(debugMessage, NetworkLogType.ServerSessionManager));

        // Send operation result to client
        var commandData = getResponseCommandDataBuilder(ResponseCommandHandle.kCommandServerOperationResult)
            .SetServerOperationResultCode((int)serverOperationResult);
        var responseData = getResponseWithCommand(commandData);

        mDedicatedServerManager.SendToClient_TCP(sessionID, responseData.Build());
    }

    private void sendInitialDataToClient(int sessionID)
    {
        ServerMasterNetObjectManager.Instance?.SendInitialDataToClient(sessionID);
        ServerMasterEntityManager.Instance?.SendInitialEntitiesSpawnDataTo(sessionID);
        ServerMasterEntityManager.Instance?.SendInitialEntitiesStateDataTo(sessionID);

        if (LocatorEventManager.TryGetInstance(out var locatorEventManager))
        {
            locatorEventManager.SendInitialLocatorLifeDataTo(sessionID);
        }

        if (ItemObjectManager.TryGetInstance(out var itemObjectManager))
        {
            itemObjectManager.SendInitialItemObjectLifeDataTo(sessionID);
        }

        sendCommandTo(sessionID, ResponseCommandHandle.kCommandResponseInitialData);
    }

    private void sendInitialDataToAll()
    {
        foreach (int sessionID in UserSessionData.SessionSlots.GetConnectedSessionIDs())
        {
            ServerMasterNetObjectManager.Instance?.SendInitialDataToClient(sessionID);
            ServerMasterEntityManager.Instance?.SendInitialEntitiesSpawnDataTo(sessionID);
            ServerMasterEntityManager.Instance?.SendInitialEntitiesStateDataTo(sessionID);

            if (LocatorEventManager.TryGetInstance(out var locatorEventManager))
            {
                locatorEventManager.SendInitialLocatorLifeDataTo(sessionID);
            }

            if (ItemObjectManager.TryGetInstance(out var itemObjectManager))
            {
                itemObjectManager.SendInitialItemObjectLifeDataTo(sessionID);
            }
        }
    }

    private void sendCommandToAll(ResponseCommandHandle commandHandle)
    {
        var commandData = getResponseCommandDataBuilder(commandHandle);
        var responseData = getResponseWithCommand(commandData).Build();

        mDedicatedServerManager.SendToAllClient_TCP(responseData);
    }

    private void sendCommandTo(int sessionID, ResponseCommandHandle commandHandle)
    {
        var commandData = getResponseCommandDataBuilder(commandHandle);
        var responseData = getResponseWithCommand(commandData).Build();

        mDedicatedServerManager.SendToClient_TCP(sessionID, responseData);
    }

    #endregion
}
