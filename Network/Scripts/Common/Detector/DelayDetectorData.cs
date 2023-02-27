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

public class DelayDetectorData : BaseDetectorData
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
    private float delay;
    public float Delay { get => delay; }

    [SerializeField]
    private float distance;
    public float Distance { get => distance; }

    private readonly List<BaseDetectorData> detectedDetector = new List<BaseDetectorData>();
    private CoroutineWrapper detectingWrapper;

    public BaseEntityData Owner { get; private set; }

    public Vector3 StartPosition { get; private set; }
    public Vector3 TargetPoint { get; private set; }

    public event Action OnHitscanStart;
    public event Action<float> OnHitscanUpdate;
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
        
        StartPosition = Owner.Position.Value;
        TargetPoint = Owner.Position.Value + info.RawViewVector;

        detectingWrapper.StartSingleton(Detecting()).SetOnComplete(OnDetectingComplete);
    }


    protected override void StartDestroyCoroutine()
    {
        //empty
    }

    private IEnumerator Detecting()
    {
        //initialize shot
        OnHitscanStart?.Invoke();

        float t = 0;
        while (t < delay)
        {
            OnHitscanUpdate?.Invoke(t / delay);
            t += Time.deltaTime;
            yield return null;
        }

        TestCollisions(TargetPoint);
        
        OnHitscanEnd?.Invoke();
    }

    private bool TestCollisions(in Vector3 targetPoint)
    {
        var layerMask = 1 << Global.LayerIndex_Entity | 1 << Global.LayerIndex_Detector;

        var hits = Physics.SphereCastAll(targetPoint + Vector3.up, distance, Vector3.down, 0.1f, layerMask);

        if (hits == null || hits.IsEmpty())
            return false;

        foreach (var hit in hits)
        {
            if (TryGenerateDetectedInfo(hit, out var detectedInfo))
                ApplyDetection(detectedInfo);
        }

        return true;
    }
}
