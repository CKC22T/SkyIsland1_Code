using CulterLib.Presets;
using CulterLib.Types;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CulterLib.UI.Views
{
    public class View_SettingVolume : Mono_UI
    {
        #region Inspector
        [TabGroup("Component"), SerializeField] private Slider m_MasterSlider;
        [TabGroup("Component"), SerializeField] private Slider m_BGMSlider;
        [TabGroup("Component"), SerializeField] private Slider m_EnvSlider;
        [TabGroup("Component"), SerializeField] private Slider m_SESlider;
        [TabGroup("Component"), SerializeField] private Slider m_UISlider;
        #endregion

        #region Event
        protected override void OnInitData()
        {
            base.OnInitData();

            //이벤트 초기화
            if (m_MasterSlider)
                m_MasterSlider.onValueChanged.AddListener((dummy) =>
                {   //Master 크기 변경
                    GlobalManager.Instance.SoundMgr.CurMasterVolume.Value = m_MasterSlider.value;
                });
            if (m_BGMSlider)
                m_BGMSlider.onValueChanged.AddListener((dummy) =>
                {   //BGM 크기 변경
                    GlobalManager.Instance.SoundMgr.CurBGMVolume.Value = m_BGMSlider.value;
                });
            if (m_EnvSlider)
                m_EnvSlider.onValueChanged.AddListener((dummy) =>
                {   //Env 크기 변경
                    GlobalManager.Instance.SoundMgr.CurEnvVolume.Value = m_EnvSlider.value;
                });
            if (m_SESlider)
                m_SESlider.onValueChanged.AddListener((dummy) =>
                {   //SE 크기 변경
                    GlobalManager.Instance.SoundMgr.CurSEVolume.Value = m_SESlider.value;
                });
            if (m_UISlider)
                m_UISlider.onValueChanged.AddListener((dummy) =>
                {   //UI SE 크기 변경
                    GlobalManager.Instance.SoundMgr.CurUIVolume.Value = m_UISlider.value;
                });
        }
        protected override void OnParentOpen()
        {
            base.OnParentOpen();

            if (m_MasterSlider)
                m_MasterSlider.value = GlobalManager.Instance.SoundMgr.CurMasterVolume.Value;
            if (m_BGMSlider)
                m_BGMSlider.value = GlobalManager.Instance.SoundMgr.CurBGMVolume.Value;
            if (m_EnvSlider)
                m_EnvSlider.value = GlobalManager.Instance.SoundMgr.CurEnvVolume.Value;
            if (m_SESlider)
                m_SESlider.value = GlobalManager.Instance.SoundMgr.CurSEVolume.Value;
            if (m_UISlider)
                m_UISlider.value = GlobalManager.Instance.SoundMgr.CurUIVolume.Value;
        }
        #endregion
    }
}