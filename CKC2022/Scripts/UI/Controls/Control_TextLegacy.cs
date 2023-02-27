using System.Collections;
using System.Collections.Generic;
using CulterLib.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CulterLib.UI.Controls
{
    [RequireComponent(typeof(Text))]
    public class Control_TextLegacy : Control_Text
    { 
        #region Inspector
        [Title("Control_TextLegacy")]
        [SerializeField, TabGroup("Component")] private Text m_Text;
        #endregion
        #region Get,Set
        /// <summary>
        /// Å¸°Ù ÅØ½ºÆ® ÄÄÆ÷³ÍÆ®
        /// </summary>
        public Text Tar { get => m_Text; }

        //Override
        public override string text { get => m_Text.text; set => m_Text.text = value; }
        #endregion

        #region Function - Editor
#if UNITY_EDITOR
        public override void Setup()
        {
            base.Setup();

            m_Text = GetComponent<Text>();
        }
#endif
        #endregion
    }
}