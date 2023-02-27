
using UnityEngine;

[ExecuteInEditMode]
public class SpherePointGizmo : MonoBehaviour
{
#if UNITY_EDITOR
    private SphereCollider mSphereCollider;

    public Color GizmoFillColor = new Color(1, 1, 1, 0.3f);
    public Color GizmoLineColor = new Color(1, 1, 1, 0.6f);
    public float Radius = 0.5f;

	public void Start()
	{
        mSphereCollider = GetComponent<SphereCollider>();
    }

	public void OnDrawGizmos()
    {
        Gizmos.color = GizmoFillColor;

        if (mSphereCollider == null)
        {
            Gizmos.DrawSphere(transform.position, Radius);
            Gizmos.color = GizmoLineColor;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
        else
        {
            Gizmos.DrawSphere(transform.position, mSphereCollider.radius);
            Gizmos.color = GizmoLineColor;
            Gizmos.DrawWireSphere(transform.position, mSphereCollider.radius);
        }

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * Radius * 3);
    }
#endif
}
