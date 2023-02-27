using System;
using System.Collections;
using UnityEngine;

using Network.Packet;
using Network.Server;
using Utils;
using UnityEngine.Events;
using Network.Client;
using Network;
using System.Collections.Generic;
using Network.Common;

[Serializable]
public class DetectorPacket
{

    public enum PacketType
    {
        None = 0,
        Detected = 1,
        Complete = 2,

        LightningAdditionalData = 10
    }

    public int typeID; //Detected = 1, Destroy = 2
    public int detectorID;

    //detector info
    public int OwnerEntityID;
    public Vector3 Origin;
    public Vector3 Direction;

    //detected info
    public int DetectedEntityID;
    public Vector3 hitPoint;
    public Vector3 normal;

    //lightning additional data
    public int StartEntityID;
    public int ReachableEntityID;

    //damage info
    public int damage;

    public string Serialize()
    {
        return JsonUtility.ToJson(this);
    }

    public static DetectorPacket Create(in int detectorID, in DetectedInfo detectedInfo, in DamageInfo damageInfo)
    {
        var packet = new DetectorPacket();
        packet.detectorID = detectorID;
        packet.typeID = (int)PacketType.Detected;

        if (detectedInfo.detectedEntity != null)
            packet.DetectedEntityID = detectedInfo.detectedEntity.EntityID;

        packet.hitPoint = detectedInfo.hitPoint;
        packet.normal = detectedInfo.normal;

        packet.damage = damageInfo.damage;
        return packet;
    }

    public static DetectorPacket CreateComplete(in int detectorID)
    {
        var packet = new DetectorPacket();
        packet.detectorID = detectorID;
        packet.typeID = (int)PacketType.Complete;

        return packet;
    }
    
    public static DetectorPacket CreateLightning(in int detectorID, in BaseEntityData originEntity, in BaseEntityData targetEntity)
    {
        var packet = new DetectorPacket();
        packet.detectorID = detectorID;
        packet.typeID = (int)PacketType.LightningAdditionalData;

        packet.StartEntityID = originEntity.EntityID;
        packet.ReachableEntityID = targetEntity.EntityID;
        return packet;
    }
    
    public bool TryGetInfo(out DetectedInfo detectedInfo, out DamageInfo damageInfo)
    {
        detectedInfo = new DetectedInfo();
        damageInfo = new DamageInfo();

        if (DetectedEntityID == 0)
            return false;

        if (!ClientWorldManager.Instance.TryGetEntity(DetectedEntityID, out var entity))
            return false;

        detectedInfo.detectedEntity = entity;
        detectedInfo.hitPoint = hitPoint;
        detectedInfo.normal = normal;

        damageInfo.damage = damage;

        return true;
    }

    public static DetectorPacket ParseForm(in string stringPacket)
    {
        return JsonUtility.FromJson<DetectorPacket>(stringPacket);
    }
}

public class DetectorInfo
{
    public Collider OwnerCollider;
    public Vector3 Origin;
    public Vector3 Direction;
    public Vector3 RawViewVector;
    public int OwnerEntityID;
    public DamageInfo DamageInfo;
}

public class ReplicatedDetectorInfo : DetectorInfo
{
    //클라용 추가 정보
    public DetectorType detectorType;
}

public class DetectedInfo
{
    public BaseEntityData detectedEntity;
    public Vector3 hitPoint;
    public Vector3 normal;
}


public class ReplicatedDetectedInfo : DetectedInfo //복사생성자 구현 필요
{
    public ReplicatedDetectorInfo DetectorInfo;

    public ReplicatedDetectedInfo() { }
    public ReplicatedDetectedInfo(DetectedInfo info)
    {
        detectedEntity = info.detectedEntity;
        hitPoint = info.hitPoint;
        normal = info.normal;
    }
}

public class DamageInfo
{
    public DamageInfo() { }
    public DamageInfo(int damage, FactionType attacterFaction)
    {
        this.damage = damage;
        this.AttacterFaction = attacterFaction;
    }

    public FactionType AttacterFaction;
    public int damage;
}


public abstract class BaseDetectorData : MonoBehaviour
{
    public bool IsInstanceHitscan = false;
    public CollisionEventRiser EventRiser;
    public BaseDetectorBehavior Behavior;

    public Rigidbody Rigid;
    public DetectorType DetectorType;
    public int DetectorID;

    public DetectorInfo Info { get; protected set; }

    protected Action<BaseDetectorData> mOnStop;
    protected Action mOnDetectedCallback;
    public event Action<DetectedInfo> OnDetectedOnce;

    public float SelfDestroyTime = 3f;

    public CoroutineWrapper DestroyCoroutine;
    protected float mDestroyTime;

    public event Action OnInitialized;

    public virtual void Awake()
    {
        EventRiser.OnTriggerEnterEvent += eventRiser_OnTriggerEnterEvent;
        DestroyCoroutine = CoroutineWrapper.Generate(this);
    }

    public virtual void Initialize(int detectorID, DetectorInfo info, in Action<BaseDetectorData> onStopAction)
    {
        StartDestroyCoroutine();

        DetectorID = detectorID;
        Info = info;

        mOnStop = onStopAction;

        transform.position = info.Origin;
        transform.rotation = Quaternion.LookRotation(info.Direction);

        Behavior?.Initialize(Rigid, info);
        OnInitialized?.Invoke();
    }

    protected virtual void StartDestroyCoroutine()
    {
        mDestroyTime = Time.time + SelfDestroyTime;
        DestroyCoroutine.StartSingleton(destroySelf());
    }

    public void SetDestoryTime(float destroyTime)
    {
        SelfDestroyTime = destroyTime;
        mDestroyTime = Time.time + SelfDestroyTime;
    }

    private IEnumerator destroySelf()
    {
        if (IsInstanceHitscan)
        {
            yield return YieldInstructionCache.WaitForFixedUpdate;
            yield return YieldInstructionCache.WaitForFixedUpdate;
            ForceDestroy();
        }
        else
        {
            var waitUntileTime = new WaitUntil(() => Time.time > mDestroyTime);
            yield return waitUntileTime;
            ForceDestroy();
        }
    }

    public void ForceDestroy()
    {
        mOnStop?.Invoke(this);
        mOnStop = null;
    }

    public void SetParent(Transform parentTransform, Action onDetected)
    {
        transform.parent = parentTransform;
        mOnDetectedCallback += onDetected;
    }

    private void eventRiser_OnTriggerEnterEvent(Collider other)
    {
        if (!TryGenerateDetectedInfo(other, out var detectedInfo, out var hit))
            return;

        ApplyDetection(detectedInfo);

        OnDetectingComplete();
    }

    //generate detected info

    internal bool TryGenerateDetectedInfo(in RaycastHit hit, out DetectedInfo detectedInfo)
    {
        detectedInfo = null;

        var other = hit.collider;
        var otherLayer = other.gameObject.layer;

        if (other == null)
            return false;

        if (!(otherLayer == Global.LayerIndex_Entity || otherLayer == Global.LayerIndex_Ground))
            return false;

        // Ignore if collide itself
        if (other == Info.OwnerCollider)
            return false;

        // Set DetectedInfo of hit position and normal
        detectedInfo = new DetectedInfo();
        detectedInfo.hitPoint = hit.point;
        detectedInfo.normal = hit.normal;

        if (otherLayer == Global.LayerIndex_Entity)
        {
            if (!other.TryGetComponent<BaseEntityData>(out var entity))
                return false;

            if (entity.IsEnabled.Value == false)
                return false;

            if (entity.IsAlive.Value == false)
                return false;

            detectedInfo.detectedEntity = entity;
        }

        return true;
    }

    internal bool TryGenerateDetectedInfo(in Collider other, out DetectedInfo detectedInfo, out RaycastHit hit)
    {
        detectedInfo = null;
        hit = default;

        var otherLayer = other.gameObject.layer;

        if (other == null)
            return false;

        if (!(otherLayer == Global.LayerIndex_Entity || otherLayer == Global.LayerIndex_Ground))
            return false;

        // Ignore if collide itself
        if (other == Info.OwnerCollider)
            return false;

        // Set DetectedInfo of hit position and normal
        detectedInfo = new DetectedInfo();

        var rayDirection = Info.Direction == Vector3.zero ? Vector3.up : Info.Direction;

        if (other.Raycast(new Ray(transform.position - Info.Direction, rayDirection), out hit, Info.Direction.magnitude * 2))
        {
            detectedInfo.hitPoint = hit.point;
            detectedInfo.normal = hit.normal;
        }
        else
        {
            detectedInfo.hitPoint = transform.position;
            detectedInfo.normal = -rayDirection;
        }

        if (otherLayer == Global.LayerIndex_Entity)
        {
            if (!other.TryGetComponent<BaseEntityData>(out var entity))
                return false;

            if (entity.IsEnabled.Value == false)
                return false;

            if (entity.IsAlive.Value == false)
                return false;

            detectedInfo.detectedEntity = entity;

        }

        return true;
    }

    //apply detection

    internal void ApplyDetection(DetectedInfo detectedInfo, DamageInfo damageInfo = null)
    {
        if (damageInfo == null)
            damageInfo = Info.DamageInfo;


        ApplyOnServer(detectedInfo, damageInfo);
        //ApplyOnClient(detectedInfo, damageInfo);

        mOnDetectedCallback?.Invoke();
        OnDetectedOnce?.Invoke(detectedInfo);
    }

    private void ApplyOnServer(in DetectedInfo detectedInfo, in DamageInfo damageInfo)
    {
        if (ServerConfiguration.IS_CLIENT)
            return;

        Action<DamageInfo> takeDamageProcess = detectedInfo.detectedEntity switch
        {
            MasterEntityData e => e.ActionTakeDamage,
            _ => null
        };

        takeDamageProcess?.Invoke(damageInfo);

        var packet = DetectorPacket.Create(DetectorID, detectedInfo, damageInfo);

        SendAsMaster(packet);
    }

    protected virtual void ApplyOnClient(in DetectedInfo detectedInfo, in DamageInfo damageInfo)
    {
        if (ServerConfiguration.IS_SERVER)
            return;

        Action<ReplicatedDetectedInfo> clientRaiseHitInfo = detectedInfo.detectedEntity switch
        {
            LocalEntityData led => led.RaiseHitInfo,
            ReplicatedEntityData red => red.RaiseHitInfo,
            _ => null
        };

        var replicatedDetectedInfo = new ReplicatedDetectedInfo(detectedInfo)
        {
            DetectorInfo = Info as ReplicatedDetectorInfo
        };

        clientRaiseHitInfo?.Invoke(replicatedDetectedInfo);

        //Sound
        if (ItemManager.TryGetConfig(DetectorType, out var config))
        {
            CKC2022.GameSoundManager.Play(config.HIT_SOUND_CODE, new CKC2022.SoundPlayData(transform.position));
        }
    }


    protected void SendAsMaster(in DetectorPacket detectorPacket)
    {
        if (!ServerMasterDetectorManager.TryGetInstance(out var manager))
            return;

        manager.SendDetectorStringAction(this, detectorPacket.Serialize());
    }

    public virtual DetectorPacket ReceiveAsRemote(in string stringPacket)
    {
        var data = DetectorPacket.ParseForm(stringPacket);

        switch ((DetectorPacket.PacketType)data.typeID)
        {
            case DetectorPacket.PacketType.Detected:
                ReceiveDetected(data);
                break;

            case DetectorPacket.PacketType.Complete:
                ReceiveDetectComplete(data);
                break;
        }

        return data;
    }

    protected void ReceiveDetected(in DetectorPacket data)
    {
        data.TryGetInfo(out var debug_detected, out var debug_damage);
        Debug.Log($"[Detector]ReceiveAsRemote. \n" +
            $"detectorID is ({data.detectorID}), \n " +
            $"detectedInfo is ({debug_detected}),  \n" +
            $"damageInfo is ({debug_damage}),  \n");

        //validate
        if (data.detectorID != DetectorID)
            return;

        //OK
        if (data.TryGetInfo(out var detected, out var damage))
        {
            Debug.Log($"[Detector][ReceiveAsRemote] detectorID is ({data.detectorID}), Apply by network");
            //ApplyDetection(detected, damage);
            ApplyOnClient(detected, damage);
        }
    }

    protected void ReceiveDetectComplete(in DetectorPacket data)
    {
        OnDetectingAbort();
    }

    //release

    protected void OnDetectingComplete(bool isNotAborted)
    {
        OnDetectingComplete();
    }

    protected void OnDetectingComplete()
    {
        if (ServerConfiguration.IS_CLIENT)
            return;

        SendAsMaster(DetectorPacket.CreateComplete(DetectorID));

        OnDetectingAbort();
    }

    protected void OnDetectingAbort()
    {
        //sound

        if (ServerConfiguration.IS_CLIENT)
        {
            if (ItemManager.TryGetConfig(DetectorType, out var config))
            {
                CKC2022.GameSoundManager.Play(config.HIT_SOUND_CODE, new CKC2022.SoundPlayData(transform.position));
            }
        }

        Info.OwnerCollider = null;
        mOnDetectedCallback = null;

        DestroyCoroutine?.Stop();
        ForceDestroy();
    }

    private void OnDisable()
    {

    }
}
