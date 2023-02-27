using CulterLib.Presets;
using CulterLib.Types;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.UI.Controls
{
    public abstract class Control_Text : Mono_UI
    {
        #region Type
        public enum ETextType
        {
            /// <summary>
            /// 일반 텍스트 (왠만하면 Control까지 넣지말고 유니티 기본으로만 쓸것)
            /// </summary>
            None,
            /// <summary>
            /// 해당 ID의 텍스트
            /// </summary>
            ID,
        }
        #endregion

        #region Inspector
        [Title("Control_Text")]
        [SerializeField, TabGroup("Option")] private ETextType m_Type = ETextType.ID;
        #endregion
        #region Get,Set
        /// <summary>
        /// 현재 TextType
        /// </summary>
        public ETextType Type { get => m_Type; }

        //Abstract
        /// <summary>
        /// 현재 텍스트
        /// </summary>
        public abstract string text { get; set; }
        #endregion

        #region Event
        protected override void OnUpdateLanguage()
        {
            base.OnUpdateLanguage();

            if (Type == ETextType.ID)
            {
                var tt = GlobalManager.Instance.DataMgr.GetTextTableData("Text_" + ID.Value);
                text = (tt != null) ? tt.GetText() : "";
            }
        }
        #endregion
    }
}