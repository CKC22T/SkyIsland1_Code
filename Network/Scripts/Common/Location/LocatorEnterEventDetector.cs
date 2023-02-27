using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class LocatorEnterEventDetector : ReplicableLocator
{
    [Sirenix.OdinInspector.Button]
    public void SetupEventTriggers()
    {
        mEventTriggers.Clear();

        var childrens = transform.GetComponentsInChildren<BaseLocationEventTrigger>();

        foreach (var trigger in childrens)
        {
            mEventTriggers.Add(new TimerEvent(trigger));
        }
    }

    [SerializeField]
    private List<TimerEvent> mEventTriggers;

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

        callEvent(mEventTriggers, baseEntityData);

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
        // Check entity type if it's has BaseEntityData
        if (!other.TryGetComponent<BaseEntityData>(out var baseEntityData))
            return;

        mSpawned.RemoveAll((data) => data == baseEntityData);
    }
}
