using CulterLib.Types;
using Network.Common;
using Network.Packet;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils;

public class CheckPointEquipObject : Mono_UI
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private TextMeshProUGUI m_WeaponName;
    [TabGroup("Component"), SerializeField] private TextMeshProUGUI m_WeaponInfo;
    [TabGroup("Component"), SerializeField] private TextMeshProUGUI m_WeaponStat;
    //[TabGroup("Component"), SerializeField] private Image m_Gauge;
    #endregion
    #region Value
    private ItemType m_TargetType;
    private Vector3 m_TargetPosition;
    [SerializeField] private Vector3 offset;
    #endregion

    #region Event
    protected override void OnInitData()
    {
        base.OnInitData();
    }

    private void FixedUpdate()
    {
        //위치 설정
        if (m_TargetPosition != null)
        {
            Vector3 position = m_TargetPosition + offset;
            transform.localPosition = position.WorldToCanvas(transform.parent as RectTransform);
        }
    }
    #endregion
    #region Function
    public void Open(ItemType _weapon, Vector3 position)
    {
        m_TargetPosition = position;
        SetTargetObject(_weapon);
        gameObject.SetActive(true);
    }

    private void SetTargetObject(ItemType target)
    {
        m_TargetType = target;
        m_WeaponName.text = target.GetItemName();
        m_WeaponInfo.text = target.GetItemAdditionalInfo();
        m_WeaponStat.text = target.GetItemStatInfo();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
    #endregion
}
