using Network;
using Network.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[Serializable]
public class WaveEventInfo
{
    public int WaveCount = 0;
    public BaseLocationEventTrigger EventTrigger;
}


public class BridgeWaveEventManager : LocalSingleton<BridgeWaveEventManager>
{
    [SerializeField] private List<WaveEventInfo> WaveEventTriggers = new();

    [SerializeField] private BridgeWaveOnEvent mBridgeController;

    [SerializeField] private BaseLocationEventTrigger mWaveStartTrigger;
    [SerializeField] private BaseLocationEventTrigger mWaveEndTrigger;

    [SerializeField] private float mDelayBetweenWave = 1.0f;

    [SerializeField] private List<WaveEvent> mWaveInfoList;
    [field: SerializeField] public int CurrentWaveIndexer { get; private set; } = 0;
    [field: SerializeField] public int CurrentWaveCounter { get; private set; } = 0;

    private Coroutine mWaveCoroutine;

    private NetworkMode mNetworkMode;

    private bool mIsWaveOn = false;

    public void InitializeByManager(NetworkMode networkMode)
    {
        if (mWaveCoroutine != null)
        {
            StopCoroutine(mWaveCoroutine);
        }

        mNetworkMode = networkMode;

        if (mNetworkMode == NetworkMode.Master)
        {
            foreach (var waveEvent in mWaveInfoList)
            {
                waveEvent.InitializeByManager(mNetworkMode);
                waveEvent.OnFinished += WaveEvent_OnFinished;
            }
        }
    }

    public void FixedUpdate()
    {
        if (mNetworkMode == NetworkMode.Remote)
        {
            int bridgeWave = ClientSessionManager.Instance.GameGlobalState.GameGlobalState.BridgeWaveCounter.Value;

            if (bridgeWave >= 0)
            {
                mBridgeController.BridgeWaveOn(bridgeWave);
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        stopWaveCoroutine();
    }

    private void OnDisable()
    {
        stopWaveCoroutine();
    }

    private void WaveEvent_OnFinished()
    {
        if (mNetworkMode != NetworkMode.Master)
        {
            return;
        }

        if (!mIsWaveOn)
        {
            return;
        }

        CurrentWaveIndexer++;
        CurrentWaveIndexer = CurrentWaveIndexer % mWaveInfoList.Count;

        mWaveCoroutine = StartCoroutine(startWave(mDelayBetweenWave));

        // 다리 올라오게 하기
        mBridgeController.BridgeWaveOn(CurrentWaveCounter);
        ServerSessionManager.Instance.GameGlobalState.GameGlobalState.SetBridgeWaveCounter(CurrentWaveCounter);

        // 이벤트 발동
        foreach (var e in WaveEventTriggers)
        {
            if (e.WaveCount == CurrentWaveCounter)
            {
                e.EventTrigger.TriggeredEvent(null);
            }
        }

        CurrentWaveCounter++;

        // Stop wave
        if (CurrentWaveCounter >= ServerConfiguration.MaxBridgeWaveCountIndexer)
        {
            endWave();
        }

        IEnumerator startWave(float delay)
        {
            yield return new WaitForSeconds(delay);
            mWaveInfoList[CurrentWaveIndexer].StartWave();
        }
    }

    public void StartWave()
    {
        if (mNetworkMode != NetworkMode.Master)
        {
            return;
        }

        mIsWaveOn = true;
        mWaveStartTrigger?.TriggeredEvent(null);
        stopWaveCoroutine();
        mWaveInfoList[0].StartWave();
    }

    private void endWave()
    {
        if (mNetworkMode != NetworkMode.Master)
        {
            return;
        }

        mIsWaveOn = false;
        mWaveEndTrigger?.TriggeredEvent(null);
        CurrentWaveIndexer = 0;

        //foreach (var w in mWaveInfoList)
        //{
        //    w.OnRemoveExsitEntities();
        //}

        stopWaveCoroutine();
    }

    private void stopWaveCoroutine()
    {
        if (mWaveCoroutine != null)
        {
            StopCoroutine(mWaveCoroutine);
        }
    }
}
