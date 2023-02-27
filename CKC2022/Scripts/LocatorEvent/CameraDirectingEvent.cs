using CKC2022.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDirectingEvent : BaseLocationEventTrigger
{
    [SerializeField] private ReplicableLocator DeactivateLocator;
    [SerializeField] private float directingTime = 3.0f;
    [SerializeField] private Transform DirectingTarget;

    public override void TriggeredEvent(BaseEntityData other)
    {
        var networkMode = LocatorEventManager.Instance.NetworkMode;
        if(networkMode == NetworkMode.Master)
        {
            return;
        }
        if (ClientSessionManager.Instance.TryGetMySessionSlotDataOrNull(out var slot))
        {
            if (slot.SelectedCharacterType.Value == other.EntityType)
            {
                DeactivateLocator.SetActivation(false);
                StartCoroutine(cameraDirection());
            }
        }
    }

    private IEnumerator cameraDirection()
    {
        PlayerInputNetworkManager.TryGetAnyInputContainer(out var container);
        container.Input.SetInputActiveState(true);

        var originTarget = CameraFollow.Instance.centerPoint;
        CameraFollow.Instance.centerPoint = DirectingTarget;

        yield return new WaitForSeconds(directingTime);

        CameraFollow.Instance.centerPoint = originTarget;

        container.Input.SetInputActiveState(false);
    }
}
