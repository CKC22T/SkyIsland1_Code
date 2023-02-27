using CKC2022;
using CKC2022.Input;
using CulterLib.UI.Popups;
using Network;
using Network.Client;
using Network.Packet;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils;

public class WeaponChangePopup : PopupWindow
{
    public static WeaponChangePopup Instance { get; private set; }

    private Coroutine closeCoroutine;
    private Coroutine changeCoroutine;
    private Transform m_targetAt;
    [SerializeField] private float selectSpeed = 2.0f;

    #region Inspector
    [TabGroup("Component"), SerializeField] Transform m_WeaponSelector;
    [TabGroup("Component"), SerializeField] List<WeaponSlotUI> m_WeaponSlots;
    [TabGroup("Component"), SerializeField] Vector3 m_offset;
    [TabGroup("Component"), SerializeField] RectTransform m_ControlRoot;
    #endregion

    private int mClientWeaponPointer = 0;
    private float mPointerDelay = 0;
    private float mInitialDelay = ServerConfiguration.MaxLatency;

    #region Event
    protected override void OnInitSingleton()
    {
        base.OnInitSingleton();
        Instance = this;
    }
    protected override void OnInitData()
    {
        base.OnInitData();
    }

    private void FixedUpdate()
    {
        if (m_targetAt)
        {
            Vector3 position = m_targetAt.position + m_offset;
            m_ControlRoot.localPosition = position.WorldToCanvas(transform as RectTransform);
        }

        if (mPointerDelay > 0)
        {
            mPointerDelay -= Time.fixedDeltaTime;
        }

        if (ClientSessionManager.Instance.TryGetMyInventory(out var myInventory))
        {
            if(myInventory.GetWeaponCount() <= 0)
            {
                Close();
                return;
            }

            int applyWeaponPointer = mPointerDelay > 0 ? mClientWeaponPointer : myInventory.WeaponPointer.Value;

            foreach (var w in m_WeaponSlots)
            {
                var currentSlotWeapon = myInventory.GetWeaponByIndex(w.WeaponSlotNumber);
                w.SetSlot(currentSlotWeapon);
                w.SetWeaponPointer(applyWeaponPointer);
            }

            var weaponUI = m_WeaponSlots[applyWeaponPointer].transform;

            if ((m_WeaponSelector.localRotation.eulerAngles - weaponUI.localRotation.eulerAngles).magnitude > 0.001f)
            {
                m_WeaponSelector.localRotation = Quaternion.Slerp(m_WeaponSelector.localRotation, weaponUI.localRotation, Time.fixedDeltaTime * selectSpeed);
                if (closeCoroutine != null)
                {
                    StopCoroutine(closeCoroutine);
                    closeCoroutine = null;
                }
            }
            else
            {
                m_WeaponSelector.localRotation = weaponUI.localRotation;
                if (closeCoroutine == null)
                {
                    closeCoroutine = StartCoroutine(autoClose());
                }
            }
        }
    }

    protected override void OnStartOpen(string _opt)
    {
        base.OnStartOpen(_opt);

        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        if (ClientWorldManager.Instance.TryGetMyEntity(out var player))
        {
            if (player.TryGetComponent(out PlaceHolder holder))
            {
                m_targetAt = holder[PlaceHolder.PlaceType.Neck];
            }
        }

        if (int.TryParse(_opt, out var selectedWeapon))
        {
            mClientWeaponPointer = selectedWeapon;
            mPointerDelay = mInitialDelay;
        }
    }

    private IEnumerator autoClose()
    {
        yield return new WaitForSeconds(1.0f);
        Close();
        closeCoroutine = null;
    }
    #endregion
}
