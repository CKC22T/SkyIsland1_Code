using Network;
using Network.Server;
using Network.Packet;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using System.Collections;

public class BossSpawnEventTrigger_Master : BaseLocationEventTrigger
{
    public List<BaseLocationEventTrigger> BossDieEvent = new();
    public Transform BossPosition;
    public float BossCinemaDuration = 10.0f;
    private float mBossCreateDelay = 4.0f;
    private MasterSpriteEntityData mSpiriteEntity = null;

    private CoroutineWrapper mCoroutineWrapper;

    public void Awake()
    {
        if (mCoroutineWrapper == null)
        {
            mCoroutineWrapper = new CoroutineWrapper(CoroutineWrapper.CoroutineRunner.Instance);
        }
    }

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ServerConfiguration.IS_SERVER)
        {
            if (mSpiriteEntity != null)
            {
                return;
            }

            mCoroutineWrapper.StartSingleton(spawnBoss());
        }
    }

    private IEnumerator spawnBoss()
    {
        if (CinemaManager.TryGetInstance(out var cinemaManager))
        {
            if (cinemaManager.TryGetCinemaPlayTime(CinemaType.Boss, out var cinemaPlayTime))
            {
                BossCinemaDuration = (float)cinemaPlayTime;
            }
        }

        if (ServerPlayerCharacterManager.TryGetInstance(out var playerManager))
        {
            playerManager.LockPlayerAI(BossCinemaDuration - 0.5f);
        }

        yield return new WaitForSeconds(BossCinemaDuration - 0.5f);

        ServerSessionManager.Instance.GameGlobalState.GameGlobalState.BossBlockingRock.Value = true;

        if (ServerMasterEntityManager.TryGetInstance(out var entityManager))
        {
            mSpiriteEntity = entityManager.CreateNewEntity(
                EntityType.kSpirit,
                FactionType.kEnemyFaction_1,
                BossPosition.position,
                BossPosition.rotation,
                true) as MasterSpriteEntityData;

            mSpiriteEntity.SetDieTriggerEvents(BossDieEvent);
            mSpiriteEntity.ActionFreeze(0.3f);
        }
    }
}
