using Network.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Network.Packet.Response.Types;

public class ReplicatedRegenEffectActor : MonoBehaviour
{
    [Sirenix.OdinInspector.Button]
    public void TestRegen()
    {
        var data = EntityActionData.CreateBuilder().SetAction(EntityAction.kRegenHp).Build();

        OnAction(data);
    }

    [SerializeField] private ParticleSystem Particle;
    [SerializeField] private ReplicatedEntityData Data;
    [SerializeField] private float Duration = 1f;

    private float mDurationRemaining = 0f;

    // Start is called before the first frame update
    private void Start()
    {
        Data.OnAction += OnAction;
    }

    private void OnAction(EntityActionData actionData)
    {
        if (!actionData.HasAction || actionData.Action != EntityAction.kRegenHp)
            return;

        mDurationRemaining = Duration;
    }

    private void FixedUpdate()
    {
        if (mDurationRemaining > 0f)
        {
            mDurationRemaining -= Time.fixedDeltaTime;
            if (!Particle.isPlaying)
            {
                Particle.Play(true);
            }
        }
        else
        {
            Particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void OnDestroy()
    {
        if (Data != null)
        {
            Data.OnAction -= OnAction;
        }
    }
}
