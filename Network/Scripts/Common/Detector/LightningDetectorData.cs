using Network.Client;
using Network.Common;
using Network.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Utils;

public class LightningDetectorData : BaseDetectorData
{
    [SerializeField]
    private float distance;
    [SerializeField]
    private float interval;

    [SerializeField]
    private GameObject LineEffectOrigin;

    private readonly List<BaseEntityData> detectedTargets = new List<BaseEntityData>();
    private readonly Dictionary<int, List<BaseEntityData>> detectingTargets = new();
    private readonly List<BaseEntityData> TestTargets = new();

    private CoroutineWrapper detectingWrapper;

    public event Action<BaseEntityData, BaseEntityData> OnDetected;

    private bool FirstDetection = false;
    
    public override void Awake()
    {
        EventRiser.OnTriggerEnterEvent += EventRiser_OnTriggerEnterEvent;
        DestroyCoroutine = CoroutineWrapper.Generate(CoroutineWrapper.CoroutineRunner.Instance);
        detectingWrapper = CoroutineWrapper.Generate(CoroutineWrapper.CoroutineRunner.Instance);
    }

    public override void Initialize(int detectorID, DetectorInfo info, in Action<BaseDetectorData> onStopAction)
    {
        base.Initialize(detectorID, info, onStopAction);

        FirstDetection = true;
        
        TestTargets.Clear();
        detectingTargets.Clear();
        detectedTargets.Clear();
    }


    private void EventRiser_OnTriggerEnterEvent(Collider other)
    {
        if (!TryGenerateDetectedInfo(other, out var detectedInfo, out var hit))
            return;

        Info.OwnerCollider = null;

        ApplyDetection(detectedInfo);

        detectedTargets.Add(detectedInfo.detectedEntity);

        TestTargets.Add(detectedInfo.detectedEntity);
        detectingWrapper.StartSingleton(SearchTarget());
        
        //mOnStop?.Invoke(this);
        
        gameObject.SetActive(false);
    }

    private IEnumerator SearchTarget()
    {
        int index = 0;
        while (TestTargets.Count > 0)
        {
            var copiedPoints = TestTargets.Where(entity => entity != null).ToList();
            
            TestTargets.Clear();

            foreach (var point in copiedPoints)
            {
                TestReachableTargets(index, point);
            }

            yield return YieldInstructionCache.WaitForSeconds(interval);

            index++;
        }

        OnDetectingComplete();
    }

    private bool TestReachableTargets(in int index, in BaseEntityData originEntity)
    {
        var origin = originEntity.Position.Value + Vector3.up;

        var layerMask = 1 << Global.LayerIndex_Entity;

        var hits = Physics.SphereCastAll((Vector3)origin, distance, Vector3.down, 0.1f, layerMask);

        //hits = hits.Where((hit) => hit.point.magnitude > Vector3.kEpsilon).ToArray();

        if (hits == null || hits.IsEmpty())
            return false;

        foreach (var hit in hits)
        {
            if (TryGenerateDetectedInfo(hit.collider, out var detectedInfo, out var detectedHit))
            {
                if (detectedInfo.detectedEntity == null)
                    continue;

                if (detectedTargets.Contains(detectedInfo.detectedEntity))
                    continue;

                detectedTargets.Add(detectedInfo.detectedEntity);

                //
                if (!detectingTargets.TryGetValue(index, out var list))
                {
                    list = new List<BaseEntityData>();
                    detectingTargets.Add(index, list);
                }

                list.Add(detectedInfo.detectedEntity);

                ApplyDetection(detectedInfo);

                SendAdditionalData(originEntity, detectedInfo.detectedEntity);

                TestTargets.Add(detectedInfo.detectedEntity);
            }
        }

        return true;
    }

    protected override void ApplyOnClient(in DetectedInfo detectedInfo, in DamageInfo damageInfo)
    {
        base.ApplyOnClient(detectedInfo, damageInfo);

        if(FirstDetection)
        {
            FirstDetection = false;
            
            if (Network.ServerConfiguration.IS_SERVER)
                return;
            
            if (!ItemManager.TryGetConfig((Info as ReplicatedDetectorInfo).detectorType, out var config))
                return;
            
            if (config.DETECT_EFFECT == null)
                return;

            var instance = PoolManager.SpawnObject(config.DETECT_EFFECT,
                detectedInfo.hitPoint,
                Quaternion.LookRotation(detectedInfo.normal));
        }
    }

    private void SendAdditionalData(in BaseEntityData originEntity, in BaseEntityData targetEntity)
    {
        if (Network.ServerConfiguration.IS_CLIENT)
            return;
        
        var packet = DetectorPacket.CreateLightning(DetectorID, originEntity, targetEntity);

        SendAsMaster(packet);
    }

    public override DetectorPacket ReceiveAsRemote(in string stringPacket)
    {
        var data = base.ReceiveAsRemote(stringPacket);
        
        switch ((DetectorPacket.PacketType)data.typeID)
        {
            case DetectorPacket.PacketType.LightningAdditionalData:
                ReceiveAdditionalData(data);
                break;
        }

        return data;
    }

    private void ReceiveAdditionalData(in DetectorPacket packet)
    {
        if (!ClientWorldManager.Instance.TryGetEntity(packet.StartEntityID, out var originEntity))
            return;

        if (!ClientWorldManager.Instance.TryGetEntity(packet.ReachableEntityID, out var detectedEntity))
            return;

        OnDetected?.Invoke(originEntity, detectedEntity);
    }

}
