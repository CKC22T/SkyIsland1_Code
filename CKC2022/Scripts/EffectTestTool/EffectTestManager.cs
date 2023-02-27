using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace CKC2022.Test
{
    public class EffectTestManager : MonoBehaviour
    {
        [SerializeField]
        private Transform Start;

        [SerializeField]
        private ParticleSystem MuzzleOrigin;
        [SerializeField]
        private ParticleSystem ProjectileOrigin;

        [SerializeField]
        private ParticleSystem ExplosionOrigin;

        [SerializeField]
        private float moveSpeed = 2f;

        [SerializeField]
        private float destroyTime = 1f;

        [Range(0.01f, 3f)]
        [SerializeField]
        private float RepeatTime = 1f;

        private CoroutineWrapper generatorLoop;
        private CoroutineWrapper wrapper;

        //private List<CoroutineWrapper> wrapperPool = new List<CoroutineWrapper>();
        //private CoroutineWrapper wrapper
        //{
        //    get
        //    {
        //        var target = wrapperPool.FirstOrDefault(wrapper => !wrapper.IsPlaying);
        //        if (target == null)
        //        {
        //            target = new CoroutineWrapper(this);
        //            wrapperPool.Add(target);
        //        }
        //        return target;
        //    }
        //}

        private void Awake()
        {
            generatorLoop = new CoroutineWrapper(this);
            wrapper = new CoroutineWrapper(this);

            PoolManager.OnInitialized += PoolManager_OnInitialized;
        }

        private void PoolManager_OnInitialized(PoolManager poolManager)
        {
            poolManager.root = transform;
        }

        public void OnEnable()
        {
            generatorLoop.StartSingleton(Run());
            //wrapper.StartSingleton(Run());
        }


        private IEnumerator Run()
        {
            while (enabled)
            {
                SpawnEffect();

                yield return new WaitForSeconds(RepeatTime);
            }
        }

        private void SpawnEffect()
        {
            RunMuzzleEffect();

            if (ProjectileOrigin == null)
                return;

            var instance = PoolManager.SpawnObject(ProjectileOrigin.gameObject);

            if (!instance.TryGetComponent<EffectAutoRelease>(out var autoRelease))
                instance.AddComponent<EffectAutoRelease>();

            wrapper.Start(MoveProjectile(instance));
        }

        private void RunMuzzleEffect()
        {
            if (MuzzleOrigin == null)
                return;

            var instance = PoolManager.SpawnObject(MuzzleOrigin.gameObject, Start.transform.position, Start.transform.rotation);

            if (!instance.TryGetComponent<EffectAutoRelease>(out var autoRelease))
                instance.AddComponent<EffectAutoRelease>();
        }

        private IEnumerator MoveProjectile(GameObject projectile)
        {
            var runtime = destroyTime;
            float t = 0;
            var start = Start.transform.position;
            var destination = Start.transform.position + destroyTime * moveSpeed * Start.forward;
            var cachedRotation = Start.rotation;

            while (t < runtime)
            {
                projectile.transform.position = Vector3.Lerp(start, destination, t / runtime);
                projectile.transform.rotation = cachedRotation;
                t += Time.fixedDeltaTime;
                yield return YieldInstructionCache.WaitForFixedUpdate;
            }

            projectile.SetActive(false);

            if (ExplosionOrigin == null)
                yield break;

            var explosion = PoolManager.SpawnObject(ExplosionOrigin.gameObject, destination, Quaternion.identity);
            if (!explosion.TryGetComponent<EffectAutoRelease>(out var autoRelease))
                explosion.AddComponent<EffectAutoRelease>();

        }


    }
}