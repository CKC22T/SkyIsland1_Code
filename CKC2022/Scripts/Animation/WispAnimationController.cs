using CKC2022;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WispAnimationController : HumanoidAnimationController
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private ParticleSystem mWispDeath;
    [TabGroup("Component"), SerializeField] private SkinnedMeshRenderer[] mMeshRenderer;
    [TabGroup("Option"), SerializeField] private float mDissolveAt = 2.1f;
    [TabGroup("Option"), SerializeField] private float mDissolveTime = 1.1f;
    [TabGroup("Option"), SerializeField] private AnimationCurve mDissolveCurve;
    #endregion

    protected override void OnSetDeath()
    {
        base.OnSetDeath();

        animator.Play("WispDeath", 2);
        mWispDeath.Play();
        StartCoroutine(DissolveCor());
    }
    private IEnumerator DissolveCor()
    {
        yield return new WaitForSeconds(mDissolveAt);

        float timer = 0;
        while(true)
        {
            timer += Time.deltaTime / mDissolveTime;
            float value = mDissolveCurve.Evaluate(timer);
            foreach (var v in mMeshRenderer)
                v.material.SetFloat("_Dissolve_con", value);
            yield return null;
        }
    }
}
