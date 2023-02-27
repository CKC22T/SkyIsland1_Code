using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CulterLib.UI.Popups
{
    public class SettingPopup : PopupWindow
    {
        public static SettingPopup Instance { get; private set; }

        #region Event
        //Popup Event
        protected override void OnInitSingleton()
        {
            base.OnInitSingleton();

            Instance = this;
        }
        #endregion
    }
}