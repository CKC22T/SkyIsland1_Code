using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CKC2022
{
    public class PlaceHolder : MonoBehaviour
    {
        public enum PlaceType
        {
            None,
            Hand,
            Neck,
            ModelRoot,
        }

        [Serializable]
        public class Placeholder
        {
            public PlaceType key;
            public Transform transform;
        }

        [SerializeField]
        private Readonly<List<Placeholder>> holders = new();

        private bool isInitialized = false;
        private readonly Dictionary<PlaceType, Transform> holderDict = new();

        private void Awake()
        {
            foreach (var holder in holders.Value)
            {
                holderDict[holder.key] = holder.transform;
            }

            isInitialized = true;
        }

        private void Initialize()
        {
            foreach (var holder in holders.Value)
            {
                holderDict[holder.key] = holder.transform;
            }

            isInitialized = true;
        }

        public Transform this[PlaceType key]
        {
            get
            {
                if (!isInitialized)
                    Initialize();

                return holderDict[key];
            }
        }
    }
}