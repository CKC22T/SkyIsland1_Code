using System.Collections;
using System.Linq;
using UnityEngine;

using Network.Packet;
using Utils;
using System.Collections.Concurrent;
using CKC2022;
using CKC2022.Input;
using Network.Common;

namespace Network.Client
{
    public class ReplicatedEntityController : MonoBehaviour
    {
        [SerializeField]
        private ReplicatedEntityData replicatedData;

        private readonly Notifier<InputContainer> InputContainer = new();

        [SerializeField]
        private CharacterPhysics mPhysics;

        [SerializeField]
        private Vector2 InputCameraSpaceMovement;

        [SerializeField]
        private Vector2 CharacterViewDirection;

        public float MoveSpeed = 2f;
        private bool Snap = false;

        private float InitializeTime;
        private readonly ConcurrentStack<Vector3> positionQueue = new ConcurrentStack<Vector3>();
        private readonly ConcurrentStack<Quaternion> rotationQueue = new ConcurrentStack<Quaternion>();

        private void Awake()
        {
            replicatedData.OnInitialized += ReplicatedData_OnInitialized;
            replicatedData.BindedClientID.OnDataChanged += BindedClientID_OnDataChanged;
            replicatedData.OnSnap += ReplicatedData_OnSnap;

            if (replicatedData.EntityType.GetEntityBaseType() == EntityBaseType.Humanoid)
            {
                InputContainer.OnDataChangedDelta += InputContainer_OnDataChangedDelta;
            }
        }

        private void ReplicatedData_OnSnap()
        {
            Snap = true;
        }

        private void ReplicatedData_OnInitialized(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            predictPosition = position;
            prevPosition = position;

            transform.rotation = rotation;
            prevRotation = rotation;
            predictRotation = rotation;

            InitializeTime = Time.time;
            Snap = true;
        }

        private void OnEnable()
        {
            replicatedData.Position.OnDataChanged += ForceUpdate_Position_OnDataChanged;
            replicatedData.Rotation.OnDataChanged += ForceUpdate_Rotation_OnDataChanged;
            
            Snap = true;
        }

        
        private void BindedClientID_OnDataChanged(int clientID)
        {
            if (replicatedData.IsMine)
            {
                if (PlayerInputNetworkManager.TryGetInputContainer(ClientSessionManager.Instance.SessionID, out var container))
                    InputContainer.Value = container;
            }
            else
            {
                if (InputContainer.Value != null && InputContainer.Value.Input != null && InputContainer.Value.Input.PlayerID != clientID)
                    InputContainer.Value = null;
            }
        }

        private void InputContainer_OnDataChangedDelta(InputContainer prev, InputContainer next)
        {
            if (prev != null)
            {
                prev.CharacterLookDirection.OnDataChanged -= View_OnDataChanged;
                prev.CameraSpaceMovementDirection.OnDataChanged -= MovementInputDirection_OnDataChanged;
            }

            if (next != null)
            {
                next.CharacterLookDirection.OnDataChanged += View_OnDataChanged;
                next.CameraSpaceMovementDirection.OnDataChanged += MovementInputDirection_OnDataChanged;
            }
        }

        private void View_OnDataChanged(Vector2 viewDir)
        {
            CharacterViewDirection = viewDir;
        }

        private void MovementInputDirection_OnDataChanged(Vector2 movement)
        {
            InputCameraSpaceMovement = movement;
        }

        private void FixedUpdate()
        {
            if (replicatedData.EntityType.GetEntityBaseType() == EntityBaseType.Humanoid && replicatedData.IsMine)
            {
                PredictTransformAndUpdate(false);

                var info = new PlayerEntityBehavior.MovementSimulationInformation
                {
                    moveDirection = InputCameraSpaceMovement,
                    ChracterViewDirection = CharacterViewDirection,
                    moveSpeed = MoveSpeed,
                    deltaTime = Time.deltaTime
                };

                var isCalculated = PlayerEntityBehavior.CalculateMoving(transform, info, out var result);

                if (isCalculated && result.HasPosition)
                    transform.position = Vector3.Lerp(predictPosition, result.Position, 0.75f);
                else
                    transform.position = Vector3.Lerp(transform.position, prevPosition, 0.35f);

                if (isCalculated && result.HasRotation)
                    transform.rotation = Quaternion.Slerp(predictRotation, result.Rotation, 0.75f);
                else
                    transform.rotation = Quaternion.Slerp(transform.rotation, prevRotation, 0.27f);

            }
            else if (replicatedData.IsEnabled.Value == true)
            {
                PredictTransformAndUpdate(true);
            }
        }

        private Vector3 prevPosition;
        private Vector3 predictPosition;
        private Quaternion prevRotation;
        private Quaternion predictRotation;

        [SerializeField]
        private float velocityFactor = 0.5f;

        private float UpdateTime;

        private void PredictTransformAndUpdate(bool withUpdate)
        {
            if (rotationQueue.TryPop(out var rotation))
            {
                predictRotation = rotation * (rotation * Quaternion.Inverse(prevRotation));
                prevRotation = rotation;
                UpdateTime = 0;
            }

            if (positionQueue.TryPop(out var position))
            {
                var diff = position - prevPosition;
                predictPosition = position + diff * velocityFactor;
                prevPosition = position;
                UpdateTime = 0;
            }

            if (withUpdate)
            {
                if (Time.time - InitializeTime < 0.25f || Snap)
                {
                    //Snap
                    transform.SetPositionAndRotation(replicatedData.Position.Value, replicatedData.Rotation.Value);
                    
                    Snap = false;
                }
                else
                {
                    //interpolation
                    var targetPosition = Vector3.Lerp(transform.position, Vector3.Lerp(prevPosition, predictPosition, UpdateTime), 0.35f);
                    var targetRotation = Quaternion.Slerp(transform.rotation, Quaternion.Slerp(prevRotation, predictRotation, UpdateTime), 0.27f);
                    transform.SetPositionAndRotation(targetPosition, targetRotation);
                }
            }

            UpdateTime += Time.fixedDeltaTime;
        }

        private Quaternion QuaternionSimpleExploation(Quaternion q1, Quaternion q2, float dt)
        {
            (q2 * Quaternion.Inverse(q1)).ToAngleAxis(out var ang, out var axis);

            if (ang > 180)
                ang -= 360;

            ang = ang * dt % 360;

            return Quaternion.AngleAxis(ang, axis) * q1;
        }


        //HardSnapping
        private void ForceUpdate_Rotation_OnDataChanged(Quaternion rotation)
        {
            if (replicatedData.IsEnabled.Value == false)
                return;

            rotationQueue.Clear();
            rotationQueue.Push(rotation);
        }

        private void ForceUpdate_Position_OnDataChanged(Vector3 position)
        {
            if (replicatedData.IsEnabled.Value == false)
                return;

            positionQueue.Clear();
            positionQueue.Push(position);
        }

        private void OnDisable()
        {
            replicatedData.Position.OnDataChanged -= ForceUpdate_Position_OnDataChanged;
            replicatedData.Rotation.OnDataChanged -= ForceUpdate_Rotation_OnDataChanged;
        }

    }
}