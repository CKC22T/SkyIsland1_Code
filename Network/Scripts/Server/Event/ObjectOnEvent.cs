using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectOnEvent : BaseLocationEventTrigger
{
    public List<GameObject> objects;
    public override void TriggeredEvent(BaseEntityData other)
    {
        foreach(var obj in objects)
        {
            obj.SetActive(true);
        }
    }
}
