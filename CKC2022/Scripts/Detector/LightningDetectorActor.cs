using System.Collections;
using UnityEngine;

namespace CKC2022
{
    public class LightningDetectorActor : MonoBehaviour
    {
        [SerializeField]
        private LightningDetectorData data;

        [SerializeField]
        private LightningDetectorActorChild childOrigin;


        private void Awake()
        {
            data.OnDetected += Data_OnDetected;
        }

        private void Data_OnDetected(BaseEntityData prev, BaseEntityData next)
        {
            var instance = PoolManager.SpawnObject(childOrigin.gameObject).GetComponent<LightningDetectorActorChild>();
            instance.SetLine(prev.Position.Value, next.Position.Value);
        }
    }
}