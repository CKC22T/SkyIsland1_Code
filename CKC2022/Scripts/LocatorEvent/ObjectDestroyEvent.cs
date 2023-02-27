using UnityEngine;
using System;
using System.Collections.Generic;
using Utils;

public class ObjectDestroyEvent : BaseLocationEventTrigger
{
    public List<GameObject> DestroyObjectList;
    private Queue<GameObject> DestroyQueue = new();

    public void Start()
    {
        if (DestroyObjectList != null)
        {
            foreach (var go in DestroyObjectList)
            {
                DestroyQueue.Enqueue(go);
            }
        }
    }

    public override void TriggeredEvent(BaseEntityData other)
    {
        while (!DestroyQueue.IsEmpty())
        {
            GameObject go = DestroyQueue.Dequeue();
            Destroy(go);
        }
    }
}