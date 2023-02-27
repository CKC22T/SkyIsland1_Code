using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class LocatorEnterExitEventDetector : ReplicableLocator
{
    [SerializeField]
    protected List<TimerEvent> mEnterEventTriggers;

    [SerializeField]
    protected List<TimerEvent> mExitEventTriggers;

    private List<Collider> mEnterColliders = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        if (mEnterColliders.Contains(other))
        {
            return;
        }

        if (!isConditionMatched(other, out var baseEntityData))
        {
            return;
        }

        if (mSpawned.Contains(baseEntityData))
            return;

        callEvent(mEnterEventTriggers, baseEntityData);

        mEnterColliders.Add(other);
        StartCoroutine(removeEnterCollider(other));

        if (IsSingleUsed)
        {
            IsActivated.Value = false;
        }
    }

    private IEnumerator removeEnterCollider(Collider enteredCollider)
    {
        yield return new WaitForSeconds(1.0f);
        mEnterColliders.Remove(enteredCollider);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isConditionMatched(other, out var baseEntityData))
        {
            return;
        }

        mSpawned.RemoveAll((data) => data == baseEntityData);

        callEvent(mExitEventTriggers, baseEntityData);
    }
}
