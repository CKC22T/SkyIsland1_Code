using Network.Server;
using Network.Packet;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class TurretAI : MonoBehaviour
{
    [SerializeField] private float AttackDelay = 1.0f;
    [SerializeField] private MasterHumanoidEntityData mTurrentEntity;

    private List<MasterEntityData> mCurrentDetectedEntities = new();

    private CoroutineWrapper mCoroutine;

    private void Start()
    {
        mCoroutine = CoroutineWrapper.Generate(this);
    }

    public void OnTriggerEnter(Collider other)
    {
        var entity = other.GetComponent<MasterEntityData>();
        if (entity == null)
            return;

        if (entity.FactionType.IsEnemy(mTurrentEntity.FactionType) == false)
            return;

        mCurrentDetectedEntities.Add(entity);

        mCoroutine.StartSingleton(attack());
    }

    public void OnTriggerExit(Collider other)
    {
        var entity = other.GetComponent<MasterEntityData>();
        if (entity == null)
            return;

        mCurrentDetectedEntities.Remove(entity);
    }

    private void OnDisable()
    {
        mCurrentDetectedEntities.Clear();
    }

    private IEnumerator attack()
    {
        float nearestDistance = float.MaxValue;
        Vector3 turretPosition = transform.position;
        Vector3 targetPosition = Vector3.zero;

        // Select nearest entity
        for (int i = mCurrentDetectedEntities.Count - 1; i >= 0; i--)
        {
            var e = mCurrentDetectedEntities[i];

            // Check valid entity
            if (e.gameObject.activeSelf == false || e == null || !e.IsAlive.Value || !e.IsEnabled.Value)
            {
                mCurrentDetectedEntities.RemoveAt(i);
                continue;
            }

            float distance = (turretPosition - e.transform.position).sqrMagnitude;

            if (nearestDistance > distance)
            {
                nearestDistance = distance;
                targetPosition = e.transform.position;
            }
        }

        // If there is detected target
        if (targetPosition != Vector3.zero)
        {
            Vector3 attackDirection = (targetPosition - transform.position).normalized;
            mTurrentEntity.ActionUseWeapon(attackDirection);
            Quaternion lookRotation = Quaternion.LookRotation(attackDirection.ToXZ().ToVector3FromXZ(), Vector3.up);

            mTurrentEntity.InputViewDirection = attackDirection.ToXZ();
            mTurrentEntity.InputMoveDirection = attackDirection.ToXZ();
            mTurrentEntity.transform.rotation = lookRotation;
            mTurrentEntity.Rotation.Value = lookRotation;

            yield return YieldInstructionCache.WaitForSeconds(AttackDelay);
            mCoroutine.StartSingleton(attack());
        }
    }
}
