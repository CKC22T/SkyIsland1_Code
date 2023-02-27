using CulterLib.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace CulterLib.UI.Popups
{
    public class BlockingPopup : PopupWindow
    {
        public static BlockingPopup Instance { get; private set; }
        #region Inspector
        [TabGroup("Component"), SerializeField] private Transform m_Spin;
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
            StartCoroutine(SpinCor());
        }
        #endregion
        #region Function
        private IEnumerator SpinCor()
        {
            while(true)
            {
                yield return new WaitForSeconds(0.2f);
                m_Spin.localEulerAngles += new Vector3(0, 0, -45);
            }
        }
        #endregion
    }
}