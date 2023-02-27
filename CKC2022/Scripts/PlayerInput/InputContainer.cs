using Network.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace CKC2022.Input
{
    public class InputContainer
    {
        public HumanoidPlayerInput Input { get; private set; }
        public ReplicatedHumanoidEntityInteractor Interactor { get; private set; }
        public event Action<ReplicatedHumanoidEntityInteractor> OnInteractorBinded;

        public event Action<InputContainer, bool> OnAttack;
        public event Action<InputContainer, bool> OnRevive;
        public event Action<InputContainer, bool> OnJump;
        public event Action<InputContainer, ReplicableItemObject> OnWeaponSelected;
        public event Action<InputContainer, int> OnCheckPointInteract;
        public event Action<InputContainer> OnWeaponReleased;
        public event Action<int> OnWeaponSwapSelected; // range 1~3

        private Vector2 LastCharacterViewDirection;
        public readonly Notifier<Vector2> RawViewDirection = new(); //contains zero
        public readonly Notifier<Vector2> CharacterLookDirection = new(); //Never zero
        public readonly Notifier<Vector2> CameraSpaceMovementDirection = new();
        public readonly Notifier<Vector3> ViewPosition = new();
        public readonly Notifier<Vector2> RawViewVector= new();
        private Vector2 SmoothCameraSpaceMovementDirection;

        private CoroutineWrapper UpdateRoutine;
        private CoroutineWrapper AutofireRoutine;

        public void BindInput(in HumanoidPlayerInput input)
        {
            if (Input != null)
            {
                
                Input.CameraSpaceMovement.OnDataChanged -= Input_CameraSpaceMovement_OnDataChanged;
                Input.ViewPosition.OnDataChanged -= Input_ViewPosition_OnDataChanged;
                Input.View.OnDataChanged -= Input_View_OnDataChanged;

                Input.Attack.OnDataChanged -= Attack_Raiser;
                Input.Revive.OnDataChanged -= Revive_Raiser;
                Input.Jump.OnDataChanged -= Jump_Raiser;
                
                Input.Interaction.OnDataChanged -= Interaction_OnDataChanged;
                Input.Release.OnDataChanged -= Release_OnDataChanged;
                Input.WeaponeSwapSelection -= WeaponeSwapSelection_OnDataChanged;
            }

            Input = input;
            Input.Attack.OnDataChanged += Attack_Raiser;
            Input.Revive.OnDataChanged += Revive_Raiser;
            Input.Jump.OnDataChanged += Jump_Raiser;
            Input.Interaction.OnDataChanged += Interaction_OnDataChanged;
            Input.Release.OnDataChanged += Release_OnDataChanged;
            Input.WeaponeSwapSelection += WeaponeSwapSelection_OnDataChanged;

            Input.View.OnDataChanged += Input_View_OnDataChanged;
            Input.ViewPosition.OnDataChanged += Input_ViewPosition_OnDataChanged;
            Input.CameraSpaceMovement.OnDataChanged += Input_CameraSpaceMovement_OnDataChanged;

            UpdateRoutine = new CoroutineWrapper(input);
            UpdateRoutine.StartSingleton(Update());
        }


        private IEnumerator Update()
        {
            var waitInteractor = new WaitUntil(() => Interactor != null);
            while (UpdateRoutine.Runner.enabled)
            {
                yield return waitInteractor;
                SmoothMovementInput();
                UpdateCharacterRotation();
                TryUseWeapon();
                yield return null;
            }
        }

        private void SmoothMovementInput()
        {
            var moveDir = CameraSpaceMovementDirection.Value;
            if (moveDir.magnitude <= Vector2.kEpsilon)
                moveDir = LastCharacterViewDirection;

            var smooth = Quaternion.LookRotation(SmoothCameraSpaceMovementDirection.ToVector3FromXZ());
            var current = Quaternion.LookRotation(moveDir.ToVector3FromXZ());
            var smoothRot = Quaternion.Slerp(smooth, current, 0.32f);
            SmoothCameraSpaceMovementDirection = (smoothRot * Vector3.forward * moveDir.magnitude).ToXZ();
        }

        private void UpdateCharacterRotation()
        {
            if (Interactor == null)
                return;

            LastCharacterViewDirection = CharacterLookDirection.Value;
            CharacterLookDirection.Value = CalculateCharacterRotation(Interactor.LookAtWeight);

            //local function
            Vector2 CalculateCharacterRotation(float weight)
            {
                var view = RawViewDirection.Value;

                var movement = SmoothCameraSpaceMovementDirection;
                var last = LastCharacterViewDirection;
                var rotation = Interactor.BindingTarget.Rotation.Value.eulerAngles.ToXZ();
                var defaultRotation = Vector2.down;

                Vector2 controlDirection = true switch
                {
                    true when movement.magnitude >= Vector2.kEpsilon => movement,
                    true when last.magnitude >= Vector2.kEpsilon => last,
                    true when rotation.magnitude >= Vector2.kEpsilon => rotation,
                    _ => defaultRotation
                };

                return Vector2.Lerp(controlDirection, view, weight);
            }
        }

        private void TryUseWeapon()
        {
            if (!Input.Attack.Value)
                return;

            if (Interactor == null)
                return;

            if (!Interactor.CheckWeaponCanBeUsed())
                return;

            OnAttack?.Invoke(this, true);
        }

        public bool BindInteractor(in ReplicatedHumanoidEntityInteractor interactor)
        {
            bool changed = !EqualityComparer<ReplicatedHumanoidEntityInteractor>.Default.Equals(Interactor, interactor);

            if (Interactor != null)
            {
                Interactor.SetActiveInteraction(false);
            }

            Interactor = interactor;

            if (Interactor != null)
            {
                Interactor.SetActiveInteraction(true);
            }

            OnInteractorBinded?.Invoke(Interactor);

            return changed;
        }

        //Update Values

        private void Input_View_OnDataChanged(Vector2 direction)
        {
            RawViewDirection.Value = direction;
            RawViewVector.Value = direction;
        }

        private void Input_ViewPosition_OnDataChanged(Vector2 position)
        {
            if (Interactor == null)
                return;

            var ray = Camera.main.ScreenPointToRay(position);
            var worldPosition = VectorExtension.ProjectionToYAxis(ray.origin + ray.direction * 1000, ray.origin, Interactor.BindingTarget.Position.Value.y);
            RawViewDirection.Value = (worldPosition - Interactor.BindingTarget.Position.Value).ToXZ().normalized;
            ViewPosition.Value = worldPosition;
            RawViewVector.Value = (worldPosition - Interactor.BindingTarget.Position.Value).ToXZ();
        }

        private void Input_CameraSpaceMovement_OnDataChanged(Vector2 movement)
        {
            CameraSpaceMovementDirection.Value = movement;
        }

        private void Interaction_OnDataChanged(bool isDown)
        {
            if (CheckPointManager.TryGetInstance(out var checkPointManager))
            {
                Vector3 playerPosition = Interactor.BindingTarget.Position.Value;

                if (checkPointManager.TryGetInteractableCheckPointNumber(playerPosition, out int checkPointNumber))
                {
                    var checkPointSystem = ClientSessionManager.Instance.GameGlobalState.GameGlobalState.CheckPointSystem;

                    if (checkPointSystem.TryGetCheckPointWeapon(checkPointNumber, out var weaponItem))
                    {
                        OnCheckPointInteract?.Invoke(this, checkPointNumber);
                        return;
                    }
                }
            }

            if (Interactor.HoverWeapon.Value == null)
                return;

            if (isDown)
                OnWeaponSelected?.Invoke(this, Interactor.HoverWeapon.Value);
        }
        
        private void Release_OnDataChanged(bool isDown)
        {
            if (isDown)
                OnWeaponReleased?.Invoke(this);
        }
        
        private void WeaponeSwapSelection_OnDataChanged(int selection)
        {
            OnWeaponSwapSelected?.Invoke(selection);
        }

        //raising Events
        private void Revive_Raiser(bool isDown) => OnRevive?.Invoke(this, isDown);

        private void Attack_Raiser(bool isDown)
        {
            if (isDown)
                TryUseWeapon();
            else
                OnAttack?.Invoke(this, isDown);
        }

        private void Jump_Raiser(bool isDown) => OnJump?.Invoke(this, isDown);


        public void ReleaseInteractor()
        {
            Interactor = null;
        }

        public void ReleaseEvent()
        {
            if (Input != null)
            {
                Input.Attack.OnDataChanged -= Attack_Raiser;
                Input.Revive.OnDataChanged -= Revive_Raiser;
                Input.Jump.OnDataChanged -= Jump_Raiser;
            }
        }
    }
}