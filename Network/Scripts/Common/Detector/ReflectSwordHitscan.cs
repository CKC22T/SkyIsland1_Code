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
using Network.Common;

public class ReflectSwordHitscan : BaseDetectorData
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

    private readonly List<BaseDetectorData> detectedDetector = new List<BaseDetectorData>();
    private CoroutineWrapper detectingWrapper;

    private BaseEntityData Owner;

    private Vector3 AimDirection;

    public event Action OnHitscanStart;
    public event Action OnHitscanEnd;

    public override void Awake()
    {
        if (Network.ServerConfiguration.IS_SERVER)
            detectingWrapper = new CoroutineWrapper(ServerMasterDetectorManager.Instance);
        else
            detectingWrapper = new CoroutineWrapper(ClientNetworkManager.Instance);
    }

    public override void Initialize(int detectorID, DetectorInfo info, in Action<BaseDetectorData> onStopAction)
    {
        base.Initialize(detectorID, info, onStopAction);

        if (ClientWorldManager.TryGetInstance(out var clientWorld))
        {
            if (!clientWorld.TryGetEntity(info.OwnerEntityID, out var ownerData))
                return;

            Owner = ownerData;
        }
        else if (ServerMasterEntityManager.TryGetInstance(out var serverWorld))
        {
            var ownerData = serverWorld.GetEntityOrNull(info.OwnerEntityID);
            if (ownerData == null)
                return;

            Owner = ownerData;
        }
        
        detectedDetector.Clear();

        AimDirection = info.Direction;

        detectingWrapper.StartSingleton(Detecting()).SetOnComplete(OnDetectingComplete);
    }


    protected override void StartDestroyCoroutine()
    {
        //empty
    }

    private IEnumerator Detecting()
    {
        float t = 0;

        //initialize shot
        OnHitscanStart?.Invoke();

        for (int i = 0; i < 5; ++i)
        {
            TestCollisions(AimDirection);

            yield return YieldInstructionCache.WaitForFixedUpdate;
        }
    }

    private bool TestCollisions(in Vector3 direction)
    {
        var origin = Owner.transform.position + Vector3.up;

        var detectorHits = Physics.SphereCastAll(origin, distance * 2f, direction, 0.1f, 1 << Global.LayerIndex_Detector);
        var entityHits = Physics.SphereCastAll(origin, distance, direction, 0.1f, 1 << Global.LayerIndex_Entity);

        var isDetectorHit = ProcessDetector(detectorHits);
        var isEntityHit = ProcessEntity(entityHits);

        return isDetectorHit || isEntityHit;
    }

    private bool ProcessDetector(in RaycastHit[] hits)
    {
        if (hits == null || hits.IsEmpty())
            return false;

        foreach (var hit in hits)
        {
            TryDetectorHit(hit);
        }

        return true;
    }

    private bool ProcessEntity(in RaycastHit[] hits)
    {
        if (hits == null || hits.IsEmpty())
            return false;

        foreach (var hit in hits)
        {
            if (TryGenerateDetectedInfo(hit, out var detectedInfo))
                ApplyDetection(detectedInfo);
        }
        
        return true;
    }

    private void TryDetectorHit(in RaycastHit hit)
    {
        if (Mathf.Abs((hit.transform.position - Owner.transform.position).y) > 2)
            return;
        
        var detector = hit.transform.GetComponentInParent<BaseDetectorData>();
        if (detector == null)
            return;

        if (detector.Behavior == null)
            return;

        if (detectedDetector.Contains(detector))
            return;

        var direction = (detector.transform.position - Owner.Position.Value);
        
        var dot = Vector3.Dot(AimDirection.normalized, direction.normalized);
        if (dot < 0.707f)
            return;
        
        switch (detector.Behavior)
        {
            case RocketBehavior rocket:
            {
                //rocket.GetDirection(out var direction);
                var dir = (detector.transform.position - Owner.transform.position).ToXZ().normalized.ToVector3FromXZ();
                rocket.SetDirection(dir);
                detector.SetDestoryTime(3f);
                detector.Info.OwnerCollider = null;

                RunEffect(detector.transform.position);

                break;
            }

            case BaseDetectorBehavior _:
            {
                break;
            }
        }

        detectedDetector.Add(detector);

    }

    private void RunEffect(in Vector3 position)
    {
        if (Info is not ReplicatedDetectorInfo)
            return;
        
        var info = Info as ReplicatedDetectorInfo;
        
        //Effect and sound
        if (!ItemManager.TryGetConfig(info.detectorType, out var config))
            return;

        PoolManager.SpawnObject(config.DETECT_EFFECT, position, Quaternion.identity);
    }
}