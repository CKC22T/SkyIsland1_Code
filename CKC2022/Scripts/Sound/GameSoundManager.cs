using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Utils;
using UnityEngine.Audio;

namespace CKC2022
{
    [Serializable]
    public class SoundPlayData
    {
        public float volume = 1.0f;
        public float pitch = 1.0f;
        public bool loop = false;

        public Vector3 Position;

        public SoundPlayData() { }
        
        public SoundPlayData(in Vector3 position) { this.Position = position; }


        public static readonly SoundPlayData Default = new SoundPlayData();
    }

    [Serializable]
    public class BGMLoader
    {
        [SerializeField]
        private AudioMixerGroup group;

        [SerializeField]
        private AudioSource swapSource0;

        [SerializeField]
        private AudioSource swapSource1;

        private int index = -1;

        [SerializeField]
        private float FadeTime;

        [SerializeField]
        private AnimationCurve OutCurve;

        [SerializeField]
        private AnimationCurve InCurve;

        private CoroutineWrapper fadeRoutine;
        public AudioSource CurrentSource { get => index == 0 ? swapSource0 : swapSource1; }
        public AudioSource AuxSource { get => index == 0 ? swapSource1 : swapSource0; }

        public void Initialize(MonoBehaviour fadeRunner)
        {
            fadeRoutine = new CoroutineWrapper(fadeRunner);

            index = 0;
        }

        public void Play(SoundConfiguration config)
        {
            AuxSource.SetupConfiguration(config, group, SoundPlayData.Default);

            fadeRoutine.StartSingleton(Fading(CurrentSource, AuxSource));
        }

        private IEnumerator Fading(AudioSource outSource, AudioSource inSource)
        {
            float t = 0;
            float outDefaultVolume = outSource?.volume ?? 0;
            float inDefaultVolume = inSource?.volume ?? 0;

            inSource.volume = 0;
            inSource.Play();
            
            while (t < FadeTime)
            {
                if (outSource != null)
                    outSource.volume = outDefaultVolume * OutCurve.Evaluate(t / FadeTime);

                if (inSource != null)
                    inSource.volume = inDefaultVolume * InCurve.Evaluate(t / FadeTime);

                t += Time.deltaTime;
                yield return null;
            }

            outSource.Stop();
            index = (index + 1) % 2;
        }
    }

    public class GameSoundManager : LocalSingleton<GameSoundManager>
    {
        [SerializeField]
        private AudioSource Origin;

        [SerializeField]
        private AudioMixer mixer;

        [Header("Background")]
        [SerializeField]
        private BGMLoader BGM;
        
        [SerializeField]
        private List<SoundConfiguration> Sounds = new List<SoundConfiguration>();

        private readonly Dictionary<SoundType, SoundConfiguration> SoundByType = new Dictionary<SoundType, SoundConfiguration>();

        private readonly List<AudioSource> SourceInstances = new List<AudioSource>();
        
        private readonly List<AudioSource> LoopingSources = new List<AudioSource>();

        public bool TryGetConfig(SoundType type, out SoundConfiguration config)
        {
            return SoundByType.TryGetValue(type, out config);
        }

        protected override void Initialize()
        {
            base.Initialize();

            BGM.Initialize(this);
            
            DontDestroyOnLoad(gameObject);

            foreach (var sound in Sounds)
            {
                SoundByType.Add(sound.type, sound);
            }

            PoolManager.WarmPool(Origin.gameObject, 20);
        }

        private AudioSource PlayInternal(SoundType type, SoundPlayData data = null)
        {
            if (data == null)
                data = SoundPlayData.Default;

            var config = SoundByType[type];
            var loop = config.loop || data.loop;

            var collection = loop ? LoopingSources : SourceInstances;

            var instance = collection.GetInstance(Origin.gameObject);

            var group = true switch
            {
                true when type.IsBGM() => mixer.FindMatchingGroups("BGM")[0],
                true when type.IsEffect() => mixer.FindMatchingGroups("SE")[0],
                true when type.IsAmbient() => mixer.FindMatchingGroups("Env")[0],
                true when type.IsUIEffect() => mixer.FindMatchingGroups("UI")[0],

                _ => mixer.FindMatchingGroups("Master")[0]
            };

            instance.transform.position = data.Position;
            instance.SetupConfiguration(config, group, data).Play();

            return instance;
        }

        private void StopInternal(SoundType type)
        {
            LoopingSources.FindAll((source) => source.clip == SoundByType[type].clip)
                .ForEach((source) => source.Stop());
        }

        public static AudioSource Play(SoundType type, SoundPlayData data = null)
        {
            if (!TryGetInstance(out var instance))
            {
                Debug.LogError("GameSoundManager is not initialized");
                return null;
            }

            if (type == SoundType.None)
                return null;


            return instance.PlayInternal(type, data);
        }
        
        public static void PlayBGM(SoundType type)
        {
            if (!TryGetInstance(out var instance))
                throw new NullReferenceException("GameSoundManager is not initialized");

            if (!type.IsBGM())
                return;

            instance.TryGetConfig(type, out var config);
            instance.BGM.Play(config);
        }

        public static void Stop(SoundType type)
        {
            if (!TryGetInstance(out var instance))
                return;

            instance.StopInternal(type);
        }

    }

    public static class GameSoundManagerExtention
    {
        public static bool IsBGM(this SoundType type)
        {
            return 0 < (int)type && (int)type < 101;
        }
        
        public static bool IsEffect(this SoundType type)
        {
            return 100 < (int)type  && (int)type < 401;
        }

        public static bool IsAmbient(this SoundType type)
        {
            return 400 < (int)type && (int)type < 501;
        }
        
        public static bool IsUIEffect(this SoundType type)
        {
            return 500 < (int)type && (int)type < 601;
        }

        public static AudioSource SetupConfiguration(this AudioSource target, SoundConfiguration config, AudioMixerGroup group, SoundPlayData data)
        {
            target.clip = config.clip;
            target.volume = config.volume * data.volume;
            target.pitch = config.pitch * data.pitch;
            target.loop = config.loop || data.loop;
            target.outputAudioMixerGroup = group;

            if (config.pitchRandomMultiplier != 1)
            {
                if (UnityEngine.Random.value < .5)
                    target.pitch *= UnityEngine.Random.Range(1 / config.pitchRandomMultiplier, 1);
                else
                    target.pitch *= UnityEngine.Random.Range(1, config.pitchRandomMultiplier);
            }

            return target;
        }
        
        public static AudioSource GetInstance(this List<AudioSource> list, GameObject origin)
        {
            var source = list.Find((source) => !source.isPlaying);
            if (source == null)
            {
                var instance = PoolManager.SpawnObject(origin);
                instance.transform.SetParent(GameSoundManager.Instance.transform);
                
                source = instance.GetComponent<AudioSource>();
                list.Add(source);
            }
            return source;
        }
    }
}