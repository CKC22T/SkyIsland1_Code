using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace CKC2022
{
    public class CharacterDissolver : MonoBehaviour
    {
        [SerializeField]
        private float delay;

        [SerializeField]
        private float runtime;

        [Header("Color")]
        [SerializeField]
        private string ColorPropertyName;

        [GradientUsage(true)]
        [SerializeField]
        private Gradient ColorGradient;

        [Header("Dissolve")]
        [SerializeField]
        private string dissolvePropertyName;

        [SerializeField]
        private AnimationCurve dissolveCurve;


        [SerializeField]
        private List<SkinnedMeshRenderer> targets;

        private CoroutineWrapper wrapper;


        private void Awake()
        {
            wrapper = new CoroutineWrapper(this);
        }

        public void InitializeDissolve()
        {
            ApplyPropery(0);
        }


        public void StartDissovle(Action onComplete)
        {
            wrapper.StartSingleton(RunDissolve())
                .SetOnComplete((complete) => onComplete?.Invoke());
        }


        IEnumerator RunDissolve()
        {
            yield return YieldInstructionCache.WaitForSeconds(delay);

            float t = 0;
            while (t < runtime)
            {
                ApplyPropery(t / runtime);
                t += Time.deltaTime;
                yield return null;
            }

            ApplyPropery(1);
        }

        private void ApplyPropery(in float runRate)
        {
            foreach (var renderer in targets)
            {
                foreach (var targetMaterial in renderer.materials)
                {
                    targetMaterial.SetColor(ColorPropertyName, ColorGradient.Evaluate(runRate));
                    targetMaterial.SetFloat(dissolvePropertyName, dissolveCurve.Evaluate(runRate));
                }
            }
        }

    }
}