using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StagePopup : PopupWindow
{
    public static StagePopup Instance { get; private set; }

    #region Inspector
    [TabGroup("Component"), SerializeField] TextMeshProUGUI m_StageName;
    [TabGroup("Component"), SerializeField] TextMeshProUGUI m_CheckPointName;
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

        m_StageName.text = GlobalAreaName.StageName;
        m_CheckPointName.text = GlobalAreaName.GetAreaName(0);

        StartCoroutine(autoClose());
        IEnumerator autoClose()
        {
            yield return new WaitForSeconds(1.0f);
            Close();
        }
    }
    #endregion
}
