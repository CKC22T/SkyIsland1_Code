using Network;
using Network.Packet;
using Network.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

[Serializable]
public class WaveSpawnInfo
{
    public EntityType EntityType;
    public FactionType FactionType = FactionType.kEnemyFaction_1;
    public Transform Transform;
    public float SpawnTime;
}

public class WaveEvent : MonoBehaviour
{
    [SerializeField] private List<WaveSpawnInfo> mWaveSpawnInfoList;

    public event Action OnFinished;

    private NetworkMode mNetworkMode = NetworkMode.None;

    private bool mIsSpawning = false;
    private bool mIsWaving = false;
    private float mSpawnEndTime = 0;

    private List<int> mCreatedEntities = new List<int>();

    public void InitializeByManager(NetworkMode networkMode)
    {
        mNetworkMode = networkMode;

        foreach (var waveSpawnInfo in mWaveSpawnInfoList)
        {
            mSpawnEndTime = (mSpawnEndTime < waveSpawnInfo.SpawnTime) ? waveSpawnInfo.SpawnTime : mSpawnEndTime;
        }
    }

    public void StartWave()
    {
        if (mNetworkMode == NetworkMode.Remote)
            return;

        mIsWaving = true;
        mIsSpawning = true;

        foreach (var waveSpawnInfo in mWaveSpawnInfoList)
        {
            StartCoroutine(spawnEntity(waveSpawnInfo));
        }

        StartCoroutine(waitFor(mSpawnEndTime));
    }

    public IEnumerator spawnEntity(WaveSpawnInfo info)
    {
        yield return new WaitForSeconds(info.SpawnTime);

        if (mIsWaving && ServerMasterEntityManager.TryGetInstance(out var entityManager))
        {
            var entity = entityManager.CreateNewEntity(
                info.EntityType,
                info.FactionType,
                info.Transform.position,
                info.Transform.rotation,
                true,
                OnEnemyDestroy);

            if (info.EntityType == EntityType.kWisp)
            {
                var wispEntity =  entity as MasterHumanoidEntityData;
                wispEntity.ActionForceEquipWeapon(ServerConfiguration.WispDefaultWeapon);
            }

            mCreatedEntities.Add(entity.EntityID);
        }
    }

    public IEnumerator waitFor(float waitTime)
    {
        yield return new WaitForSeconds(waitTime + 1f);
        mIsSpawning = false;

        checkIfEndWave();
    }

    public void OnEnemyDestroy(int entityID)
    {
        if (!mIsWaving)
        {
            mCreatedEntities.Clear();
            return;
        }

        if (!mCreatedEntities.Contains(entityID))
            return;

        mCreatedEntities.Remove(entityID);

        if (mIsSpawning)
            return;

        checkIfEndWave();
    }

    public void OnRemoveExsitEntities()
    {
        if (!ServerMasterEntityManager.TryGetInstance(out var manager))
        {
            mCreatedEntities.Clear();
        }

        for (int i = mCreatedEntities.Count - 1; i >= 0; i--)
        {
            manager.KillEntity(mCreatedEntities[i]);
        }

        mCreatedEntities.Clear();
    }

    private void checkIfEndWave()
    {
        if (mCreatedEntities.IsEmpty())
        {
            OnFinished?.Invoke();
            mIsWaving = false;
        }
    }
}
