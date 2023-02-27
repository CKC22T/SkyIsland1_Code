#if UNITY_EDITOR
using CulterLib.Global.Data;
using CulterLib.Presets;
using CulterLib.Types;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CulterLib.Global.Setup
{
    public class SetupTool : MonoBehaviour
    {
        #region Inspector
        [TabGroup("Option"), SerializeField] private bool mSetupAtPlay;
        #endregion

        #region Event
        private void Awake()
        {
            if (mSetupAtPlay)
                SetupAll();
        }
        #endregion
        #region Function
        [Sirenix.OdinInspector.Button("Setup All", 15)]
        public void SetupAll()
        {
            //Global
            foreach (var v in FindObjectsOfType<GlobalManager>())
                v.Setup();
            foreach (var v in FindObjectsOfType<DataManager>())
                v.Setup();

            //UI
            foreach (var v in FindObjectsOfType<UIManager>())
                v.Setup();
            foreach (var v in FindObjectsOfType<PopupManager>())
                v.Setup();
            foreach(var v in FindObjectsOfType<Mono_UI>())
                v.Setup();
            foreach (var v in FindObjectsOfType<BlockingPopup>())
                v.Setup();
        }
        #endregion
    }
}
#endif