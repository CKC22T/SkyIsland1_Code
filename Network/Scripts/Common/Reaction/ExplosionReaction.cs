using Network.Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[RequireComponent(typeof(BaseDetectorData))]
public class ExplosionReaction : MonoBehaviour
{
    [SerializeField]
    private BaseDetectorData data;
    
    [SerializeField]
    private int overrideDamage = 15;
    
    [SerializeField]
    private float distance = 4f;

    private void Awake()
    {
        //data.OnInitialized += Data_OnInitialized;
        data.OnDetectedOnce += Data_OnDetectedOnce;
    }

    //private void Data_OnInitialized()
    //{
    //}

    private void Data_OnDetectedOnce(DetectedInfo obj)
    {
        //for once call reaction, remove event at first detection   
        //data.OnDetectedOnce -= Data_OnDetectedOnce;

        //override Owner
        data.Info.OwnerCollider = null;

        ApplyDetectionInExplosion(obj.hitPoint);
    }

    private void ApplyDetectionInExplosion(in Vector3 position)
    {
        var origin = position;

        var layerMask = 1 << Global.LayerIndex_Entity;

        var hits = Physics.SphereCastAll(origin, distance, Vector3.down, 0.1f, layerMask);

        foreach (var hit in hits)
        {
            if (data.TryGenerateDetectedInfo(hit, out var detectedInfo))
            {
                data.ApplyDetection(detectedInfo, new DamageInfo(overrideDamage, FactionType.kNeutral));
            }
        }

    }
}
