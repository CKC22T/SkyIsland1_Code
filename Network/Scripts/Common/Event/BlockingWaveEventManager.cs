using Network.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class BlockingWaveEventManager : LocalSingleton<BlockingWaveEventManager>
{
    [SerializeField] private BaseLocationEventTrigger mWaveStartTrigger;
    [SerializeField] private BaseLocationEventTrigger mWaveEndTrigger;

    [SerializeField] private float mDelayBetweenWave = 1.0f;

    [SerializeField] private Transform mWaveBlockStonePosition;
    [SerializeField] private List<WaveEvent> mWaveInfoList;
    [field : SerializeField] public int CurrentWave { get; private set; } = 0;

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

    protected override void OnDestroy()
    {
        base.OnDestroy();

        stopWaveCoroutine();
    }

    private void OnDisable()
    {
        stopWaveCoroutine();
    }

    public void CreateBlockingStone()
    {
        if (mNetworkMode == NetworkMode.Master)
        {
            ServerMasterEntityManager.Instance.CreateNewEntity(
                EntityType.kStructureWaveBlockingStone,
                FactionType.kNeutral,
                mWaveBlockStonePosition.position,
                mWaveBlockStonePosition.rotation,
                true,
                endWave);
        }
    }

    private void WaveEvent_OnFinished()
    {
        if (!mIsWaveOn)
        {
            return;
        }

        CurrentWave++;
        CurrentWave = CurrentWave % mWaveInfoList.Count;

        mWaveCoroutine = StartCoroutine(startWave(mDelayBetweenWave));

        IEnumerator startWave(float delay)
        {
            yield return new WaitForSeconds(delay);
            mWaveInfoList[CurrentWave].StartWave();
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

    private void endWave(int blockingStoneEntityID)
    {
        mIsWaveOn = false;
        mWaveEndTrigger?.TriggeredEvent(null);
        CurrentWave = 0;

        foreach (var w in mWaveInfoList)
        {
            w.OnRemoveExsitEntities();
        }

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
