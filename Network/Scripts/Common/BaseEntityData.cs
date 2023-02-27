using System;
using Network.Packet;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using static Network.Packet.Response.Types;

public abstract class BaseEntityData : MonoBehaviour
{
    // Base entity information data
    public int EntityID;
    public EntityType EntityType;
    public FactionType FactionType;

    // Server controlled data
    public readonly Notifier<bool> IsEnabled = new Notifier<bool>(true);
    public readonly Notifier<int> BindedClientID = new Notifier<int>();
    public readonly Notifier<ItemType> EquippedWeaponType = new Notifier<ItemType>(ItemType.kNoneItemType);
    [Obsolete("무기 엔티티를 줍는 기능은 더 이상 사용하지 않음")]
    public readonly Notifier<int> EquippedWeaponEntityID = new Notifier<int>(-1);

    // Transform data
    public readonly Notifier<Vector3> Position = new Notifier<Vector3>();
    public readonly Notifier<Quaternion> Rotation = new Notifier<Quaternion>();
    public readonly Notifier<Vector3> Velocity = new Notifier<Vector3>();

    // Gameplay state data
    public readonly Notifier<bool> IsAlive = new Notifier<bool>(true);
    public readonly Notifier<int> Hp = new Notifier<int>(100);
    public int MaxHp { get; private set; }

    // Entity action data buffer
    public List<EntityActionData> ActionDataBuffer = new List<EntityActionData>(8);

    protected int mInitialLayerIndex = 0;
    protected bool mOnlyGroundPhysicsEnable;
    public bool OnlyGroundPhysicsEnable
    {
        get => mOnlyGroundPhysicsEnable;
        set
        {
            mOnlyGroundPhysicsEnable = value;
            if (mOnlyGroundPhysicsEnable)
            {
                gameObject.layer = Global.LayerIndex_Deactivated;
            }
            else
            {
                gameObject.layer = mInitialLayerIndex;
            }
        }
    }

    // For debug
    [Header("Server Controlled data")]
    [SerializeField] private bool mIsEnabled;
    [SerializeField] private int mBindedClientID;
    [SerializeField] private int mEquippedWeaponEntityID;

    [Header("Transform Data")]
    [SerializeField] private Vector3 mPosition;
    [SerializeField] private Quaternion mRotation;
    [SerializeField] private Vector3 mVelocity;

    [Header("Gameplay state data")]
    [SerializeField] private int mHp;

    [SerializeField] private string CurrentWeapon;

    private void Start()
    {
        MaxHp = Hp.Value;
        Hp.OnChanged += () =>
        {
            if (MaxHp <= Hp.Value)
                MaxHp = Hp.Value;
        };
    }
    public void Update()
    {
        mIsEnabled = IsEnabled.Value;
        mBindedClientID = BindedClientID.Value;
        mEquippedWeaponEntityID = EquippedWeaponEntityID.Value;

        mPosition = Position.Value;
        mRotation = Rotation.Value;
        mVelocity = Velocity.Value;

        mHp = Hp.Value;

        CurrentWeapon = EquippedWeaponType.Value.GetItemName();
    }
}
