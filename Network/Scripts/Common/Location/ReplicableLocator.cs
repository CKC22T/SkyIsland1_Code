using Network.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using static Network.Packet.Response.Types;

[Serializable]
public class TimerEvent
{
    public BaseLocationEventTrigger EventTrigger;
    public float Timer;

    public TimerEvent(BaseLocationEventTrigger eventTrigger, float timer = 0)
    {
        EventTrigger = eventTrigger;
        Timer = timer;
    }
}

public abstract class ReplicableLocator : MonoBehaviour
{
    private NetworkMode mNetworkMode = NetworkMode.None;

    [SerializeField]
    protected int mLocatorID = 0;
    public int LocatorID => mLocatorID;

    [SerializeField]
    protected Collider mLocatorCollider;

    [SerializeField]
    protected LocatorType mLocatorType;
    public LocatorType LocatorType => mLocatorType;

    [SerializeField]
    public Notifier<bool> IsActivated = new Notifier<bool>(true);

    [SerializeField]
    public Notifier<Vector3> Position;

    [SerializeField]
    public Notifier<Quaternion> Rotation;

    [SerializeField]
    protected LayerMask DetectLayerMask;

    [SerializeField]
    protected EntityBaseType EntityBaseTypeCondition;

    [SerializeField]
    protected bool IsSingleUsed = true;

    [SerializeField]
    protected List<FactionType> FactionTypeConditions;

    private bool mDetectAllFaction = false;

    protected List<BaseEntityData> mSpawned = new List<BaseEntityData>();

    private Coroutine mEventCoroutine = null;

    public void FixedUpdate()
    {
        if (mNetworkMode == NetworkMode.Master)
        {
            Position.Value = transform.position;
            Rotation.Value = transform.rotation;
        }
    }

    private Vector3 mDestinationPosition;
    private Quaternion mDestinationRotation;
    private float mLerpSpeed = 0.65f;

    public void Start()
    {
        foreach (FactionType factionType in FactionTypeConditions)
        {
            if (factionType == FactionType.kNoneFactionType)
            {
                mDetectAllFaction = true;
            }
        }
    }

    public void Update()
    {
        if (mNetworkMode == NetworkMode.Remote)
        {
            // TODO : 캣멀룸
            if(Vector3.Distance(transform.position, mDestinationPosition) > 30.0f)
            {
                transform.position = mDestinationPosition;
                transform.rotation = mDestinationRotation;
            }

            transform.position = Vector3.Lerp(transform.position, mDestinationPosition, mLerpSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, mDestinationRotation, mLerpSpeed);

            //transform.position = Position.Value;
            //transform.rotation = Rotation.Value;
        }
    }

    public virtual void Awake()
    {
        //IsActivated.OnDataChanged += onLocatorActivatedChanged;

        Position = new Notifier<Vector3>(transform.position);
        Rotation = new Notifier<Quaternion>(transform.rotation);

        Position.OnDataChanged += setPosition;
        Rotation.OnDataChanged += setRotation;

        mDestinationRotation = transform.rotation;
        mDestinationPosition = transform.position;
    }

    private bool mIsDestroyed = false;

    //public virtual void OnDestroy()
    //{
    //    if (mIsDestroyed)
    //    {
    //        return;
    //    }

    //    mIsDestroyed = true;

    //    if (mEventCoroutine != null)
    //    {
    //        StopCoroutine(mEventCoroutine);
    //        mEventCoroutine = null;
    //    }

    //    IsActivated.OnDataChanged -= onLocatorActivatedChanged;

    //    Position.OnDataChanged -= setPosition;
    //    Rotation.OnDataChanged -= setRotation;

    //    //if (mNetworkMode == NetworkMode.Master && LocatorEventManager.TryGetInstance(out var locatorEventManager))
    //    //{
    //    //    locatorEventManager.DestroyLocatorAsMaster(LocatorID);
    //    //}
    //}

    #region Operation

    //public void DestroyAsMaster()
    //{
    //    if (mIsDestroyed)
    //    {
    //        return;
    //    }

    //    mIsDestroyed = true;

    //    IsActivated.OnDataChanged -= onLocatorActivatedChanged;

    //    Position.OnDataChanged -= setPosition;
    //    Rotation.OnDataChanged -= setRotation;

    //    if (mNetworkMode == NetworkMode.Master && LocatorEventManager.TryGetInstance(out var locatorEventManager))
    //    {
    //        locatorEventManager.DestroyLocatorAsMaster(LocatorID);
    //    }
    //}

    public void SetActivation(bool isActive)
    {
        IsActivated.Value = isActive;
    }

    #endregion

    #region Locator Events

    //private void onLocatorActivatedChanged(bool isActivated)
    //{
    //    mLocatorCollider.enabled = isActivated;
    //}

    protected bool isConditionMatched(Collider other, out BaseEntityData baseEntityData)
    {
        baseEntityData = null;

        if (IsActivated.Value == false)
            return false;

        // Check layermask
        if (1 << other.gameObject.layer != DetectLayerMask.value)
            return false;

        // Check entity type if it's has BaseEntityData
        if (!other.TryGetComponent<BaseEntityData>(out baseEntityData))
            return false;

        // Check base entity type
        EntityBaseType currentBaseType = baseEntityData.EntityType.GetEntityBaseType();
        if (EntityBaseTypeCondition != EntityBaseType.None && currentBaseType != EntityBaseTypeCondition)
            return false;

        // Check faction

        if (mDetectAllFaction)
        {
            return true;
        }

        foreach (FactionType factionCondition in FactionTypeConditions)
        {
            if (factionCondition == baseEntityData.FactionType)
            {
                return true;
            }
        }

        // There is no matched faction
        return false;
    }

    protected void callEvent(List<TimerEvent> events, BaseEntityData entityData)
    {
        foreach (var e in events)
        {
            switch (e.EventTrigger)
            {
                case EntitySpawnerEvent spawn when entityData is not null:
                    {
                        mEventCoroutine = StartCoroutine(spawnTimerEvent(e, spawn, entityData));
                        break;
                    }

                default:
                    {
                        //e.EventTrigger.TriggeredEvent(data);
                        mEventCoroutine = StartCoroutine(triggeredTimerEvent(e, entityData));
                        break;
                    }
            }
        }

        IEnumerator spawnTimerEvent(TimerEvent timerEvent, EntitySpawnerEvent e, BaseEntityData entityData)
        {
            yield return new WaitForSeconds(timerEvent.Timer);

            if (e.TrySpawnEvent(entityData, out var SpawnedEntity))
            {
                mSpawned.Add(SpawnedEntity);
            }
        }

        IEnumerator triggeredTimerEvent(TimerEvent timerEvent, BaseEntityData data)
        {
            yield return new WaitForSeconds(timerEvent.Timer);
            timerEvent.EventTrigger.TriggeredEvent(data);
        }
    }

    #endregion

    #region Networking

    public void Initialized(int locatorID, LocatorType type, Vector3 position, Quaternion rotation)
    {
        // Set locator base information
        mLocatorID = locatorID;
        mLocatorType = type;

        // Set locator transform
        Position.Set(position, false);
        Rotation.Set(rotation, false);

        transform.position = position;
        transform.rotation = rotation;

        //mDestinationPosition = position;
        //mDestinationRotation = rotation;
    }

    private void setPosition(Vector3 position) => mDestinationPosition = position;
    private void setRotation(Quaternion rotation) => mDestinationRotation = rotation;

    public void SetNetworkMode(NetworkMode networkMode) => mNetworkMode = networkMode;
    public void SetLocatorID(int eventID) => mLocatorID = eventID;

    public bool TryGetChangedLocatorStateDataOrNull(out LocatorStateData data)
    {
        if (Position.IsDirty || Rotation.IsDirty || IsActivated.IsDirty)
        {
            var builder = LocatorStateData.CreateBuilder()
                .SetLocatorId(mLocatorID);

            //if (IsActivated.GetDirtyAndClear(out var isActivatedData))
            //    builder.SetIsActivated(isActivatedData);

            if (Position.GetDirtyAndClear(out var positionData))
                builder.SetPosition(positionData.ToData());

            if (Rotation.GetDirtyAndClear(out var rotationData))
                builder.SetRotation(rotationData.ToData());

            data = builder.Build();

            return true;
        }
        else
        {
            data = null;
            return false;
        }
    }

    public LocatorActionData.Builder GetCreationAction()
    {
        var creationData = LocatorCreationData.CreateBuilder()
            .SetType(LocatorType)
            .SetPosition(Position.Value.ToData())
            .SetRotation(Rotation.Value.ToData());

        var creationAction = LocatorActionData.CreateBuilder()
            .SetLocatorId(LocatorID)
            .SetLocatorAction(ObjectActionType.kCreated)
            .SetLocatorCreationData(creationData);

        return creationAction;
    }

    public LocatorStateData GetInitialLocatorStateData()
    {
        return LocatorStateData.CreateBuilder()
            .SetLocatorId(mLocatorID)
            .SetPosition(Position.Value.ToData())
            .SetRotation(Rotation.Value.ToData())
            //.SetIsActivated(IsActivated.Value)
            .Build();
    }

    public void SetState(LocatorStateData data)
    {
        if (data.LocatorId != mLocatorID)
            return;

        //if (data.HasIsActivated)
        //    IsActivated.Value = data.IsActivated;

        if (data.HasPosition)
            Position.Value = data.Position.ToVector3();

        if (data.HasRotation)
            Rotation.Value = data.Rotation.ToQuaternion();

        //if (data.HasPosition)
        //    mDestinationPosition = data.Position.ToVector3();
        //
        //if (data.HasRotation)
        //    mDestinationRotation = data.Rotation.ToQuaternion();
    }

    public void SetState(Vector3 position, Quaternion rotation)
    {
        Position.Value = position;
        Rotation.Value = rotation;
    }

    #endregion
}
