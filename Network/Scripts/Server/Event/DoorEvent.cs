using Network.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorEvent : BaseLocationEventTrigger
{
    public Vector3 destination;
    public Vector3 startingPoint;
    public float destinationMoveSpeed = 3.0f;
    public float startingPointMoveSpeed = 5.0f;

    private Coroutine moveCoroutine = null;
    private bool isOpen = false;

    [Sirenix.OdinInspector.Button(Name = "Set Destination")]
    public void SetDestination()
    {
        destination = transform.position;
    }

    [Sirenix.OdinInspector.Button(Name = "Set StartingPoint")]
    public void SetStartingPoint()
    {
        startingPoint = transform.position;
    }

    [Sirenix.OdinInspector.Button(Name = "Set Position To Destination")]
    public void SetPositionToDestination()
    {
        transform.position = destination;
    }

    [Sirenix.OdinInspector.Button(Name = "Set Position To StartingPoint")]
    public void SetPositionToStartingPoint()
    {
        transform.position = startingPoint;
    }


    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ClientWorldManager.TryGetInstance(out var worldManager))
        {
            return;
        }

        if (isOpen)
        {
            DoorClose();
            isOpen = false;
        }
        else
        {
            DoorOpen();
            isOpen = true;
        }
    }

    private void DoorOpen()
    {
        //var rigidbody = GetComponent<Rigidbody>();
        //rigidbody.useGravity = false;
        //rigidbody.isKinematic = true;

        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(moveing());

        IEnumerator moveing()
        {
            //var objectAniControl = GetComponent<ObjectAniControl>();
            //objectAniControl.isActive = false;

            while ((destination - transform.position).magnitude > 0.001f)
            {
                transform.position = Vector3.Lerp(transform.position, destination, Time.fixedDeltaTime * destinationMoveSpeed);
                yield return new WaitForFixedUpdate();
            }

            transform.position = destination;
            //objectAniControl.SetOriginPos();
        }
    }

    public void DoorClose()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(moveing());

        IEnumerator moveing()
        {
            //var objectAniControl = GetComponent<ObjectAniControl>();
            //objectAniControl.isActive = false;

            while ((startingPoint - transform.position).magnitude > 0.001f)
            {
                transform.position = Vector3.Lerp(transform.position, startingPoint, Time.fixedDeltaTime * startingPointMoveSpeed);
                yield return new WaitForFixedUpdate();
            }

            transform.position = startingPoint;
            //objectAniControl.SetOriginPos();
        }
    }
}
