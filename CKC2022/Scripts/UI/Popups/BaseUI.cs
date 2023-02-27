using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.UI.Popups
{
    public class BaseUI : PopupWindow
    {
        public static BaseUI Instance { get; private set; }

        #region Event
        protected override void OnInitSingleton()
        {
            base.OnInitSingleton();

            Instance = this;
        }
        #endregion
    }
}