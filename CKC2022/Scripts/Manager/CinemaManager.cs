using CKC2022.Input;
using Network;
using Network.Client;
using Network.Packet;
using Network.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using Utils;

public enum CinemaType
{
    None,
    Starting = 10,
    Boss = 20,
    Ending = 30,
}

[Serializable]
public class CinemaInfo
{
    public CinemaType CinemaType;
    public GameObject CinemaObject;
    public PlayableDirector timeline;
    public bool ShouldLockInput;
    public bool ShouldPlayerImmortal;
    public List<BaseLocationEventTrigger> OnStart = new();
    public List<BaseLocationEventTrigger> OnEnd = new();
}

public class CinemaManager : LocalSingleton<CinemaManager>
{
    [SerializeField] public List<CinemaInfo> CinemaList;

    private Dictionary<CinemaType, CinemaInfo> mCinemaTable = new Dictionary<CinemaType, CinemaInfo>();

    private Coroutine mPlayRoutine;

    public bool TryGetCinemaPlayTime(CinemaType cinemaType, out double playTime)
    {
        if (mCinemaTable.TryGetValue(cinemaType, out var cinemaInfo))
        {
            playTime = cinemaInfo.timeline.duration;
            return true;
        }

        playTime = 0;
        return false;
    }

    private CoroutineWrapper Wapper;

    public void Start()
    {
        if (Wapper == null)
        {
            Wapper = new CoroutineWrapper(CoroutineWrapper.CoroutineRunner.Instance);
        }

        if (ServerConfiguration.IS_CLIENT)
        {
            ClientHandler.OnCinemaCall += ClientHandler_OnCinemaCall;
            Wapper.StartSingleton(startClientInitialCutscene());
        }
        else if (ServerConfiguration.IS_SERVER)
        {
            Wapper.StartSingleton(startServerInitialCutscene());
        }

        foreach (var c in CinemaList)
        {
            c.CinemaObject.SetActive(false);
            mCinemaTable.Add(c.CinemaType, c);
        }
    }

    private IEnumerator startClientInitialCutscene()
    {
        yield return new WaitUntil(()=>ServerConfiguration.TriggerStartInitialCutscene);
        ServerConfiguration.TriggerStartInitialCutscene = false;
        ForcePlayCinemaOnRemote(CinemaType.Starting);
    }

    private IEnumerator startServerInitialCutscene()
    {
        yield return new WaitUntil(() => ServerConfiguration.TriggerStartInitialCutscene);
        ServerConfiguration.TriggerStartInitialCutscene = false;
        PlayCinemaOnMaster(CinemaType.Starting);
    }

    private void ClientHandler_OnCinemaCall(string cinemaName)
    {
        if (Enum.TryParse<CinemaType>(cinemaName, out var cinemaType))
        {
            ForcePlayCinemaOnRemote(cinemaType);
        }
        else
        {
            Debug.LogError(LogManager.GetLogMessage($"Cinema type parse error! Type name : {cinemaName}", NetworkLogType.CinemaManager, true));
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (ServerConfiguration.IS_CLIENT)
        {
            ClientHandler.OnCinemaCall -= ClientHandler_OnCinemaCall;
        }

        if (mPlayRoutine != null)
        {
            StopCoroutine(mPlayRoutine);
        }
    }

    /// <summary>원격으로 시네마를 실행합니다.</summary>
    public void RemotePlayCinema(CinemaType cinemaType)
    {
        if (DedicatedServerManager.TryGetInstance(out var server))
        {
            var packet = server.GetBaseResponseBuilder(ResponseHandle.kRemotePlayCinema)
                .SetRemotePlayCinemaName(cinemaType.ToString()).Build();

            server.SendToAllClient_TCP(packet);
        }

        PlayCinemaOnMaster(cinemaType);
    }

    /// <summary>클라이언트 측에서 강제로 시네마를 실행합니다.</summary>
    public void ForcePlayCinemaOnRemote(CinemaType cinemaType)
    {
        if (cinemaType == CinemaType.Boss)
        {
            CKC2022.GameSoundManager.PlayBGM(CKC2022.SoundType.Boss_BackGround);
        }
        else if (cinemaType == CinemaType.Ending)
        {
            CKC2022.GameSoundManager.PlayBGM(CKC2022.SoundType.End_BackGround);
        }

        if (!PlayerInputNetworkManager.TryGetAnyInputContainer(out var container))
        {
            Debug.LogError(LogManager.GetLogMessage($"There is no input container", NetworkLogType.CinemaManager, true));
            return;
        }

        if (!mCinemaTable.TryGetValue(cinemaType, out var cinema))
        {
            Debug.LogError(LogManager.GetLogMessage($"There is no cinema on the table. Cinema : {cinemaType}", NetworkLogType.CinemaManager, true));
            return;
        }

        mPlayRoutine = StartCoroutine(playCinemaOnRemote(container, cinema));
    }

    /// <summary>클라이언트 측에서 시네마를 진행합니다.</summary>
    private IEnumerator playCinemaOnRemote(InputContainer inputContainer, CinemaInfo info)
    {
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.Space))
        {
            yield break;
        }
#endif

        Debug.Log(LogManager.GetLogMessage($"Play cinema type : {info.CinemaType}", NetworkLogType.CinemaManager));

        // When cinema started.
        if (info.ShouldLockInput)
        {
            if (inputContainer.Input != null && inputContainer.Input.playerInput.enabled)
                inputContainer.Input.SetInputActiveState(true);
        }

        info.CinemaObject.SetActive(true);
        var cameraData = Camera.main.GetUniversalAdditionalCameraData();
        cameraData.renderType = CameraRenderType.Overlay;

        foreach (var e in info.OnStart)
        {
            e.TriggeredEvent(null);
        }

        // Detect if cinema ended.
        //yield return new WaitForSeconds(1);
        //yield return new WaitUntil(() => (info.timeline.state == PlayState.Paused));
        yield return new WaitForSeconds((float)info.timeline.duration - ServerConfiguration.CinemaFadeoutDelay);

        if (info.ShouldLockInput)
        {
            if (inputContainer.Input != null && inputContainer.Input.playerInput.enabled)
                inputContainer.Input.SetInputActiveState(false);
        }

        foreach (var e in info.OnEnd)
        {
            e.TriggeredEvent(null);
        }

        yield return new WaitUntil(() => (info.timeline.state == PlayState.Paused));
        // When cinema ended.
        info.CinemaObject.SetActive(false);
        cameraData.renderType = CameraRenderType.Base;

        Debug.Log(LogManager.GetLogMessage($"End cinema type : {info.CinemaType}", NetworkLogType.CinemaManager));
    }

    /// <summary>서버측에서 시네마를 진행합니다.</summary>
    public void PlayCinemaOnMaster(CinemaType cinemaType)
    {
        if (!mCinemaTable.TryGetValue(cinemaType, out var cinema))
        {
            Debug.LogError(LogManager.GetLogMessage($"There is no cinema on the table. Cinema : {cinemaType}", NetworkLogType.CinemaManager, true));
            return;
        }

        mPlayRoutine = StartCoroutine(playCinemaOnMaster(cinema));
    }

    /// <summary>서버측에서 시네마를 진행합니다.</summary>
    private IEnumerator playCinemaOnMaster(CinemaInfo info)
    {
        Debug.Log(LogManager.GetLogMessage($"Play cinema on server / type : {info.CinemaType}", NetworkLogType.CinemaManager));

        // When cinema started.
        {
            if (ServerPlayerCharacterManager.TryGetInstance(out var playerManager))
            {
                if (info.ShouldPlayerImmortal)
                {
                    playerManager.SetPlayerImmortal(true);
                }

                if (info.ShouldLockInput)
                {
                    playerManager.LockPlayer(true);
                }
            }
        }

        foreach (var e in info.OnStart)
        {
            e.TriggeredEvent(null);
        }

        // Detect if cinema ended.
        //yield return new WaitForSeconds(1);
        //yield return new WaitUntil(() => (info.timeline.state == PlayState.Paused));
        yield return new WaitForSeconds((float)info.timeline.duration - ServerConfiguration.CinemaFadeoutDelay);

        {
            if (ServerPlayerCharacterManager.TryGetInstance(out var playerManager))
            {
                if (info.ShouldPlayerImmortal)
                {
                    playerManager.SetPlayerImmortal(false);
                }

                if (info.ShouldLockInput)
                {
                    playerManager.LockPlayer(false);
                }
            }
        }

        foreach (var e in info.OnEnd)
        {
            e.TriggeredEvent(null);
        }

        yield return new WaitUntil(() => (info.timeline.state == PlayState.Paused));

        Debug.Log(LogManager.GetLogMessage($"End cinema type : {info.CinemaType}", NetworkLogType.CinemaManager));
    }

}
