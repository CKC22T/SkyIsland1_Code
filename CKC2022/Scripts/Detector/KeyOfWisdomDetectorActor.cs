using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace CKC2022
{
    public class KeyOfWisdomDetectorActor : MonoBehaviour
    {
        public class InnerActor
        {
            public Vector3 startPosition;
            public Vector3 samplePosition;
            public Vector3 endPosition;

            public Vector3 offset;
            public GameObject instance;

            public float StartTime;
            public float InitializeTime;
            public float EndTime;


            public AnimationCurve curve;

            public void InitializeSample(float factor,float minDistance, float maxDistance)
            {
                if (instance.TryGetComponent<ParticleSystem>(out var particle))
                    particle.Clear();

                var dir = endPosition - startPosition;
                var lookRot = Quaternion.LookRotation(dir);
                var axis = lookRot * (Quaternion.Euler(Random.Range(-3, 3) * 15, 0, 0) * Vector3.up);

                samplePosition = Vector3.Lerp(startPosition, endPosition, Random.Range(0.15f, 0.35f)) + axis * Mathf.Clamp(factor / dir.magnitude, minDistance, maxDistance);
            }

            public void SetOffset(float radius)
            {
                offset = new Vector3(Random.Range(-1f, 1f) * radius * 0.5f, 0.5f, Random.Range(-1f, 1f) * radius * 0.5f);
                endPosition += offset;
            }

            public void Update()
            {
                var t = (Time.time - InitializeTime) / (EndTime - InitializeTime);
                t = curve.Evaluate(t);

                instance.transform.position = Vector3.Lerp(Vector3.Lerp(startPosition, samplePosition, t), Vector3.Lerp(samplePosition, endPosition, t), t);
            }
        }

        [SerializeField]
        private bool isTest;

        [SerializeField]
        private Transform EffectRoot;

        [SerializeField]
        private DelayDetectorData data;

        [SerializeField]
        private KeyOfWisdomTest test;

        [SerializeField]
        private float minDistance = 5;
        
        [SerializeField]
        private float maxDistance = 15;

        [SerializeField]
        private int GenerateCount = 4;

        [SerializeField]
        private float spawnInterval = 0.25f;

        [SerializeField]
        private float Factor;

        [SerializeField]
        private AnimationCurve curve;

        [SerializeField]
        private ParticleSystem MoveEffect;

        [SerializeField]
        private ParticleSystem DestoryEffect;

        private readonly List<InnerActor> InnerActors = new();

        private float Distance { get => isTest ? test.Distance : data.Distance; }
        private float Runtime { get => isTest ? test.Delay : data.Delay; }

        private Vector3 StartPoint { get => isTest ? test.StartPosition : data.Owner.Position.Value; }
        private Vector3 TargetPoint { get => isTest ? test.TargetPoint : data.TargetPoint; }

        private CoroutineWrapper wrapper;

        private void Awake()
        {
            wrapper = new CoroutineWrapper(this);

            if (data != null)
            {
                data.OnHitscanStart += Data_OnHitscanStart;
                data.OnHitscanUpdate += Data_OnHitscanUpdate;
                data.OnHitscanEnd += Data_OnHitscanEnd;
            }

            if (test != null)
            {
                test.OnHitscanStart += Data_OnHitscanStart;
                test.OnHitscanUpdate += Data_OnHitscanUpdate;
                test.OnHitscanEnd += Data_OnHitscanEnd;
            }
        }

        private void OnEnable()
        {
            EffectRoot.transform.localScale = Vector3.one * Distance * 0.75f;
        }

        private void Data_OnHitscanStart()
        {
            wrapper.Start(MakeInstance());

            if (Physics.Raycast(new Ray(TargetPoint + Vector3.up, Vector3.down), out var hit, 3f, 1 << LayerMask.NameToLayer("Ground")))
            {
                EffectRoot.transform.position = hit.point + Vector3.up * 0.05f;
            }
            else
            {
                EffectRoot.transform.position = TargetPoint + Vector3.up * 0.05f;
            }
        }


        private IEnumerator MakeInstance()
        {
            float startTime = Time.time;
            for (int i = 0; i < GenerateCount; ++i)
            {
                var instance = PoolManager.SpawnObject(MoveEffect.gameObject, StartPoint, Quaternion.identity);

                var actor = new InnerActor()
                {
                    startPosition = StartPoint,
                    endPosition = TargetPoint,
                    instance = instance,

                    StartTime = startTime,
                    InitializeTime = Time.time,
                    EndTime = startTime + Runtime,

                    curve = curve
                };

                actor.InitializeSample(Factor, minDistance, maxDistance);
                actor.SetOffset(Distance);

                InnerActors.Add(actor);
                GameSoundManager.Play(SoundType.Weapon_Key, new SoundPlayData(StartPoint));

                yield return YieldInstructionCache.WaitForSeconds(spawnInterval);
            }

        }

        private void Data_OnHitscanUpdate(float t)
        {
            foreach (var actor in InnerActors)
            {
                actor.Update();
            }
        }

        private void Data_OnHitscanEnd()
        {
            foreach (var pair in InnerActors)
            {
                PoolManager.ReleaseObject(pair.instance);

                var instance = PoolManager.SpawnObject(DestoryEffect.gameObject, TargetPoint + pair.offset, Quaternion.identity);
                instance.transform.localScale = Vector3.one * Distance;
            }
            
            GameSoundManager.Play(SoundType.Weapon_Key_Blow, new SoundPlayData(TargetPoint));

            InnerActors.Clear();
        }
    }
}