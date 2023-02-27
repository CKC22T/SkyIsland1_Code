using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CheckPointPopup : PopupWindow
{
    public static CheckPointPopup Instance { get; private set; }

    #region Inspector
    [TabGroup("Component"), SerializeField] TextMeshProUGUI m_CheckPointName;
    #endregion

    #region Param
    [SerializeField] private float m_ActivePopupTime;
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
    }

    protected override void OnStartOpen(string _opt)
    {
        base.OnStartOpen(_opt);

        m_CheckPointName.text = _opt;

        StartCoroutine(autoClose());
        IEnumerator autoClose()
        {
            yield return new WaitForSeconds(m_ActivePopupTime);
            Close();
        }
    }
    #endregion
}
