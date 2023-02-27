using Network.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Network.Packet.Response.Types;

public class ReplicatedWispActor : MonoBehaviour
{
    [SerializeField] private ReplicatedEntityData mEntityData;
    [SerializeField] private Animator mAnimator;

    [SerializeField] private ParticleSystem mFindEffect;

    private void Start()
    {
        mEntityData.OnAction += MEntityData_OnAction;
    }
    private void MEntityData_OnAction(EntityActionData obj)
    {
        if (!obj.HasTriggerString)
            return;

        switch (obj.TriggerString)
        {
            case "findStart":
                mAnimator.SetTrigger("findStart");
                mFindEffect.Play();
                break;
            case "findEnd":
                mAnimator.SetTrigger("findEnd");
                break;
        }
    }
}
