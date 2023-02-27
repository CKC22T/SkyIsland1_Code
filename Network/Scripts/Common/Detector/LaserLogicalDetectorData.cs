using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Network.Packet;
using Network.Server;
using Utils;
using UnityEngine.Events;
using Network.Client;

public class LaserLogicalDetectorData : BaseDetectorData
{
    public class SingleEntityDetectingRaycastComparer : IEqualityComparer<RaycastHit>
    {
        public static readonly SingleEntityDetectingRaycastComparer defaultComparer = new SingleEntityDetectingRaycastComparer();

        public bool Equals(RaycastHit x, RaycastHit y)
            => x.collider == y.collider;

        public int GetHashCode(RaycastHit obj)
            => obj.GetHashCode();

    }


    [SerializeField]
    private float distance;

    private readonly List<BaseEntityData> detectingTargets = new List<BaseEntityData>();
    private CoroutineWrapper detectingWrapper;

    private BaseEntityData Owner;

    private Vector3 LastAimDirection;

    public event Action<Vector3, Vector3> OnRotationUpdated;
    public event Action<RaycastHit> OnDetected;
    public event Action OnHitscanStart;
    public event Action OnHitscanEnd;
    public event Action OnDetectStart;
    public event Action OnDetectEnd;

    public override void Awake()
    {
        EventRiser.OnTriggerEnterEvent += CollisionEventRiser_OnTriggerEnterEvent;
        EventRiser.OnTriggerExitEvent += CollisionEventRiser_OnTriggerExitEvent;

        if (Network.ServerConfiguration.IS_SERVER)
            detectingWrapper = new CoroutineWrapper(ServerMasterDetectorManager.Instance);
        else
            detectingWrapper = new CoroutineWrapper(ClientNetworkManager.Instance);
    }


    private void CollisionEventRiser_OnTriggerEnterEvent(Collider obj)
    {
        if (TryGetEntity(obj, out var entity))
        {
            detectingTargets.Add(entity);
        }
    }

    private void CollisionEventRiser_OnTriggerExitEvent(Collider obj)
    {
        if (TryGetEntity(obj, out var entity))
        {
            detectingTargets.RemoveAll(e => e == entity);
        }
    }

    private bool TryGetEntity(in Collider other, out BaseEntityData entity)
    {
        entity = null;

        var otherLayer = other.gameObject.layer;
        if (otherLayer != Global.LayerIndex_Entity)
            return false;

        // Ignore if collide itself
        if (other == Info.OwnerCollider)
            return false;

        if (!other.TryGetComponent(out entity))
            return false;

        if (entity.IsEnabled.Value == false)
            return false;

        if (entity.IsAlive.Value == false)
            return false;

        return true;
    }

    public override void Initialize(int detectorID, DetectorInfo info, in Action<BaseDetectorData> onStopAction)
    {
        base.Initialize(detectorID, info, onStopAction);

        if(ClientWorldManager.TryGetInstance(out var clientWorld))
        {
            if (!clientWorld.TryGetEntity(info.OwnerEntityID, out var ownerData))
                return;

            Owner = ownerData;
        }
        else if(ServerMasterEntityManager.TryGetInstance(out var serverWorld))
        {
            var ownerData = serverWorld.GetEntityOrNull(info.OwnerEntityID);
            if (ownerData == null)
                return;

            Owner = ownerData;
        }

        LastAimDirection = info.Direction;

        detectingWrapper.StartSingleton(Detecting()).SetOnComplete(OnDetectingComplete);
    }


    protected override void StartDestroyCoroutine()
    {

    }

    private IEnumerator Detecting()
    {
        float t = 0;

        OnDetectStart?.Invoke();

        OnRotationUpdated?.Invoke(LastAimDirection, Owner.transform.forward * distance);

        //initialize shot
        OnHitscanStart?.Invoke();

        if (TestSingleCollision(LastAimDirection, out var hit) && TryGenerateDetectedInfo(hit, out var detectedInfo))
        {
            ApplyDetection(detectedInfo);
            OnDetected?.Invoke(hit);
        }

        OnHitscanEnd?.Invoke();

        //Loop
        while (t < SelfDestroyTime)
        {
            yield return YieldInstructionCache.WaitForFixedUpdate;
            t += Time.fixedDeltaTime;
            
            OnHitscanStart?.Invoke();
            
            transform.position = Owner.transform.position + Vector3.up;
            OnRotationUpdated?.Invoke(LastAimDirection, Owner.transform.forward * distance);
            
            //movement hit
            TestAngular(LastAimDirection, Owner.transform.forward);
            
            LastAimDirection = Owner.transform.forward;

            //currentPosition hit
            if (TestSingleCollision(LastAimDirection, out hit) && TryGenerateDetectedInfo(hit, out detectedInfo))
            {
                ApplyDetection(detectedInfo);
                OnDetected?.Invoke(hit);
            }

            OnHitscanEnd?.Invoke();
        }

        //lastReleaseShot
        OnHitscanStart?.Invoke();
        
        TestAngular(LastAimDirection, Owner.transform.forward * distance);

        OnHitscanEnd?.Invoke();


        OnDetectEnd?.Invoke();
    }

    public bool TestSingleCollision(in Vector3 direction, out RaycastHit closestHit)
    {
        var start = Owner.transform.position + Vector3.up;
        var end = Owner.transform.position + distance * direction;

        var layerMask = 1 << Global.LayerIndex_Entity | 1 << Global.LayerIndex_Ground;

        var hits = Physics.SphereCastAll(start, 0.2f, direction, distance, layerMask);

        hits = hits.Where((hit) => hit.point.magnitude > Vector3.kEpsilon).ToArray();

        if (hits == null || hits.IsEmpty())
        {
            closestHit = default;
            return false;
        }

        DebugingHit(hits);

        closestHit = hits[0];
        return true;
    }

    private static void DebugingHit(in RaycastHit[] hits)
    {
        //int i = 0;
        //var root = new GameObject();
        //root.transform.name = $"HitInformation + {Time.time}";

        //foreach (var hit in hits)
        //{
        //    var instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    instance.transform.position = hit.point;
        //    instance.transform.name = hit.transform.name + $" ({i++})";
        //    instance.transform.rotation = Quaternion.LookRotation(hit.normal);
        //    instance.transform.localScale = new Vector3(0.1f, 0.1f, 0.5f);
        //    instance.transform.SetParent(root.transform, true);
        //    DestroyImmediate(instance.GetComponent<BoxCollider>());
        //}
    }

    private void TestAngular(Vector3 lastDir, Vector3 currentDir)
    {
        lastDir = Vector3.Normalize(lastDir);
        currentDir = Vector3.Normalize(currentDir);
        
        var dot = Vector3.Dot(lastDir, currentDir);
        var entitas = detectingTargets.Where((entity)
            => Vector3.Dot(Vector3.Normalize(entity.Position.Value - Owner.Position.Value), currentDir) >= dot);

        var collected = new List<RaycastHit>();
        foreach (var entity in entitas)
        {
            var direction = Vector3.Normalize(entity.Position.Value - Owner.Position.Value);

            if (TestSingleCollision(direction, out var hit))
            {
                if (!collected.Contains(hit, SingleEntityDetectingRaycastComparer.defaultComparer))
                    collected.Add(hit);
            }
        }

        foreach (var hit in collected)
        {
            if (TryGenerateDetectedInfo(hit, out var detectedInfo))
            {
                ApplyDetection(detectedInfo);
                OnDetected?.Invoke(hit);
            }
        }
    }    
}