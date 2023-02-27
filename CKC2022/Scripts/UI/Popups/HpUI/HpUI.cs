using CulterLib.Types;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterUI;

public class HpUI : PopupWindow
{
    public static HpUI Instance { get; private set; }
    #region Type
    /// <summary>
    /// UI타입별 HP바
    /// </summary>
    [System.Serializable] public struct SHpVariant
    {
        public ECharacterUIType type;
        public HpBar[] original;
    }
    #endregion

    #region Inspector
    [TabGroup("Component"), SerializeField] private SHpVariant[] mOriginalHp;
    #endregion
    #region Value
    private Dictionary<ECharacterUIType, UIObjectPool<HpBar>> m_HPGaugePool;
    private Dictionary<CharacterUI, HpBar> m_HpDic = new Dictionary<CharacterUI, HpBar>();
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
        m_HPGaugePool = new Dictionary<ECharacterUIType, UIObjectPool<HpBar>>();
        foreach (var v in mOriginalHp)
        {
            var pool = new UIObjectPool<HpBar>(v.original, 50, false, (gauge) => AddChildUI(gauge));
            m_HPGaugePool.Add(v.type, pool);
        }
    }
    #endregion
    #region Function
    //Public
    /// <summary>
    /// 캐릭터UI를 등록합니다.
    /// </summary>
    /// <param name="_chrUI"></param>
    public void AddCharacterUI(CharacterUI _chrUI)
    {
        if (m_HpDic.ContainsKey(_chrUI))
            return;

        var hp = _chrUI.UseHPGauge ? m_HPGaugePool[_chrUI.ChrUIType].GetObject() : null;
        hp?.Open();
        hp?.SetTarget(_chrUI);
        m_HpDic.Add(_chrUI, hp);
    }
    /// <summary>
    /// 캐릭터 UI를 제거합니다.
    /// </summary>
    /// <param name="_chrUI"></param>
    internal void RemoveCharacterUI(CharacterUI _chrUI)
    {
        if (!m_HpDic.ContainsKey(_chrUI))
            return;
        
        var hp = m_HpDic[_chrUI];
        if(hp != null)
            hp.Close();
        m_HpDic.Remove(_chrUI);
    }
    #endregion
}