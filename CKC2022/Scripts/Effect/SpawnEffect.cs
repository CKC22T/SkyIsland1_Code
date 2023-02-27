using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem effect;

    private void Start()
    {
        Instantiate(effect, transform);
    }

    private void OnEnable()
    {
        
    }
}
