using Network.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuriPopupEvent : MonoBehaviour
{
    [SerializeField] private PooriScriptType pooriScriptType;
    [SerializeField] private List<BaseLocationEventTrigger> CallbackEvents;

    private void OnTriggerEnter(Collider other)
    {
        var networkMode = LocatorEventManager.Instance.NetworkMode;

        if (networkMode == NetworkMode.Remote)
        {
            if (ClientWorldManager.Instance.TryGetMyEntity(out var player))
            {
                if (other.TryGetComponent<ReplicatedEntityData>(out var entity))
                {
                    if (player.EntityID.Equals(entity.EntityID))
                    {
                        PuriPopup.Instance.Open(pooriScriptType.GetPooriScript());
                        gameObject.SetActive(false);

                        if (CallbackEvents != null)
                        {
                            foreach (var e in CallbackEvents)
                            {
                                e.TriggeredEvent(null);
                            }
                        }
                    }
                }
            }
        }
    }
}
