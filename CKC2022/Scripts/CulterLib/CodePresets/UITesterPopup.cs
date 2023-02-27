using CulterLib.Presets;
using CulterLib.Types;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITesterPopup : PopupWindow
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private Button[] mOriBtn;
    #endregion
    #region Value
    public UIObjectPool<Button> mBtnPool;
    #endregion

    #region Inspector
    protected override void OnInitData()
    {
        base.OnInitData();

        mBtnPool = new UIObjectPool<Button>(mOriBtn, UIManager.Instance.PopMgr.InitPopup.Count, true);
        foreach(var v in UIManager.Instance.PopMgr.InitPopup)
        {
            var pop = v;
            if (!pop.IsBasePopup)
            {
                var btn = mBtnPool.GetObject();
                btn.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = pop.name;
                btn.onClick.AddListener(() =>
                {
                    pop.Open();
                });
            }
        }
    }
    #endregion
}
