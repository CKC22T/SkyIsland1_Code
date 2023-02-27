using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class CameraFollow : LocalSingleton<CameraFollow>
{
    [Header("Target")]
    [SerializeField]
    public Transform centerPoint;
    [SerializeField]
    private float snapThreshold = 3f;


    [Header("Camera Transforms")]
    [SerializeField]
    private Transform RootTransform;
    [SerializeField]
    private Transform LookAtOffset;
    [SerializeField]
    private Transform CameraTransform;

    [SerializeField]
    private Transform referenceTransform;


    [Header("Property")]
    [SerializeField]
    private Vector3 targetOffset;

    [SerializeField]
    private float distanceFactor;

    [SerializeField]
    private AnimationCurve curve;

    [SerializeField]
    private float MaxDistance;

    [SerializeField]
    private float lookAtLerpFactor = 0.16f;

    [Header("debugging")]
    [SerializeField]
    private float curveTest;

    public float overrideFactor;

    private void FixedUpdate()
    {
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition(bool forceSnap = false)
    {
        if (centerPoint == null)
            return;

        var diff = centerPoint.position + targetOffset - referenceTransform.position;
        var distance = diff.ToXZ().magnitude;

        if (forceSnap || distance > snapThreshold)
            curveTest = 1;
        else
            curveTest = curve.Evaluate(distance * distanceFactor);


        RootTransform.position += Vector3.Lerp(Vector3.zero, diff, curveTest + overrideFactor);
    }

    public void ForceSnap()
    {
        UpdateCameraPosition(true);
    }

    public void UpdateLookAtOffset(in float height)
    {
        LookAtOffset.localPosition = LookAtOffset.transform.localPosition.ToXZ().ToVector3FromXZ(height);
    }
}
