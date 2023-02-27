using System;
using System.Collections;
using UnityEngine;
using static Network.Packet.Response.Types;

namespace Network.Client
{
    [Serializable]
    public class StoneAttackParticle
    {
        public ParticleSystem ObjectParticle;
        public float SpawnAnimationTime;
    }

    public class ReplicatedBossStoneActor : MonoBehaviour
    {
        [SerializeField] private ReplicatedEntityData mEntityData;
        [SerializeField] private BossStoneEffectController mEffectController;


        private bool mIsPlayed = false;
        public void Awake()
        {
            mEntityData.OnAction += OnAction;
            mEffectController.effectEndCallback += onEndEffect;
        }

        public void OnEnable()
        {
            if (mIsPlayed)
            {
                return;
            }

            mIsPlayed = true;

            mEffectController.StoneSpawn();
        }

        private void OnAction(EntityActionData actionData)
        {
            if (!actionData.HasAction)
            {
                return;
            }

            switch (actionData.Action)
            {
                case EntityAction.kDestroy:
                case EntityAction.kDie:
                    mEffectController.StoneDestroy();
                    break;
            }
        }

        private void onEndEffect()
        {
            PoolManager.ReleaseObject(gameObject);
            mIsPlayed = false;
        }
    }
}
