using CKC2022;
using CKC2022.Input;
using CulterLib.Presets;
using CulterLib.Types;
using Network.Client;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class EquipObject : Mono_UI
{
    //#region Inspector
    //[TabGroup("Component"), SerializeField] private GameObject m_DefaultRoot;
    //[TabGroup("Component"), SerializeField] private GameObject m_TutorialRoot;
    //[TabGroup("Component"), SerializeField] private Image m_Gauge;
    //#endregion
    #region Value
    private GameObject m_TargetObject;
    [SerializeField] private float offsetY = 10.0f;
    #endregion

    #region Event
    protected override void OnInitData()
    {
        base.OnInitData();

        //m_TutorialRoot.SetActive(false);
        //m_DefaultRoot.SetActive(true);
    }

    private void FixedUpdate()
    {
        //위치 설정
        if (m_TargetObject)
        {
            Vector3 position = m_TargetObject.transform.position + Vector3.up * offsetY;
            transform.localPosition = position.WorldToCanvas(transform.parent as RectTransform);
        }

        ////캐릭터와의 거리에 따라 게이지 설정
        //if (!PlayerInputNetworkManager.TryGetAnyInputContainer(out var container))
        //    return;

        //if (container.Interactor.BindingTarget != null)
        //{
        //    var dir = (m_TargetWeapon.Position.Value.ToXZ() - container.Interactor.BindingTarget.transform.position.ToXZ());
        //    m_Gauge.fillAmount = 1 - dir.magnitude / GlobalManager.Instance.DataMgr.vEquipDistance;
        //}
    }
    #endregion
    #region Function
    [System.Obsolete("ReplicatedWeaponEntityData is not used. use ReplicableItemObject instead")]
    public void Open(ReplicatedWeaponEntityData _weapon)
    {
        m_TargetObject = _weapon.gameObject;
        gameObject.SetActive(true);
    }
    
    public void Open(ReplicableItemObject _weapon)
    {
        //throw new System.NotImplementedException("Weapon equipment logic is modified. implement m_TargetWeapon from ReplicableItemObject");
        //TODO : Implement this function
        
        //m_TargetWeapon = _weapon;
        m_TargetObject = _weapon.gameObject;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
    #endregion
}
