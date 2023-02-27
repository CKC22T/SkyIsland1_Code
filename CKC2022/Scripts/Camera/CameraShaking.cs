using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace CKC2022
{
    public class CameraShaking : LocalSingleton<CameraShaking>
    {
        public class ShakeInfo
        {
            public float loopTime;
            public int loopCount;
            public float releaseTime;
            
            public float runtime => loopTime * loopCount + releaseTime;
            
            public float magnitude;
            public Vector2 shakeDirection;

            public static readonly ShakeInfo defaultInfo
                = new()
                {
                    loopTime = 0.2f,
                    loopCount = 4,
                    releaseTime = 0.2f,
                    
                    magnitude = 2f,
                    shakeDirection = new Vector2(1, 1)
                };
        }

        [SerializeField]
        private ParticleSystem.MinMaxCurve reactionLoopCurve;
        
        //[SerializeField]
        //private AnimationCurve reactionCurve;

        private CoroutineWrapper wrapper;
        
        protected override void Initialize()
        {
            base.Initialize();

            wrapper = new CoroutineWrapper(this);
        }

        public void Shake(in ShakeInfo info)
        {
            wrapper.StartSingleton(ShakeInternal(info));
            
            IEnumerator ShakeInternal(ShakeInfo info)
            {
                float t = 0;
                while (t < info.runtime)
                {
                    transform.localPosition = GetLocalOffset(info, t / info.runtime, (t - (info.loopTime * info.loopCount)) / info.releaseTime);
                    t += Time.deltaTime;
                    yield return null;
                }
            }
        }


        private Vector3 GetLocalOffset(in ShakeInfo info, float t01, float l)
        {
            l = 1 - Mathf.Clamp01(l);
            
            var t = Mathf.Repeat(t01, info.loopTime) / info.loopTime;
            
            return info.magnitude * reactionLoopCurve.Evaluate(t, l * 0.5f + 0.5f) * info.shakeDirection;
        }

#if ODIN_INSPECTOR
        
        [Space(20)]
        [Header("Debugging setting")]
        [SerializeField]
        private float magnitude = 2;
        [SerializeField]
        private float loopTime = 0.2f;
        [SerializeField]
        private int loopCount = 4;
        [SerializeField]
        private float releaseTime = 0.2f;
        [SerializeField]
        private Vector2 shakeDirection = new Vector2(1, 1);

        [Sirenix.OdinInspector.Button]
        public void TestShake()
        {
            var info = ShakeInfo.defaultInfo;
            info.magnitude = magnitude;
            info.loopCount = loopCount;
            info.loopTime = loopTime;
            info.releaseTime = releaseTime;
            info.shakeDirection = shakeDirection;

            Shake(info);
        }
#endif
    }
}