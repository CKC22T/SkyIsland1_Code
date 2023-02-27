using CKC2022;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Sirenix.OdinInspector;

namespace CKC2022
{
    public class AnimationEvent : MonoBehaviour
    {
        public event Action OnDissolveComplete;

        [SerializeField]
        private bool UseDissolver;

        [ShowIf("UseDissolver")]
        [SerializeField]
        private CharacterDissolver dissolver;


        public void Dissolve()
        {
            if (!UseDissolver)
                return;

            dissolver.StartDissovle(DissolveComplete);
        }

        public void ResetDissolve()
        {
            if (!UseDissolver)
                return;

            dissolver.InitializeDissolve();
        }

        public void DissolveComplete()
        {
            OnDissolveComplete?.Invoke();
        }

        public void RunEffect(GameObject origin)
        {
            PoolManager.SpawnObject(origin, transform.position, transform.rotation);
        }
        
        public void RunEffectLocal(GameObject origin)
        {
            var instance = PoolManager.SpawnObject(origin, transform.position, transform.rotation);
            instance.transform.SetParent(transform, true);
        }

        public void RunEffectIdentity(GameObject origin)
        {
            PoolManager.SpawnObject(origin, transform.position, Quaternion.identity);
        }

        public void RunEffectZUP(GameObject origin)
        {
            PoolManager.SpawnObject(origin, transform.position, Quaternion.AngleAxis(-90, Vector3.right));
        }

        public void RunSound(int soundCode)
        {
            try
            {
                GameSoundManager.Play((SoundType)soundCode, new SoundPlayData(transform.position));
            }
            catch (Exception ex)
            {

            }
        }
    }
}