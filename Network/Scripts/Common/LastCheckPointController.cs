using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastCheckPointController : MonoBehaviour
{
    [SerializeField] private List<BaseLocationEventTrigger> mRemoteCallback = new();
    [SerializeField] private Animator LastCheckPointAnimator;
    [SerializeField] private GameObject CollapseEffect;

    private bool mIsCalled = false;

    private void Start()
    {
        CollapseEffect.SetActive(false);
    }

    public void FixedUpdate()
    {
        bool isLastDoorOn;

        if (ServerConfiguration.IS_SERVER)
        {
            isLastDoorOn = ServerSessionManager.Instance.GameGlobalState.GameGlobalState.LastCheckPointDoor.Value;
        }
        else
        {
            isLastDoorOn = ClientSessionManager.Instance.GameGlobalState.GameGlobalState.LastCheckPointDoor.Value;
        }

        LastCheckPointAnimator.SetBool("IsChecked", isLastDoorOn);
        CollapseEffect.SetActive(isLastDoorOn);

        if (ServerConfiguration.IS_CLIENT)
        {
            if (isLastDoorOn && !mIsCalled)
            {
                mIsCalled = true;
                OnCheckPointCallback();
            }
        }
    }

    public void OnCheckPointCallback()
    {
        foreach (var e in mRemoteCallback)
        {
            e.TriggeredEvent(null);
        }
    }

}
