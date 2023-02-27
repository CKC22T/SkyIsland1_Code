using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CulterLib.Utils;
using Network;

public class PuriPopup : PopupWindow
{
    public static PuriPopup Instance { get; private set; }
    #region Inspector
    [TabGroup("Component"), SerializeField] private TextMeshProUGUI m_TalkText;
    #endregion
    #region Value
    private Coroutine mTalkCor;
    #endregion

    #region Event
    protected override void OnInitSingleton()
    {
        base.OnInitSingleton();
        Instance = this;
        ClientSessionManager.Instance.OnPooriScriptCallback += Instance_OnPooriScriptCallback;
    }

    private void Instance_OnPooriScriptCallback(PooriScriptType pooriScriptType)
    {
        var script = pooriScriptType.GetPooriScript();
        Open(script);
    }

    private void OnDestroy()
    {
        if (ClientSessionManager.IsQuitting == false)
            ClientSessionManager.Instance.OnPooriScriptCallback -= Instance_OnPooriScriptCallback;
    }

    protected override void OnStartOpen(string _opt)
    {
        base.OnStartOpen(_opt);

        mTalkCor = CoroutineUtil.Change(this, mTalkCor, TalkCor(_opt));
    }
    #endregion
    #region Function
    //Private
    private IEnumerator TalkCor(string _text)
    {
        float timer = 0;
        while(timer < 1.0f)
        {
            m_TalkText.text = _text.Substring(0, (int)((_text.Length - 1) * timer));
            timer += Time.deltaTime * ServerConfiguration.PooriPopupTextingSpeed;
            yield return null;
        }
        m_TalkText.text = _text;

        float delay = ServerConfiguration.DefaultPooriPopupStayDelay +
                      _text.Length * ServerConfiguration.DefaultPooriTextEachCharacterDelay;

        yield return new WaitForSeconds(delay);
        Close();
    }
    #endregion
}
