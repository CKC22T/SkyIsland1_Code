using System.Collections;
using UnityEngine;
using Utils;
using Sirenix.OdinInspector;

[RequireComponent(typeof(LocalEntityData))]
public class LocalEntityHitReactor : MonoBehaviour
{
    [SerializeField]
    private LocalEntityData localData;

    [SerializeField]
    private Transform hitRoot;

    [SerializeField]
    private float runtime;

    [SerializeField]
    private AnimationCurve reactionCurve;


    [SerializeField]
    [MinMaxSlider(0,100)]
    private Vector2 damageRange;

    [SerializeField]
    private AnimationCurve damageRemapCurve;

    [SerializeField]
    private float referenceDamage = 50;

    private CoroutineWrapper wrapper;
    private CoroutineWrapper rotationWrapper;

    private Quaternion defaultRotation;
    private Quaternion rotationOffset;

    private void Awake()
    {
        wrapper = new CoroutineWrapper(this);
        rotationWrapper = new CoroutineWrapper(this);
        
        hitRoot = transform.parent;
        defaultRotation = hitRoot.rotation;

        localData.OnHitAction += LocalData_OnHitAction;
    }

    private void LocalData_OnHitAction(ReplicatedDetectedInfo obj)
    {
        var dir = obj.DetectorInfo.Direction;
        var point = obj.hitPoint;
        var force = referenceDamage * damageRemapCurve.Evaluate(Mathf.Clamp(obj.DetectorInfo.DamageInfo.damage, damageRange.x, damageRange.y).Remap(damageRange.ToTuple(), (0, 1)));

        wrapper.StartSingleton(Rotate());
        rotationWrapper.StartSingleton(RotateRoot(runtime));
        
        IEnumerator Rotate()
        {
            float t = 0;
            while (t < runtime)
            {
                rotationOffset = GetQuaternionOffset(dir, point, hitRoot.position, t / runtime, force);
                t += Time.deltaTime;
                yield return null;
            }

            rotationOffset = Quaternion.identity;
        }
    }
    
    private IEnumerator RotateRoot(float runtime)
    {
        float t = 0;
        while (t < runtime * 3)
        {
            hitRoot.rotation = Quaternion.Slerp(hitRoot.rotation, rotationOffset * defaultRotation, 0.32f);
            yield return YieldInstructionCache.WaitForFixedUpdate;
        }
    }

    private Quaternion GetQuaternionOffset(Vector3 force, Vector3 point, Vector3 center, float t, float weight)
    {
        Vector3 comAxis = Vector3.Cross(force, point - center);
        float comValue = reactionCurve.Evaluate(t) * weight;
        return Quaternion.AngleAxis(comValue, comAxis);
    }

}