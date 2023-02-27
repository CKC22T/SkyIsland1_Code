using System.Collections;
using System.Collections.Generic;
using CulterLib.UI.Controls;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CulterLib.UI.Popups
{
    public class LoadingPopup : PopupWindow
    {
        public static LoadingPopup Instance { get; private set; }
        #region Inspector
        [TabGroup("Component"), SerializeField] private Image m_LoadProgress;
        #endregion

        #region Event
        protected override void OnInitSingleton()
        {
            base.OnInitSingleton();

            Instance = this;
        }
        protected override void OnStartOpen(string _opt)
        {
            base.OnStartOpen(_opt);

            m_LoadProgress.fillAmount = 0;
        }
        #endregion
        #region Function
        /// <summary>
        /// 로딩 진행도를 설정합니다.
        /// </summary>
        /// <param name="_progress"></param>
        public void SetProgress(float _progress)
        {
            m_LoadProgress.fillAmount = _progress;
        }
        #endregion
    }
}