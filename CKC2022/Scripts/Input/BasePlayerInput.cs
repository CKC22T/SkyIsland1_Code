using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Utils;

public class BasePlayerInput : MonoBehaviour
{
    //Player ID
    public int PlayerID { get; private set; }

    [field: SerializeField]
    public PlayerInput playerInput { get; private set; }

    //Action Maps
    private const string actionMapPlayerControls = "Player Controls";
    private const string actionMapMenuControls = "Menu Controls";

    //Current Control Scheme
    private string currentControlScheme;

    //Get Data ----
    public InputActionAsset ActionAsset { get => playerInput.actions; }


    public void Initialize(in int id)
    {
        PlayerID = id;
        currentControlScheme = playerInput.currentControlScheme;
    }

    //INPUT SYSTEM ACTION METHODS --------------

    //This is called from PlayerInput; when a joystick or arrow keys has been pushed.
    //It stores the input Vector as a Vector3 to then be used by the smoothing function.
    public virtual void OnMovement(InputAction.CallbackContext value)
    {
    }

    public virtual void OnView(InputAction.CallbackContext value)
    {
    }

    public virtual void OnViewPosition(InputAction.CallbackContext value)
    {
    }

    //This is called from PlayerInput, when a button has been pushed, that corresponds with the 'Attack' action
    public virtual void OnAttack(InputAction.CallbackContext value)
    {
    }

    public virtual void OnRevive(InputAction.CallbackContext value)
    {
    }

    public virtual void OnInteraction(InputAction.CallbackContext value)
    {
    }

    public virtual void OnJump(InputAction.CallbackContext value)
    {
    }

    public virtual void OnRelease(InputAction.CallbackContext value)
    {
    }
    
    public virtual void OnKeyPress(InputAction.CallbackContext value)
    {
    }

    //This is called from Player Input, when a button has been pushed, that correspons with the 'TogglePause' action
    public virtual void OnTogglePause(InputAction.CallbackContext value)
    {
    }


    //INPUT SYSTEM AUTOMATIC CALLBACKS --------------

    //This is automatically called from PlayerInput, when the input device has changed
    //(IE: Keyboard -> Xbox Controller)
    public virtual void OnControlsChanged()
    {
        if (playerInput.currentControlScheme != currentControlScheme)
        {
            currentControlScheme = playerInput.currentControlScheme;

            //UpdatePlayerVisuals();
            RemoveAllBindingOverrides();
        }
    }

    //This is automatically called from PlayerInput, when the input device has been disconnected and can not be identified
    //IE: Device unplugged or has run out of batteries
    public void OnDeviceLost()
    {
        //SetDisconnectedDeviceVisuals();
    }

    public void OnDeviceRegained()
    {
        StartCoroutine(WaitForDeviceToBeRegained());
    }

    IEnumerator WaitForDeviceToBeRegained()
    {
        yield return new WaitForSeconds(0.1f);
        //UpdatePlayerVisuals();
    }

    public void SetInputActiveState(bool gameIsPaused)
    {
        switch (gameIsPaused)
        {
            case true:
                playerInput.DeactivateInput();
                break;

            case false:
                playerInput.ActivateInput();
                break;
        }
    }

    void RemoveAllBindingOverrides()
    {
        InputActionRebindingExtensions.RemoveAllBindingOverrides(playerInput.currentActionMap);
    }


    //Switching Action Maps ----
    public void EnableGameplayControls()
    {
        playerInput.SwitchCurrentActionMap(actionMapPlayerControls);
    }

    public void EnablePauseMenuControls()
    {
        playerInput.SwitchCurrentActionMap(actionMapMenuControls);
    }
}