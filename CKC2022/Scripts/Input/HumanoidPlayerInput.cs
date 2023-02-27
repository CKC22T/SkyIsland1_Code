using Network.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace CKC2022.Input
{
    public class HumanoidPlayerInput : BasePlayerInput
    {
        public readonly Notifier<Vector2> RawMovement = new();
        public readonly Notifier<Vector2> CameraSpaceMovement = new();
        public readonly Notifier<Vector2> View = new();
        public readonly Notifier<Vector2> ViewPosition = new();
        public readonly Notifier<bool> Attack = new();
        public readonly Notifier<bool> Interaction = new();
        public readonly Notifier<bool> Revive = new();
        public readonly Notifier<bool> Jump = new();
        public readonly Notifier<bool> Release = new();
        public event Action<int> WeaponeSwapSelection;

        private void Awake()
        {
            PlayerInputNetworkManager.Instance.AddInitializeInvocation(this);
        }

        //Override Input
        public override void OnMovement(InputAction.CallbackContext value)
        {
            RawMovement.Value = value.ReadValue<Vector2>();
            CameraSpaceMovement.Value = Camera.main.transform.TransformDirection(RawMovement.Value.ToVector3FromXZ()).ToXZ().normalized;
        }

        public override void OnView(InputAction.CallbackContext value)
        {
            View.Value = value.ReadValue<Vector2>();
        }

        public override void OnViewPosition(InputAction.CallbackContext value)
        {
            ViewPosition.Value = value.ReadValue<Vector2>();
        }


        //This is called from PlayerInput, when a button has been pushed, that corresponds with the 'Attack' action
        public override void OnAttack(InputAction.CallbackContext value)
        {
            if (value.started)
                Attack.Value = true;
            if (value.canceled)
                Attack.Value = false;
        }

        public override void OnRevive(InputAction.CallbackContext value)
        {
            if (value.started)
                Revive.Value = true;
            if (value.canceled)
                Revive.Value = false;
        }

        public override void OnInteraction(InputAction.CallbackContext value)
        {
            if (value.started)
                Interaction.Value = true;
            if (value.canceled)
                Interaction.Value = false;
        }

        public override void OnJump(InputAction.CallbackContext value)
        {
            if (value.started)
                Jump.Value = true;
            if (value.canceled)
                Jump.Value = false;
        }

        public override void OnRelease(InputAction.CallbackContext value)
        {
            base.OnRelease(value);

            if (value.started)
                Release.Value = true;
            if (value.canceled)
                Release.Value = false;
        }

        public override void OnKeyPress(InputAction.CallbackContext value)
        {
            base.OnRelease(value);

            if (value.started && int.TryParse(value.control.name, out var number))
            {
                WeaponeSwapSelection?.Invoke(number);
            }

        }
    }
}