using Network.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Game.Chr
{
    public class CharacterActionDetector : MonoBehaviour
    {
        #region Get,Set
        /// <summary>
        /// 감지된것들
        /// </summary>
        public IReadOnlyList<MasterEntityData> Detected { get => mDetected; }
        #endregion
        #region Value
        private List<MasterEntityData> mDetected = new List<MasterEntityData>();
        #endregion

        #region Event
        private IEnumerator Start()
        {
            //TODO : 귀찮아서 감지된것이 꺼졌을때의 예외처리는 대충 짜놨음, 나중에 수정하는것을 권장함
            while(true)
            {
                yield return new WaitForSeconds(0.1f);
                try
                {
                    for (int i = 0; i < mDetected.Count; i++)
                    {
                        if (!mDetected[i].gameObject.activeSelf || mDetected[i] is MasterMobEntityData mob && !mob.OwnerCollider.enabled)
                        {
                            mDetected.RemoveAt(i);
                            --i;
                        }
                    }
                }
                catch { }
            }
        }
        private void OnDisable()
        {
            mDetected.Clear();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody)
            {
                var entity = other.GetComponentInParent<MasterEntityData>();
                if (entity && !mDetected.Contains(entity))
                    mDetected.Add(entity);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody)
            {
                var entity = other.GetComponentInParent<MasterEntityData>();
                if (entity)
                    mDetected.Remove(entity);
            }
        }
        #endregion
    }
}