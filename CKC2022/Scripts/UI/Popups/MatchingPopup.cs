using CulterLib.Presets;
using CulterLib.Types;
using CulterLib.UI.Controls;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchingPopup : PopupWindow
{
    public static MatchingPopup Instance { get; private set; }
    #region Inspector
    [Title("MatchingPopup")]
    [TabGroup("Component"), SerializeField] private Control_MatchingServer[] m_MatchingServer;
    [TabGroup("Component"), SerializeField] private Control_Button m_RefleshBtn;
    [Title("MatchingPopup")]
    [TabGroup("Option"), SerializeField] private int m_MaxServer = 100;
    #endregion
    #region Value
    private UIObjectPool<Control_MatchingServer> m_MatchingServerPool;
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
        m_MatchingServerPool = new UIObjectPool<Control_MatchingServer>(m_MatchingServer, m_MaxServer, false, (obj) => AddChildUI(obj));

        //이벤트 초기화
        m_RefleshBtn.OnBtnClickFunc += (_btn) =>
        {
            Reflesh();
        };
    }
    protected override void OnStartOpen(string _opt)
    {
        base.OnStartOpen(_opt);

        Reflesh();
    }
    #endregion
    #region Function
    //Public
    public void Reflesh()
    {
        m_RefleshBtn.interactable = false;
        m_MatchingServerPool.Clear();
        BlockingPopup.Instance.Open();
        GlobalManager.Instance.MatchingMgr.UpdateRoomList(new GetRoomReq(), (code) =>
        {
            m_RefleshBtn.interactable = true;
            BlockingPopup.Instance.Close();
            if (code == WebErrorCodeTemp.Success)
            {
                foreach(RoomInfo v in GlobalManager.Instance.MatchingMgr.Rooms)
                    m_MatchingServerPool.GetObject()?.SetServer(v);
            }
            else
            {
                var mt = GlobalManager.Instance.DataMgr.GetTextTableData("Text_MatchingPop_RefleshFail").GetText();
                var st = GlobalManager.Instance.DataMgr.GetTextTableData($"Text_Error_{code}").GetText();
                NotifyPopup.Instance.Open(mt, st, new NotifyPopup.SBtnData("Common_Ok", null));
            }
        });
    }
    #endregion
}
