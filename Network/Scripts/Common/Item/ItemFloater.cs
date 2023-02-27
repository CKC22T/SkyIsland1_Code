using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFloater : MonoBehaviour
{
    public float RotationSpeed = 70.0f;
    public float FloatSpeed = 1.1f;
    public float FloatAmount = 0.15f;

    private float mInitialFloatOffset = 0;
    private float mRotationFactor = 0;
    private float mSinFactor = 0;

    private Coroutine mFloatAnimationCoroutine;

    public void Start()
    {
        mInitialFloatOffset = transform.localPosition.y;
    }

    public void OnDisable()
    {
        StopCoroutine(mFloatAnimationCoroutine);
    }

    public void OnEnable()
    {
        mFloatAnimationCoroutine = StartCoroutine(FloatAnimation());
    }

    public IEnumerator FloatAnimation()
    {
        float floatOffset;

        while (true)
        {
            mSinFactor += Time.deltaTime * FloatSpeed;
            mRotationFactor += Time.deltaTime * RotationSpeed;

            floatOffset = mInitialFloatOffset + (Mathf.Sin(mSinFactor) + 1) * FloatAmount;

            transform.localRotation = Quaternion.Euler(new Vector3(0, mRotationFactor, 0));
            transform.localPosition = new Vector3(0, floatOffset, 0);

            yield return null;
        }
    }
}
