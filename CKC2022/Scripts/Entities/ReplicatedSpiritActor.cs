using Network.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using static Network.Packet.Response.Types;

public class ReplicatedSpiritActor : MonoBehaviour
{
    [SerializeField] private ReplicatedEntityData mEntityData;
    [SerializeField] private Animator mAnimator;

    [SerializeField] private float mDeathReverseDelay = 3.0f;
    [SerializeField] private string mDeath1 = "Death1";

    private bool mIsIgnoreState = false;
    private CoroutineWrapper IgnoreCoroutineWapper;


    public void Start()
    {
        mEntityData.OnAction += MEntityData_OnAction;

        if (IgnoreCoroutineWapper == null)
        {
            IgnoreCoroutineWapper = new CoroutineWrapper(CoroutineWrapper.CoroutineRunner.Instance);
        }
    }

    public IEnumerator ignoreOtherActionByDelay(float ignoreDelay)
    {
        mIsIgnoreState = true;
        yield return new WaitForSeconds(ignoreDelay);
        mIsIgnoreState = false;
    }

    private void MEntityData_OnAction(EntityActionData obj)
    {
        if (mIsIgnoreState)
        {
            return;
        }

        if (!obj.HasAction)
            return;

        var animationType = obj.AnimationType;
        string animationID = obj.AnimationId;

        if (animationID == mDeath1)
        {
            IgnoreCoroutineWapper.StartSingleton(ignoreOtherActionByDelay(mDeathReverseDelay));
        }

        switch (animationType)
        {
            case AnimationType.kTrigger:
                {
                    mAnimator.SetTrigger(animationID);
                }
                break;

            case AnimationType.kBool:
                {
                    bool animationValue = obj.AnimationBoolValue;
                    mAnimator.SetBool(animationID, animationValue);
                }
                break;

            case AnimationType.kFloat:
                {
                    float animationValue = obj.AnimationFloatValue;
                    mAnimator.SetFloat(animationID, animationValue);
                }
                break;

            case AnimationType.kInt:
                {
                    int animationValue = obj.AnimationIntValue;
                    mAnimator.SetInteger(animationID, animationValue);
                }
                break;
            default:
                break;
        }
    }
}
