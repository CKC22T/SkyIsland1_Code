using Network;
using Network.Packet;
using System;
using System.Collections;
using UnityEngine;
using Utils;
using static Network.Packet.Response.Types;

public class ReplicableItemObject : MonoBehaviour
{
    [Sirenix.OdinInspector.Button]
    public void DestroyManuallyAsMaster()
    {
        OnDestroyAsMaster();
    }

    [SerializeField] private Rigidbody ItemRigid;
    [field: SerializeField] public int ItemObjectID { get; private set; } = 0;
    [field: SerializeField] public ItemType ItemType { get; private set; } = ItemType.kNoneItemType;

    [SerializeField] public Notifier<Vector3> Position;
    [SerializeField] public Notifier<Quaternion> Rotation;

    private NetworkMode mNetworkMode = NetworkMode.None;

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

    public void Update()
    {
        if (mNetworkMode == NetworkMode.Remote)
        {
            if (Vector3.Distance(transform.position, mDestinationPosition) > 30.0f)
            {
                transform.position = mDestinationPosition;
                transform.rotation = mDestinationRotation;
            }

            transform.position = Vector3.Lerp(transform.position, mDestinationPosition, mLerpSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, mDestinationRotation, mLerpSpeed);
        }
    }

    public virtual void Awake()
    {
        Position = new Notifier<Vector3>(transform.position);
        Rotation = new Notifier<Quaternion>(transform.rotation);

        Position.OnDataChanged += setPosition;
        Rotation.OnDataChanged += setRotation;

        mDestinationRotation = transform.rotation;
        mDestinationPosition = transform.position;
    }

    public virtual void OnDestroy()
    {
        Position.OnDataChanged -= setPosition;
        Rotation.OnDataChanged -= setRotation;

        OnDestroyAsMaster();
    }

    public void Initialize(int itemID, ItemType type, Vector3 position, Quaternion rotation)
    {
        // Set item object base information
        ItemObjectID = itemID;
        ItemType = type;

        // Set item object transform
        transform.position = position;
        transform.rotation = rotation;

        Position.Set(position, false);
        Rotation.Set(rotation, false);

        mDestinationPosition = position;
        mDestinationRotation = rotation;
    }

    public void ForceAsMaster(Vector3 force)
    {
        ItemRigid.AddForce(force, ForceMode.Impulse);
    }

    public void OnDestroyAsMaster()
    {
        if (mNetworkMode == NetworkMode.Master && ItemObjectManager.TryGetInstance(out var itemObjectManager))
        {
            itemObjectManager.DestroyItemObjectAsMaster(ItemObjectID);
        }
    }

    public void OnDestroyAsRemote()
    {
        if (this != null && gameObject != null)
        {
            PoolManager.ReleaseObject(gameObject);
        }
    }

    private void setPosition(Vector3 position) => mDestinationPosition = position;
    private void setRotation(Quaternion rotation) => mDestinationRotation = rotation;

    public void SetNetworkMode(NetworkMode networkMode) => mNetworkMode = networkMode;
    public void SetItemObjectID(int itemID) => ItemObjectID = itemID;

    public bool TryGetChangedItemObjectStateDataOrNull(out ItemObjectStateData data)
    {
        if (Position.IsDirty || Rotation.IsDirty)
        {
            var builder = ItemObjectStateData.CreateBuilder()
                .SetItemId(ItemObjectID);

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

    public ItemObjectActionData.Builder GetCreationAction()
    {
        var creationData = ItemObjectCreationData.CreateBuilder()
            .SetType(ItemType)
            .SetPosition(Position.Value.ToData())
            .SetRotation(Rotation.Value.ToData());

        var creationAction = ItemObjectActionData.CreateBuilder()
            .SetItemId(ItemObjectID)
            .SetItemObjectAction(ObjectActionType.kCreated)
            .SetItemObjectCreationData(creationData);

        return creationAction;
    }

    public ItemObjectStateData GetInitialItemObjectStateData()
    {
        return ItemObjectStateData.CreateBuilder()
            .SetItemId(ItemObjectID)
            .SetPosition(Position.Value.ToData())
            .SetRotation(Rotation.Value.ToData())
            .Build();
    }

    public void SetState(ItemObjectStateData data)
    {
        if (data.ItemId != ItemObjectID)
            return;

        if (data.HasPosition)
            Position.Value = data.Position.ToVector3();

        if (data.HasRotation)
            Rotation.Value = data.Rotation.ToQuaternion();
    }

    public void SetState(Vector3 position, Quaternion rotation)
    {
        Position.Value = position;
        Rotation.Value = rotation;
    }
}
