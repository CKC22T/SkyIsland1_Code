using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CKC2022
{
    public class FloatingObject
    {
        private Utils.CoroutineWrapper wrapper;

        private bool isInitialized;

        private Vector3 RotateAxis;

        //private float runRate = 0.05f;

        public void Start(in GameObject target, float runRate = 0.05f)
        {
            if (isInitialized == false)
                Initialize(target);

            wrapper.StartSingleton(UpdateFloating(target, runRate));
        }

        public void Initialize(in GameObject target)
        {
            wrapper = new Utils.CoroutineWrapper(Utils.CoroutineWrapper.CoroutineRunner.Instance);

            var randomVector = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            RotateAxis = Quaternion.Euler(randomVector) * target.transform.up;
            isInitialized = true;
        }

        IEnumerator UpdateFloating(GameObject target, float runRate)
        {
            while (target != null && target.activeInHierarchy)
            {
                var randomVector = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                RotateAxis = Quaternion.Euler(randomVector) * target.transform.up;
                target.transform.localRotation *= Quaternion.AngleAxis(runRate, RotateAxis);
                yield return null;
            }
        }

        public void Stop()
        {
            wrapper.Stop();
        }
    }

    //FlatingStone_Deco
    public class FloatingObjectContainer : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> objects = new List<GameObject>();
        private readonly List<FloatingObject> floatingObjects = new List<FloatingObject>();

        [SerializeField] private float runRate = 0.05f;

        private void Awake()
        {
            foreach (var obj in objects)
            {
                var floatingObject = new FloatingObject();
                floatingObjects.Add(floatingObject);
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < objects.Count; ++i)
            {
                floatingObjects[i].Start(objects[i], runRate);
            }
        }

        private void OnDisable()
        {
            foreach (var floating in floatingObjects)
            {
                floating.Stop();
            }
        }

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button]
        public void TestReStart()
        {
            for (int i = 0; i < objects.Count; ++i)
            {
                floatingObjects[i].Start(objects[i], runRate);
            }
        }
#endif
    }
}