using Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AutoFocus : MonoBehaviour
{
    [field: SerializeField]
    public Transform focusTarget { get; private set; }

    [SerializeField]
    private float focusOffset = -1f;

    //
    [SerializeField]
    private float viewWidth;
    public float ViewWidth { get => viewWidth; set => viewWidth = value; }
    public float ViewHeight { get; set; }

    //
    [Range(10f, 60f)]
    [SerializeField]
    private float targetFOV;
    public float TargetFOV { get => targetFOV; set => targetFOV = value; }


    [SerializeField]
    private Vector2 focalLengthRange = new Vector2(300, 92);

    [SerializeField]
    private Vector2 apertureRange = new Vector2(2, 5);


    [SerializeField]
    private Vector3 ConstraintAxis;

    [SerializeField]
    private Volume volume;

    [SerializeField]
    private Transform FocusOffsetTransform;

    [field: SerializeField]
    public Camera targetCamera { get; private set; }

    [HideInInspector]
    public Vector2 fovRange;

    private void LateUpdate()
    {
        var currentFOV = Mathf.Lerp(targetCamera.fieldOfView, targetFOV, 0.15f);
        GetDistance(currentFOV, out var distance);
        SetDof(currentFOV, distance);
        SetPosition(currentFOV, distance);
    }

    private void GetDistance(in float fov, out float distance)
    {
        var ratio = ViewWidth / Mathf.Sin(Mathf.Deg2Rad * fov);
        distance = ratio * Mathf.Cos(Mathf.Deg2Rad * fov * 0.5f);
    }

    private void SetPosition(in float currentFOV, in float distance)
    {
        var cosFov = Mathf.Abs(Mathf.Cos(currentFOV * Mathf.Deg2Rad));
        var viewVector = focusTarget.position - targetCamera.transform.position + Vector3.up * ViewHeight;
        var diff = distance - viewVector.magnitude;

        viewVector = new Vector3(ConstraintAxis.x * viewVector.x, ConstraintAxis.y * viewVector.y, ConstraintAxis.z * viewVector.z);

        FocusOffsetTransform.position += Mathf.Lerp(0, diff, cosFov) * -viewVector.normalized;
        targetCamera.fieldOfView = currentFOV;
    }

    private void SetDof(in float currentFOV, in float distance)
    {
        if (volume.profile.TryGet<DepthOfField>(out var dof))
        {

            dof.focusDistance.value = distance + focusOffset;
            dof.focalLength.value = currentFOV.Remap(fovRange.ToTuple(), focalLengthRange.ToTuple());
            dof.aperture.value = currentFOV.Remap(fovRange.ToTuple(), apertureRange.ToTuple());
        }
    }
}
