using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CKC2022
{
    public class AutoSoundPlayer : MonoBehaviour
    {
        [SerializeField]
        private SoundType soundType;

        [SerializeField]
        private SoundPlayData data;

        [SerializeField]
        private bool PlayOnEnable = false;

        private AudioSource instance;

        private void OnEnable()
        {
            if (PlayOnEnable)
                PlaySound();
        }

        public void PlaySound()
        {
            StartCoroutine(waitAndPlay());
        }

        IEnumerator waitAndPlay()
        {
            yield return new WaitUntil(() => GameSoundManager.Instance != null);

            data.Position = transform.position;

            if (soundType.IsBGM())
            {
                GameSoundManager.PlayBGM(soundType);
            }
            else
            {
                instance = GameSoundManager.Play(soundType, data);
            }
        }

        public void StopSound()
        {
            if (isPlaying())
                instance.Stop();

            instance = null;
        }
        
        private bool isPlaying()
        {
            if (instance == null)
                return false;

            if (!GameSoundManager.Instance.TryGetConfig(soundType, out var config))
                return false;

            //not match clip => play is complete and source is recycled.
            if (config.clips.Count == 1 && instance.clip != config.clip)
                return false;

            if (instance.isPlaying == false)
                return false;

            return true;
        }

        private void OnDisable()
        {
            StopSound();
        }

    }

}