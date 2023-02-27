using Network.Client;
using Network.Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEvent : BaseLocationEventTrigger
{
    [SerializeField] private EntityType itemType;
    [SerializeField] private bool isPickedUp = false;

    //Item Move Component
    [SerializeField] private ObjectAniControl aniControl;

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ClientWorldManager.TryGetInstance(out var worldManager))
        {
            return;
        }

        StartCoroutine(pickedUpItem(other));

        IEnumerator pickedUpItem(BaseEntityData picker)
        {
            isPickedUp = true;
            //Item Move Component Off
            aniControl.isActive = false;
            while(Vector3.Distance(picker.transform.position, transform.position) > 0.001)
            {
                transform.position = Vector3.Lerp(transform.position, picker.transform.position, Time.fixedDeltaTime * 10);
                yield return new WaitForFixedUpdate();
            }

            gameObject.SetActive(false);
        }
    }
}
