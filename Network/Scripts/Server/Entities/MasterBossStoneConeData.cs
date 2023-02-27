using Network.Packet;
using System;
using System.Collections;
using UnityEngine;
using Utils;

namespace Network.Server
{
    [Serializable]
    public class BossAttackInfo
    {
        public DetectorType Detector;
        public GameObject KnockbackSupporter;
        public int Damage;
        public float KnockbackRadius = 3.0f;
    }

    public class MasterBossStoneConeData : MasterEntityData
    {
        [SerializeField] private float CreatedDelay = 0.1f;
        [SerializeField] private float DestroyedDelay = 1.5f;
        [SerializeField] private BossAttackInfo DetectorOnCreated;
        [SerializeField] private BossAttackInfo DetectorOnDestroyed;

        [SerializeField] private float SelfDestroyTime = 2;

        private CoroutineWrapper mWrapperCoroutine;

        protected override void Awake()
        {
            base.Awake();

            if (DetectorOnDestroyed.Detector.IsDetector())
            {
                mOnDestroyed += onDetectorDestroy;
            }

            if (mWrapperCoroutine == null)
            {
                mWrapperCoroutine = new CoroutineWrapper(CoroutineWrapper.CoroutineRunner.Instance);
            }

            mOnCreated += ()=> { mWrapperCoroutine.StartSingleton(destroySelfWithDelay()); };
        }

        private IEnumerator destroySelfWithDelay()
        {
            yield return new WaitForSeconds(SelfDestroyTime);
            ActionDie();
        }

        private void onDetectorDestroy()
        {
            if (mWrapperCoroutine.IsPlaying)
            {
                mWrapperCoroutine.Stop();
            }

            if (DetectorOnDestroyed.Detector.IsDetector())
            {
                createDetector(DetectorOnDestroyed, DestroyedDelay);
            }
        }

        public void ActionInitialAttack()
        {
            if (DetectorOnCreated != null)
            {
                createDetector(DetectorOnCreated, CreatedDelay);
            }
        }

        private void createDetector(BossAttackInfo attackInfo, float knockbackDelay)
        {
            if (ServerMasterDetectorManager.TryGetInstance(out var manager))
            {
                var baseInfo = getBaseDetectorInfo(attackInfo.Damage);

                manager.CreateNewDetector(attackInfo.Detector, baseInfo);

                if (attackInfo.KnockbackSupporter != null)
                {
                    var go = Instantiate(attackInfo.KnockbackSupporter, baseInfo.Origin, Quaternion.identity);
                    var knockback = go.GetComponent<KnockbackSupporter>();
                    knockback.Initialize(knockbackDelay, attackInfo.KnockbackRadius);

                }
            }
        }

        private DetectorInfo getBaseDetectorInfo(int damage)
        {
            var damageInfo = new DamageInfo()
            {
                AttacterFaction = this.FactionType,
                damage = damage
            };

            return new DetectorInfo()
            {
                OwnerCollider = mOwnerCollider,
                Origin = Position.Value,
                Direction = Vector3.up,
                RawViewVector = Vector3.up,
                OwnerEntityID = EntityID,
                DamageInfo = damageInfo
            };
        }
    }
}
