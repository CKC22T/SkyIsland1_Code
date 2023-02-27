using System.Collections;
using UnityEngine;

namespace CKC2022
{
    public class AnimationSoundEvent : MonoBehaviour
    {
        [SerializeField]
        private HumanoidAnimationController controller;


        public void RunWalkSound()
        {
            //if ground is Dirt, code is 101
            //else if ground is Stone, code is 102
            var code = 101;

            var data = new SoundPlayData();
            data.volume = controller.MoveMagnitude;
            data.Position = transform.position;

            GameSoundManager.Play((SoundType)code, data);
        }

    }
}