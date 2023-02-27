using Network.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Network.Packet.Response.Types;

public class ReplicatedMagicBoreActor : MonoBehaviour
{
    [SerializeField] private ReplicatedEntityData mEntityData;
    [SerializeField] private CharacterUI mCharacterUI;
    [SerializeField] private Animator mAnimator;

    [SerializeField] private ParticleSystem mIdleSpawnEffect;
    [SerializeField] private ParticleSystem mBoarSpawnEffect;
    [SerializeField] private ParticleSystem mAttackEffect;
    [SerializeField] private ParticleSystem mCrashEffect;
    [SerializeField] private ParticleSystem mAttackRange;

    [SerializeField] private Rigidbody mRigidbody;
    [SerializeField] private Collider mCollider;

    private Coroutine mIdleSpawnCor;

    public void Start()
    {
        mEntityData.OnAction += MEntityData_OnAction;

        //이유는 모르겠는데 맨 처음 한번만 이펙트 안나와서 그냥 이렇게함
        mIdleSpawnEffect.Play();
        mIdleSpawnCor = StartCoroutine(IdleSpawnCor());
    }

    private void MEntityData_OnAction(EntityActionData obj)
    {
        if (!obj.HasAction)
            return;

        var animationType = obj.AnimationType;
        string animationID = obj.AnimationId;


        switch (animationType)
        {
            case AnimationType.kTrigger:
                {
                    mAnimator.SetTrigger(animationID);

                    if (animationID == "Rise")
                    {
                        mIdleSpawnEffect.Play();
                        var m = mIdleSpawnEffect.main;
                        m.simulationSpeed = 1.0f;
                        mIdleSpawnCor = StartCoroutine(IdleSpawnCor());
                    }
                    if (animationID == "HideI")
                    {
                        //피격 off
                        HpUI.Instance?.RemoveCharacterUI(mCharacterUI);
                        mRigidbody.isKinematic = true;
                        mCollider.enabled = false;

                        if (mIdleSpawnCor != null)
                            StopCoroutine(mIdleSpawnCor);

                        var m = mIdleSpawnEffect.main;
                        if (m.simulationSpeed == 0)
                            m.simulationSpeed = 1.0f;
                    }
                    else if (animationID == "HideA")
                    {
                        //피격 off
                        HpUI.Instance?.RemoveCharacterUI(mCharacterUI);
                        mRigidbody.isKinematic = true;
                        mCollider.enabled = false;

                        StartCoroutine(HideACor());
                    }
                    else if (animationID == "Search")
                    {
                        //피격ON
                        mRigidbody.isKinematic = false;
                        mCollider.enabled = true;
                    }
                    else if (animationID == "Attack")
                    {
                        //피격ON
                        mAttackRange.Play();
                        mAttackEffect.Play();
                        mRigidbody.isKinematic = false;
                        mCollider.enabled = true;
                    }
                    else if (animationID == "Move")
                        mBoarSpawnEffect.Play();
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
            case AnimationType.kOther:
                {
                    switch(animationID)
                    {
                        case "AttackDone":
                            mCrashEffect.Play();
                            break;
                    }
                }
                break;

            default:
                break;
        }
    }

    private IEnumerator IdleSpawnCor()
    {
        yield return new WaitForSeconds(1.0f);
        var m = mIdleSpawnEffect.main;
        m.simulationSpeed = 0.0f;
    }
    private IEnumerator HideACor()
    {
        yield return new WaitForSeconds(0.67f);
        mBoarSpawnEffect.Play();
    }
}
