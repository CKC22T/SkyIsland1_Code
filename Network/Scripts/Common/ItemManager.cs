using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Network.Data;
using Network.Packet;
using System.Linq;

namespace Network.Common
{
    public class ItemManager : LocalSingleton<ItemManager>
    {
        [SerializeField]
        private List<WeaponConfiguration> originWeaponConfigurations;

        private readonly Dictionary<ItemType, WeaponConfiguration> ConfigurationDict = new Dictionary<ItemType, WeaponConfiguration>();

        protected override void Initialize()
        {
            base.Initialize();
            DontDestroyOnLoad(gameObject);


            foreach (var config in originWeaponConfigurations)
            {
                ConfigurationDict.Add(config.ITEM_TYPE, config);
            }
        }

        public bool tryGetConfig(ItemType itemType, out WeaponConfiguration config)
        {
            return ConfigurationDict.TryGetValue(itemType, out config);
        }

        public bool tryGetConfig(DetectorType detectorType, out WeaponConfiguration config)
        {
            config = ConfigurationDict.Values.FirstOrDefault(conf => conf.Detector != null && conf.DETECTOR_TYPE == detectorType);
            return config != null;
        }

        public static bool TryGetConfig(ItemType itemType, out WeaponConfiguration config)
        {
            config = null;
            
            if (!TryGetInstance(out var itemManager))
                return false;

            return itemManager.tryGetConfig(itemType, out config);
        }
        
        public static bool TryGetConfig(DetectorType detectorType, out WeaponConfiguration config)
        {
            config = null;

            if (!TryGetInstance(out var itemManager))
                return false;

            return itemManager.tryGetConfig(detectorType, out config);
        }
    }
}