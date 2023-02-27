using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Network.Packet;
using Network.Server;
using Network;
using CKC2022;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Network.Data
{

#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "WeaponData", menuName = "WeaponData", order = 0)]
#endif
    
    [Serializable]
    public class WeaponConfiguration : ScriptableObject
    {
        [SerializeField]
        private string comment;

        [SerializeField]
        protected BaseDetectorData Detector_Master;

        [SerializeField]
        protected BaseDetectorData Detector_Remote;

        [SerializeField]
        protected GameObject DetectEffect;

        [SerializeField]
        protected GameObject DestoryEffect;

        [SerializeField]
        protected GameObject ReactionEffect;

        [SerializeField]
        protected GameObject WeaponModel;


        [SerializeField]
        protected SoundType SpawnSoundCode;

        [SerializeField]
        protected SoundType HitSoundCode;
        
        [SerializeField]
        protected SoundType ReactionSoundCode;



        [Serializable]
        public class SerializableData
        {
            public EntityType legacyEntityType;

            [Header("current data")]
            public ItemType itemType;

            public float fireDelay;

            public float generationDelay = 0.18f;

            public int damage;
        }

        public SerializableData data;

        public BaseDetectorData Detector { get => ServerConfiguration.IS_SERVER ? Detector_Master : Detector_Remote; }

        public GameObject DETECT_EFFECT { get => DetectEffect; }

        public GameObject DESTORY_EFFECT { get => DestoryEffect; }

        public GameObject REACTION_EFFECT { get => ReactionEffect; }

        public GameObject WEAPON_MODEL { get => WeaponModel; }


        public ItemType ITEM_TYPE { get => data.itemType; }
        public DetectorType DETECTOR_TYPE { get => Detector.DetectorType; }
        public float FIRE_DELAY { get => data.fireDelay; }
        public float GENERATION_DELAY { get => data.generationDelay; }
        public float DAMAGE { get => data.damage; }


        public SoundType SPAWN_SOUND_CODE { get => SpawnSoundCode; }

        public SoundType HIT_SOUND_CODE { get => HitSoundCode; }

        public SoundType REACTION_SOUND_CODE { get => ReactionSoundCode; }

        //legacy
        public EntityType entityType { get => data.legacyEntityType; }
    }

}