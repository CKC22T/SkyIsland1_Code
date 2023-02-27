using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CulterLib.UI.Controls
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class Control_TextMeshPro : Control_Text
    {
        #region Inspector
        [Title("Control_TextLegacy")]
        [SerializeField, TabGroup("Component")] private TextMeshProUGUI m_Text;
        #endregion
        #region Get,Set
        /// <summary>
        /// Å¸°Ù ÅØ½ºÆ® ÄÄÆ÷³ÍÆ®
        /// </summary>
        public TextMeshProUGUI Tar { get => m_Text; }

        //Override
        public override string text { get => m_Text.text; set => m_Text.text = value; }
        #endregion

        #region Function - Editor
#if UNITY_EDITOR
        public override void Setup()
        {
            base.Setup();

            m_Text = GetComponent<TextMeshProUGUI>();
        }
#endif
        #endregion
    }
}