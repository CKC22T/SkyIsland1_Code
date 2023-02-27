using CKC2022.Input;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipUI : PopupWindow
{
    public static EquipUI Instance { get; private set; }
    #region Inspector
    [Title("GameBaseUI")]
    [TabGroup("Component"), SerializeField] private EquipObject m_Interact;
    [TabGroup("Component"), SerializeField] private CheckPointEquipObject m_CheckPointInteract;
    #endregion

    #region Event
    protected override void OnInitSingleton()
    {
        base.OnInitSingleton();
        Instance = this;
    }
    protected override void OnInitData()
    {
        base.OnInitData();

        //구성요소 초기화
        m_Interact.gameObject.SetActive(false);
        m_CheckPointInteract.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (PlayerInputNetworkManager.TryGetAnyInputContainer(out var target) && target.Interactor && target.Interactor.HoverWeapon.Value)
            m_Interact.Open(target.Interactor.HoverWeapon.Value);
        else
            m_Interact.Close();


        if (CheckPointManager.TryGetInstance(out var checkPointManager))
        {
            //Vector3 playerPosition = Interactor.BindingTarget.Position.Value;
            if (PlayerInputNetworkManager.TryGetAnyInputContainer(out var player))
            {
                if (player.Interactor == null)
                    return;
                
                Vector3 playerPosition = player.Interactor.BindingTarget.Position.Value;

                if (checkPointManager.TryGetInteractableCheckPointNumber(playerPosition, out int checkPointNumber))
                {
                    var checkPointSystem = ClientSessionManager.Instance.GameGlobalState.GameGlobalState.CheckPointSystem;

                    if (checkPointSystem.TryGetCheckPointWeapon(checkPointNumber, out var weaponItem))
                    {
                        if (CheckPointManager.Instance.TryGetItemShowerPosition(checkPointNumber, out var position))
                        {
                            m_CheckPointInteract.Open(weaponItem, position);
                            return;
                        }
                    }
                }
            }
        }
        m_CheckPointInteract.Close();
    }
    #endregion
}
