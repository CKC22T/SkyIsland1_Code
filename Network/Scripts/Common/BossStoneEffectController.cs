using CKC2022;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class BossStoneEffectController : MonoBehaviour
{
    [SerializeField] private float effectTime = 5.0f;
    [SerializeField] private float spawnTime = 1.8f;
    private float effectSpeed { get => Mathf.Max((effectTime - bossStoneEffect.time) * proportion, 1); }

    [SerializeField]
    private ParticleSystem bossStoneEffect;

    [SerializeField]
    private float proportion => 1 / (effectTime - spawnTime);

    public event Action effectEndCallback;

    private CoroutineWrapper wrapper;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        bossStoneEffect.Stop();
        bossStoneEffect.Clear();

        var main = bossStoneEffect.main;
        main.simulationSpeed = 1;
        main.stopAction = ParticleSystemStopAction.Callback;

        if (wrapper == null)
            wrapper = new CoroutineWrapper(CoroutineWrapper.CoroutineRunner.Instance);
    }


    [Sirenix.OdinInspector.Button(Name = "Boss Stone Spawn")]
    public void StoneSpawn()
    {
        if (bossStoneEffect.isPaused || bossStoneEffect.isPlaying)
            return;

        Initialize();

        wrapper.StartSingleton(DelayPause());
    }

    private IEnumerator DelayPause()
    {
        yield return null;
        bossStoneEffect.Play();

        yield return new WaitWhile(() => bossStoneEffect.time < spawnTime);
        bossStoneEffect.Pause();
    }

    [Sirenix.OdinInspector.Button(Name = "Boss Stone Destory")]
    public void StoneDestroy()
    {
        if (wrapper.IsPlaying)
        {
            wrapper.Stop();
            bossStoneEffect.Pause();
        }

        //Get Effect Speed To ~ Destory => 1sec
        var main = bossStoneEffect.main;
        main.simulationSpeed = effectSpeed;

        //play
        if (bossStoneEffect.isPaused)
        {
            bossStoneEffect.Play();
        }
    }

    private void OnParticleSystemStopped()
    {
        effectEndCallback?.Invoke();
    }
}
