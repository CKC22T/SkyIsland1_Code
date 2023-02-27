using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class LightningReaction : MonoBehaviour
{
    [SerializeField]
    private float chainRange;
    
    public void OnReact(DetectorInfo detector, DetectedInfo detected)
    {
        
        var origin = detected.hitPoint;

        var layerMask = 1 << Global.LayerIndex_Entity;

        var hits = Physics.SphereCastAll(origin, chainRange, Vector3.down, 0.1f, layerMask);

        //hits = hits.Where((hit) => hit.point.magnitude > Vector3.kEpsilon).ToArray();

        if (hits == null || hits.IsEmpty())
            return ;

        foreach (var hit in hits)
        {
            // :Thinking:

            
            //TryDetectorHit(hit);

            //OnHit(hit);
        }

    }

    //private void TryDetectorHit(in RaycastHit hit)
    //{
    //    if (Mathf.Abs((hit.transform.position - Owner.transform.position).y) > 1)
    //        return;

    //    var detector = hit.transform.GetComponentInParent<BaseDetectorData>();
    //    if (detector == null)
    //        return;

    //    if (detector.Behavior == null)
    //        return;

    //    var direction = (detector.transform.position - Owner.Position.Value);

    //    var dot = Vector3.Dot(AimDirection.normalized, direction.normalized);
    //    if (dot < 0.707f)
    //        return;

    //    switch (detector.Behavior)
    //    {
    //        case RocketBehavior rocket:
    //        {
    //            //rocket.GetDirection(out var direction);
    //            var dir = (detector.transform.position - Owner.transform.position).ToXZ().normalized.ToVector3FromXZ();
    //            rocket.SetDirection(dir);
    //            detector.SetDestoryTime(3f);

    //            break;
    //        }

    //        case BaseDetectorBehavior _:
    //        {
    //            break;
    //        }
    //    }

    //}

    //private void OnHit(in RaycastHit hit)
    //{
    //    Debug.Log("OnDetected : " + hit.transform.name);

    //    var other = hit.collider;

    //    var otherLayer = other.gameObject.layer;

    //    var detectedInfo = new DetectedInfo();

    //    detectedInfo.hitPoint = hit.point;
    //    detectedInfo.normal = hit.normal;


    //    if (otherLayer == Global.LayerIndex_Entity)
    //    {
    //        // Ignore if collide itself
    //        if (other == Info.OwnerCollider)
    //            return;

    //        if (!other.TryGetComponent<BaseEntityData>(out var entity))
    //            return;

    //        if (entity.IsEnabled.Value == false)
    //            return;

    //        if (entity.IsAlive.Value == false)
    //            return;

    //        detectedInfo.detectedEntity = entity;

    //        Action<int> takeDamageProcess = entity switch
    //        {
    //            MasterEntityData e => e.ActionTakeDamage,
    //            _ => null
    //        };

    //        takeDamageProcess?.Invoke(Info.Damage);
    //    }

    //    Info.OwnerCollider = null;

    //    mOnDetected?.Invoke(Info, detectedInfo);
    //    mOnDetectedCallback?.Invoke();
    //    OnDetected?.Invoke(hit);
    //}
}