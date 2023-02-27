using Network.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEvent : BaseLocationEventTrigger
{
    public Vector3 destination;

    [Sirenix.OdinInspector.Button(Name = "Set Destination")]
    public void SetDestination()
    {
        destination = transform.position;
    }


    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ClientWorldManager.TryGetInstance(out var worldManager))
        {
            return;
        }

        StartCoroutine(moveing());

        IEnumerator moveing()
        {
            while((destination - transform.position).magnitude > 0.001f)
            {
                transform.position = Vector3.Lerp(transform.position, destination, Time.fixedDeltaTime * 10);
                yield return new WaitForFixedUpdate();
            }

            transform.position = destination;

            //var from = box.transform.position;
            //var to = destination;

            //var time = 0.0f;
            //while (time <= 1.0f)
            //{
            //    time += Time.deltaTime * 2.0f;
            //    box.transform.position = Vector3.Lerp(from, destination, time);
            //    yield return null;
            //}
            //box.transform.position = Vector3.Lerp(from, destination, 1.0f);
            //Destroy(box);
            //locator.GetComponent<BoxCollider>().enabled = true;
            //locator.GetComponent<LocatorEnterEventDetector>().IsActivated.Value = true;
        }
    }
}
