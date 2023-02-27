using UnityEngine;

namespace Network.Packet
{
    public static class PacketExtension
    {
        public static Vector2 ToVector2(this Vector2Data data)
        {
            return new Vector2(data.X, data.Y);
        }

        public static Vector3 ToVector3(this Vector3Data data)
        {
            return new Vector3(data.X, data.Y, data.Z);
        }

        public static Vector4 ToVector4(this Vector4Data data)
        {
            return new Vector4(data.X, data.Y, data.Z, data.W);
        }

        public static Quaternion ToQuaternion(this Vector4Data data)
        {
            return new Quaternion(data.X, data.Y, data.Z, data.W);
        }

        public static Vector2Data ToData(this Vector2 vector)
        {
            return Vector2Data.CreateBuilder()
                .SetX(vector.x)
                .SetY(vector.y)
                .Build();
        }

        public static Vector3Data ToData(this Vector3 vector)
        {
            return Vector3Data.CreateBuilder()
                .SetX(vector.x)
                .SetY(vector.y)
                .SetZ(vector.z)
                .Build();
        }

        public static Vector4Data ToData(this Vector4 vector)
        {
            return Vector4Data.CreateBuilder()
                .SetX(vector.x)
                .SetY(vector.y)
                .SetZ(vector.z)
                .SetW(vector.w)
                .Build();
        }

        public static Vector4Data ToData(this Quaternion quaternion)
        {
            return Vector4Data.CreateBuilder()
                .SetX(quaternion.x)
                .SetY(quaternion.y)
                .SetZ(quaternion.z)
                .SetW(quaternion.w)
                .Build();
        }
    }
    
    public static class DetectorTypeExtension
    {
        public static bool IsDetector(this DetectorType detectorType)
        {
            return (int)detectorType > 0;
        }

        public static int GetEffectIndex(this DetectorType detectorType)
        {
            switch (detectorType)
            {
                case DetectorType.kRocket: return 0;
                case DetectorType.kMagicBall: return 3;
                case DetectorType.kRecoveryBall: return 1;
                case DetectorType.kHitscanLightning: return 2;

                case DetectorType.kHitscanKatana:
                case DetectorType.kHitscanMagicBoreRush:
                default: return -1;
            }
        }
        
        public static int GetHitEffectIndex(this DetectorType detectorType)
        {
            switch (detectorType)
            {
                case DetectorType.kRecoveryBall: return 4;
                case DetectorType.kHitscanLightning: return 5;
                case DetectorType.kMagicBall: return 6;
                    
                default: return -1;
            }
        }
    }


    public enum EntityBaseType
    {
        None,
        Humanoid,
        Mob,
        Weapon,
        Structure,
    }

    public static class EntityTypeExtension
    {
        private const int mFirstHumanoidType = (int)EntityType.kHumanoid;
        private const int mFirstMobType = (int)EntityType.kMob;
        private const int mFirstWeaponType = (int)EntityType.kWeapon;

        private const int mFirstPlayerEntity = (int)EntityType.kTestPlayer;
        private const int mLastPlayerEntity = (int)EntityType.kLastPlayerEntity;

        private const int mFirstStructure = (int)EntityType.kStructure;

        public static EntityBaseType GetEntityBaseType(this EntityType entityType)
        {
            int entityTypeByInt = (int)entityType;

            return entityTypeByInt switch
            {
                < mFirstHumanoidType => EntityBaseType.None,
                < mFirstMobType => EntityBaseType.Humanoid,
                < mFirstWeaponType => EntityBaseType.Mob,
                < mFirstStructure => EntityBaseType.Weapon,
                _ => EntityBaseType.Structure
            };
        }

        public static bool IsPlayerEntity(this EntityType entityType)
        {
            int type = (int)entityType;
            if (type >= mFirstPlayerEntity && type < mLastPlayerEntity)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsHumanoidEntity(this EntityType entityType) => EntityBaseType.Humanoid == entityType.GetEntityBaseType();
        public static bool IsMobEntity(this EntityType entityType) => EntityBaseType.Mob == entityType.GetEntityBaseType();
        public static bool IsWeaponEntity(this EntityType entityType) => EntityBaseType.Weapon == entityType.GetEntityBaseType();
        public static bool IsStructureEntity(this EntityType entityType) => EntityBaseType.Structure == entityType.GetEntityBaseType();
        public static bool IsActor(this EntityType entityType) => (IsHumanoidEntity(entityType) || IsMobEntity(entityType));
    }

    public enum ItemBaseType
    {
        None,
        Normal,
        Weapon,
    }

    public enum WeaponBaseType
    {
        None,
        Ranged,
        SpecialRarnged,
        Melee,
    }

    public static class ItemTypeExtension
    {
        // Normal Item
        private const int mFirstNormal = (int)ItemType.kFirstNormal;
        private const int mLastNormal = (int)ItemType.kLastNormal;

        // Weapon Item
        private const int mFirstWeapon = (int)ItemType.kFirstWeapon;

        private const int mRangedWeapon = (int)ItemType.kRangedWeapon;
        private const int mSpecialRangedWeapon = (int)ItemType.kWeaponSpecialRangedWeapon;
        private const int mMeleeWeapon = (int)ItemType.kMeleeWeapon;

        private const int mLastWeapon = (int)ItemType.kLastWeapon;

        public static ItemBaseType GetItemBaseType(this ItemType itemType)
        {
            int itemTypeByInt = (int)itemType;

            return itemTypeByInt switch
            {
                < mFirstNormal => ItemBaseType.None,
                <= mLastNormal => ItemBaseType.Normal,
                <= mLastWeapon => ItemBaseType.Weapon,
                _ => ItemBaseType.None,
            };
        }

        public static bool IsWeapon(this ItemType itemType)
        {
            return itemType.GetItemBaseType() == ItemBaseType.Weapon;
        }

        public static bool IsNormal(this ItemType itemType)
        {
            return itemType.GetItemBaseType() == ItemBaseType.Normal;
        }

        public static WeaponBaseType GetWeaponBaseType(this ItemType itemType)
        {
            if (!itemType.IsWeapon())
            {
                return WeaponBaseType.None;
            }

            int itemTypeByInt = (int)itemType;

            return itemTypeByInt switch
            {
                < mRangedWeapon => WeaponBaseType.None,
                <= mSpecialRangedWeapon => WeaponBaseType.Ranged,
                <= mMeleeWeapon => WeaponBaseType.SpecialRarnged,
                < mLastWeapon => WeaponBaseType.Melee,
                _ => WeaponBaseType.None,
            };
        }
    }

    public static class FactionTypeExtension
    {
        public static bool IsAlliance(this FactionType entityFactionType, in FactionType otherEntityFactionType)
        {
            return entityFactionType == otherEntityFactionType;
        }

        public static bool IsEnemy(this FactionType entityFactionType, in FactionType otherEntityFactionType)
        {
            return !(otherEntityFactionType == FactionType.kNeutral || otherEntityFactionType == FactionType.kNoneFactionType || entityFactionType == otherEntityFactionType);
        }
    }
}