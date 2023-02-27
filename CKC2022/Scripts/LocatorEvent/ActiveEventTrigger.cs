using System;
using System.Collections.Generic;
using UnityEngine;

public class ActiveEventTrigger : BaseLocationEventTrigger
{
    public bool ShouldActive = false;
    public List<GameObject> ActiveObjectList;

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ActiveObjectList == null)
        {
            return;
        }

        foreach (var go in ActiveObjectList)
        {
            go?.SetActive(ShouldActive);
        }
    }
}
