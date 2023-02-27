using CulterLib.Presets;
using CulterLib.Types;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class MarkerObject : Mono_UI
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private RectTransform m_RectTransform;
    [TabGroup("Component"), SerializeField] private GameObject m_MarkerRoot;
    [TabGroup("Component"), SerializeField] private Image mGlow;
    [TabGroup("Component"), SerializeField] private Image m_Character;
    [TabGroup("Component"), SerializeField] private Image m_Death;
    #endregion
    #region Value
    [TabGroup("Debug"), SerializeField, ReadOnly] private CharacterUI m_Target;
    #endregion

    #region Event
    //Unity Event
    private void FixedUpdate()
    {
        if (m_Target)
        {
            var angle = Vector3.Angle((m_Target.transform.position - Camera.main.transform.position), Camera.main.transform.forward);
            var rect = (m_RectTransform.parent as RectTransform).rect;
            var pos = (m_Target.transform.position).WorldToCanvas(m_RectTransform.parent as RectTransform);
            if (90 < angle)
                pos = -pos;
            var clampedPos = new Vector2(Mathf.Clamp(pos.x, rect.min.x, rect.max.x), Mathf.Clamp(pos.y, rect.min.y, rect.max.y));
            var dir = pos.normalized;
            bool isOut = pos != clampedPos;

            m_MarkerRoot.SetActive(isOut);
            transform.localPosition = clampedPos;
            transform.localEulerAngles = new Vector3(0, 0, Vector2.Angle(Vector2.down, dir) * (0 < dir.x ? 1 : -1));
        }
    }

    //Data Event
    private void OnIsAliveChanged(bool isAlive)
    {
        m_Character.gameObject.SetActive(isAlive);
        m_Death.gameObject.SetActive(!isAlive);
    }
    #endregion
    #region Function
    /// <summary>
    /// 누구의 마커를 표시할지 설정합니다.
    /// </summary>
    /// <param name="_tar"></param>
    public void SetTarget(CharacterUI _tar)
    {
        if (m_Target)
        {
            RemoveDataChangeEvent(m_Target.Data.IsAlive, OnIsAliveChanged);
        }
        m_Target = _tar;
        if (m_Target)
        {
            mGlow.color = m_Target.PersonalColor;
            m_Character.color = m_Target.PersonalColor;
            m_Death.color = m_Target.PersonalColor;

            AddDataChangeEvent(m_Target.Data.IsAlive, OnIsAliveChanged, true);
        }
    }
    #endregion
}
