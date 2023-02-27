using Network.Client;
using Network.Common;
using Network.Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CKC2022
{
    [RequireComponent(typeof(PlaceHolder))]
    [RequireComponent(typeof(ReplicatedEntityData))]
    public class ReplicatedEntityWeaponSelector : MonoBehaviour
    {
        [SerializeField]
        private PlaceHolder holder;

        [SerializeField]
        private ReplicatedEntityData replicatedData;

        [SerializeField]
        private ParticleSystem ChangeEffect;

        private Transform WeaponRoot { get => holder[PlaceHolder.PlaceType.Hand]; }

        private GameObject EquipedWeaponInstance;

        private void Awake()
        {   
            replicatedData.EquippedWeaponType.OnDataChanged += EquippedWeaponType_OnDataChanged;
        }

        private void OnDestroy()
        {
            replicatedData.EquippedWeaponType.OnDataChanged -= EquippedWeaponType_OnDataChanged;
        }

        private void EquippedWeaponType_OnDataChanged(ItemType type)
        {
            if (EquipedWeaponInstance != null)
                PoolManager.ReleaseObject(EquipedWeaponInstance);

            if (type == ItemType.kNoneItemType)
                return;

            if (!ItemManager.TryGetConfig(type, out var config))
                return;

            EquipedWeaponInstance = PoolManager.SpawnObject(config.WEAPON_MODEL);
            EquipedWeaponInstance.transform.SetParent(WeaponRoot, false);
            EquipedWeaponInstance.transform.localPosition = Vector3.zero;
            EquipedWeaponInstance.transform.localRotation = Quaternion.identity;

            //effect
            if (ChangeEffect != null)
            {
                var effectInstance = PoolManager.SpawnObject(ChangeEffect.gameObject);
                effectInstance.transform.SetParent(WeaponRoot, false);
                effectInstance.transform.localPosition = Vector3.zero;
                effectInstance.transform.localRotation = Quaternion.identity;
            }

            //sound
            var data = new SoundPlayData(transform.position);
            GameSoundManager.Play(SoundType.Player_GetWeapon, data);
        }

    }
}