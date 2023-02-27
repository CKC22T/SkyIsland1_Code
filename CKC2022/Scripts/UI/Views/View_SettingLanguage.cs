using CulterLib.Presets;
using CulterLib.Types;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CulterLib.UI.Views
{
    public class View_SettingLanguage : Mono_UI
    {
        #region Inspector
        [TabGroup("Component"), SerializeField] private Dropdown m_LangDropdown;
        #endregion

        #region Event
        protected override void OnInitData()
        {
            base.OnInitData();

            //언어 추가
            var langtext = GlobalManager.Instance.DataMgr.GetTextTableData("Text_LangView_Lang");
            m_LangDropdown.options.Clear();
            foreach (var v in GlobalManager.Instance.LanguageMgr.Support)
                m_LangDropdown.options.Add(new Dropdown.OptionData(langtext.GetText(v)));

            //이벤트 초기화
            m_LangDropdown.onValueChanged.AddListener((UnityEngine.Events.UnityAction<int>)((_dummy) =>
            {
                var supportlang = GlobalManager.Instance.LanguageMgr.Support;
                GlobalManager.Instance.LanguageMgr.Now.Value = supportlang[m_LangDropdown.value];
            }));
        }
        protected override void OnParentOpen()
        {
            base.OnParentOpen();

            var supportlang = GlobalManager.Instance.LanguageMgr.Support;
            for (int i = 0; i < supportlang.Count; ++i)
                if (GlobalManager.Instance.LanguageMgr.Now.Value == supportlang[i])
                {
                    m_LangDropdown.value = i;
                    break;
                }
        }
        #endregion
    }
}