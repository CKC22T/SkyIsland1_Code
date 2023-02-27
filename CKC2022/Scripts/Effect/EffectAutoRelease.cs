using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EffectAutoRelease : MonoBehaviour
{
    private ParticleSystem particle = null;

    private List<TrailRenderer> trails = new List<TrailRenderer>();

    private void Awake()
    {
        if (TryGetComponent(out particle))
        {
            var main = particle.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        trails = GetComponentsInChildren<TrailRenderer>().ToList();
    }

    private void OnParticleSystemStopped()
    {
        if (gameObject != null)
        {
            particle?.Clear();

            if (trails != null && trails.Count > 0)
            {
                foreach (var trail in trails)
                {
                    trail?.Clear();
                }
            }

            if (PoolManager.ContainsInstanceLookUp(gameObject))
                PoolManager.ReleaseObject(gameObject);
        }
    }
}