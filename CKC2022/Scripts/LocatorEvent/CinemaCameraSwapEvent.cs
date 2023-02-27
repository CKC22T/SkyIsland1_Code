using CKC2022.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemaCameraSwapEvent : BaseLocationEventTrigger
{
    [SerializeField] private ReplicableLocator DeactivateLocator;
    [SerializeField] private float cinemaTime = 3.0f;
    [SerializeField] private Camera cinemaCamera;


    public override void TriggeredEvent(BaseEntityData other)
    {
        var networkMode = LocatorEventManager.Instance.NetworkMode;
        if (networkMode == NetworkMode.Master)
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
        FadeOutPopup.Instance.Open();
        yield return new WaitForSeconds(0.1f);

        cinemaCamera.gameObject.SetActive(true);

        yield return new WaitForSeconds(cinemaTime);

        cinemaCamera.gameObject.SetActive(false);

        container.Input.SetInputActiveState(false);
    }
}
