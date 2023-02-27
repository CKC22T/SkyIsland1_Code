using System;
using UnityEngine;

using Network.Packet;
using UnityEngine.Events;

namespace Network.Server
{
    [Obsolete("무기 엔티티는 이제 사용되지 않음")]
    public class MasterWeaponEntityData : MasterEntityData
    {
        [SerializeField] protected Rigidbody mRigid;
        [SerializeField] private DetectorType mDetectorType;
        [SerializeField] private int mDamage = 10;
        [SerializeField] private float mDelay = 0.1f;

        /// <summary>
        /// 해당 무기가 발사 가능한 상태인지
        /// </summary>
        public bool CanFire => mDelay <= mTimer;
        public DetectorType DetectorType => mDetectorType;
        public int Damage => mDamage;
        public Rigidbody Rigid => mRigid;

        public MasterHumanoidEntityData EquipedEntity => mEntity;

        private MasterHumanoidEntityData mEntity;

        private float mTimer;

        public void FixedUpdate()
        {
            if (mEntity == null)
                return;

            var weaponSocketTransform = mEntity.WeaponEquipSocket;

            if (weaponSocketTransform == null)
                return;

            transform.position = weaponSocketTransform.position;
            transform.rotation = weaponSocketTransform.rotation;
            mTimer += Time.fixedDeltaTime;
        }

        public void EquippedByEntity(MasterHumanoidEntityData entity)
        {
            mEntity = entity;
            IsEnabled.Value = false;
        }

        public void Unequipped()
        {
            var socketTransform = mEntity.WeaponEquipSocket;

            transform.position = socketTransform.position;
            transform.rotation = socketTransform.rotation;
            Rigid.velocity = Vector3.zero;

            mEntity = null;
            IsEnabled.Value = true;
        }

        //public void Use(Vector3 origin, Vector3 direction, int OwnerEntityID, Collider OwnerCollider)
        //{
        //    if (!CanFire)
        //        return;

        //    mTimer = 0;

        //    DetectorInfo info = new DetectorInfo()
        //    {
        //        Origin = origin,
        //        Direction = direction,
        //        OwnerCollider = OwnerCollider,
        //        OwnerEntityID = OwnerEntityID,
        //        DamageInfo = new DamageInfo(this.mDamage)
        //    };

        //    ServerMasterDetectorManager.Instance.CreateNewDetector(mDetectorType, info);
        //}
    }
}
