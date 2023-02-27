using System.Collections;
using UnityEngine;

namespace CKC2022
{
    [RequireComponent(typeof(BridgeEvent))]
    public class BridgeFloatingActor : MonoBehaviour
    {
        [SerializeField]
        private BridgeEvent targetEvent;

        private FloatingObject floating = new FloatingObject();

        [SerializeField] private float runRate = 0.05f;

        private void Awake()
        {
            if(targetEvent == null)
                targetEvent = GetComponent<BridgeEvent>();
            
            targetEvent.OnFloating += TargetEvent_OnFloating;
        }

        private bool isDisposed = false;

        private void OnDestroy()
        {
            if (!isDisposed)
            {
                targetEvent.OnFloating -= TargetEvent_OnFloating;
                isDisposed = true;
            }
        }

        private void OnDisable()
        {
            if (!isDisposed)
            {
                targetEvent.OnFloating -= TargetEvent_OnFloating;
                isDisposed = true;
            }
        }

        private void TargetEvent_OnFloating(bool isOn)
        {
            if (isOn)
                floating.Start(targetEvent.gameObject, runRate);
            else
                floating.Stop();
        }

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button]
        public void initialize()
        {
            targetEvent = GetComponent<BridgeEvent>();
        }

        [Sirenix.OdinInspector.Button]
        public void TestReStart()
        {
            floating.Start(targetEvent.gameObject, runRate);
        }
#endif
    }
}