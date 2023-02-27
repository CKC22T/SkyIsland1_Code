
using UnityEngine;
using Utils;

[ExecuteInEditMode]
public class BoxGizmo : MonoBehaviour
{
    #if UNITY_EDITOR
    public Color GizmoFillColor = new Color(1, 1, 1, 0.3f);
    public Color GizmoLineColor = new Color(1, 1, 1, 0.6f);

    public Vector3 BoxSize = Vector3.one;
    private BoxCollider mBoxCollider;
    private ReplicableLocator mLocator;

    public void Start()
    {
#if UNITY_EDITOR
        mBoxCollider = GetComponent<BoxCollider>();
        mLocator = GetComponentInChildren<ReplicableLocator>();

        if (Application.isPlaying && TryGetComponent<MeshRenderer>(out var renderer))
            renderer.enabled = false;
#endif
    }

    public void OnDrawGizmos()
    {
        if (mBoxCollider == null)
        {
            return;
        }

        Vector3 currentPosition = mBoxCollider.center - transform.position;

        Gizmos.matrix = Matrix4x4.TRS(this.transform.TransformPoint(transform.position), transform.rotation, transform.lossyScale);

        Gizmos.color = GizmoFillColor;

        if (mLocator != null && mLocator.IsActivated.Value == false)
        {
            Gizmos.color = GizmoFillColor.MultiplyToAlpha(0.5f);
        }

        if (mBoxCollider == null)
        {
            Gizmos.DrawCube(currentPosition, BoxSize);
            Gizmos.color = GizmoLineColor;
            Gizmos.DrawWireCube(currentPosition, BoxSize);
        }
        else
        {
            Gizmos.DrawCube(currentPosition, mBoxCollider.size);
            Gizmos.color = GizmoLineColor;
            Gizmos.DrawWireCube(currentPosition, mBoxCollider.size);
        }
    }
    #endif
}
