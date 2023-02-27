using System.Collections;
using UnityEngine;
using Utils;

namespace CKC2022
{
    public class LightningDetectorActorChild : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer line;

        [SerializeField]
        private float Runtime;

        [SerializeField]
        private float runtimeRandomizeOffset;

        [SerializeField]
        private AnimationCurve curve;

        private CoroutineWrapper wrapper;
        private void Awake()
        {
            wrapper = new CoroutineWrapper(CoroutineWrapper.CoroutineRunner.Instance);
        }

        public void SetLine(Vector3 start, Vector3 end)
        {
            line.SetPosition(0, start);
            line.SetPosition(1, end);

            wrapper.StartSingleton(Release());
        }

        IEnumerator Release()
        {
            float t = 0;
            var runtime = Runtime + Random.Range(-runtimeRandomizeOffset, runtimeRandomizeOffset);
            while (t < runtime)
            {
                line.colorGradient.ApplyAlphaToGradient(curve.Evaluate(t / runtime));
                t += Time.deltaTime;
                yield return null;
            }
            line.colorGradient.ApplyAlphaToGradient(curve.Evaluate(t / runtime));

            PoolManager.ReleaseObject(gameObject);
        }
    }
}