using System;
using System.Collections;
using UnityEngine;

namespace Network.Client
{
    //TODO : use table data instead.
    [Serializable]
    public class DummyWeaponEntityInfo
    {
        public float shotPerSecond;
        public int damage;
    }


    [Obsolete("Not use. Use Item instead.")]
    public class ReplicatedWeaponEntityData : ReplicatedEntityData
    {
        [SerializeField]
        private GameObject HoverSelection;

        [SerializeField]
        private DummyWeaponEntityInfo weaponInfo;

        private void Awake()
        {
            HoverSelection.SetActive(false);
        }

        public float GetDelay()
        {
            return weaponInfo.shotPerSecond;
        }

        public void HoverEnter()
        {
            HoverSelection.SetActive(true);
        }

        public void HoverExit()
        {
            HoverSelection.SetActive(false);
        }
    }
}