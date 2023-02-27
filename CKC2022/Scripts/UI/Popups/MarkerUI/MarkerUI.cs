using CulterLib.Presets;
using CulterLib.Types;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterUI;

public class MarkerUI : PopupWindow
{
    public static MarkerUI Instance { get; private set; }

    #region Inspector
    [TabGroup("Component"), SerializeField] private MarkerObject[] m_OriginalMarker;
    #endregion
    #region Value
    private UIObjectPool<MarkerObject> m_MarkerPool;
    private Dictionary<CharacterUI, MarkerObject> m_MarkerDic = new Dictionary<CharacterUI, MarkerObject>();
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
        m_MarkerPool = new UIObjectPool<MarkerObject>(m_OriginalMarker, GlobalManager.Instance.DataMgr.vPlayerMaxCount, true, (marker) => AddChildUI(marker));
    }
    #endregion
    #region Function
    //Public
    /// <summary>
    /// 캐릭터UI를 등록합니다.
    /// </summary>
    /// <param name="_chrUI"></param>
    public void AddPlayerChrUI(CharacterUI _chrUI)
    {
        if (m_MarkerDic.ContainsKey(_chrUI))
            return;

        var marker = (_chrUI.ChrUIType == ECharacterUIType.Player) ? m_MarkerPool.GetObject() : null;
        marker?.SetTarget(_chrUI);
        m_MarkerDic.Add(_chrUI, marker);
    }
    /// <summary>
    /// 캐릭터 UI를 제거합니다.
    /// </summary>
    /// <param name="_chrUI"></param>
    internal void RemovePlayerChrUI(CharacterUI _chrUI)
    {
        if (!m_MarkerDic.ContainsKey(_chrUI))
            return;

        var marker = m_MarkerDic[_chrUI];
        if (marker != null)
        {
            marker.SetTarget(null);
            marker.gameObject.SetActive(false);
        }
        m_MarkerDic.Remove(_chrUI);
    }
    #endregion
}
