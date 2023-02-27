using CulterLib.Global.Data;
using CulterLib.Types;
using CulterLib.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Utils;

namespace CulterLib.Global.Sound
{
    public class SoundManager : MonoBehaviour
    {
        #region Inspector
        [TabGroup("Component"), SerializeField] private AudioMixer m_Mixer;
        [TabGroup("Component"), SerializeField] private AudioMixerGroup m_SEGroup;
        [TabGroup("Component"), SerializeField] private AudioMixerGroup m_UIGroup;
        [TabGroup("Component"), SerializeField] private AudioSource m_BGM1;
        [TabGroup("Component"), SerializeField] private AudioSource m_BGM2;
        [TabGroup("Component"), SerializeField] private AudioSource[] m_SEs;
        [TabGroup("Option"), SerializeField] private AnimationCurve m_PlayCurve;
        [TabGroup("Option"), SerializeField] private AnimationCurve m_StopCurve;
        #endregion
        #region Get,Set
        /// <summary>
        /// 현재 마스터볼륨
        /// </summary>
        public Notifier<float> CurMasterVolume { get; private set; } = new Notifier<float>(1.0f);
        /// <summary>
        /// 현재 배경음볼륨
        /// </summary>
        public Notifier<float> CurBGMVolume { get; private set; } = new Notifier<float>(1.0f);
        /// <summary>
        /// 현재 환경음 볼륨
        /// </summary>
        public Notifier<float> CurEnvVolume { get; private set; } = new Notifier<float>(1.0f);
        /// <summary>
        /// 현재 효과음 볼륨
        /// </summary>
        public Notifier<float> CurSEVolume { get; private set; } = new Notifier<float>(1.0f);
        /// <summary>
        /// 현재 UI효과음 볼륨
        /// </summary>
        public Notifier<float> CurUIVolume { get; private set; } = new Notifier<float>(1.0f);
        #endregion
        #region Value
        //BGM 관련
        private AudioSource m_CurBGM;
        private AudioSource m_NextBGM;
        private Coroutine m_StartCor;
        private Coroutine m_StopCor;

        //SE관련
        private int m_SEIndex;
        #endregion

        #region Event
        public void Init()
        {
            //구성요소 초기화
            m_CurBGM = m_BGM1;
            m_NextBGM = m_BGM2;

            //이벤트 초기화
            CurMasterVolume.OnDataChanged += (dummy) =>
            {   //마스터 볼륨 설정이 바뀌면 볼륨을 업데이트한다.
                CurMasterVolume.Set(Mathf.Clamp(CurMasterVolume.Value, 0.0001f, 1), false);
                m_Mixer.SetFloat("MasterVolume", Mathf.Log10(CurMasterVolume.Value) * 20);
            };
            CurBGMVolume.OnDataChanged += (dummy) =>
            {   //배경음 볼륨 설정이 바뀌면 볼륨을 업데이트한다.
                CurBGMVolume.Set(Mathf.Clamp(CurBGMVolume.Value, 0.0001f, 1), false);
                m_Mixer.SetFloat("BGMVolume", Mathf.Log10(CurBGMVolume.Value) * 20);
            };
            CurEnvVolume.OnDataChanged += (dummy) =>
            {   //환경음 볼륨 설정이 바뀌면 볼륨을 업데이트한다.
                CurEnvVolume.Set(Mathf.Clamp(CurEnvVolume.Value, 0.0001f, 1), false);
                m_Mixer.SetFloat("EnvVolume", Mathf.Log10(CurEnvVolume.Value) * 20);
            };
            CurSEVolume.OnDataChanged += (dummy) =>
            {   //효과음 볼륨 설정이 바뀌면 볼륨을 업데이트한다.
                CurSEVolume.Set(Mathf.Clamp(CurSEVolume.Value, 0.0001f, 1), false);
                m_Mixer.SetFloat("SEVolume", Mathf.Log10(CurSEVolume.Value) * 20);
            };
            CurUIVolume.OnDataChanged += (dummy) =>
            {   //UI효과음 볼륨 설정이 바뀌면 볼륨을 업데이트한다.
                CurUIVolume.Set(Mathf.Clamp(CurUIVolume.Value, 0.0001f, 1), false);
                m_Mixer.SetFloat("UIVolume", Mathf.Log10(CurUIVolume.Value) * 20);
            };
        }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// BGM을 재생한다.
        /// </summary>
        /// <param name="id"></param>
        public void PlayBGM(AudioClip _clip, float _changeTime = 0)
        {
            if (m_CurBGM.clip == _clip)
                return;

            if (_clip != null)
            {
                var swap = m_CurBGM;
                m_CurBGM = m_NextBGM;
                m_NextBGM = swap;
                m_CurBGM.clip = _clip;
                m_CurBGM.volume = 0f;
                m_CurBGM.Play();
                if (0 < _changeTime)
                {
                    m_StopCor = CoroutineUtil.Stop(this, m_StopCor);
                    m_StartCor = CoroutineUtil.Stop(this, m_StartCor);
                    m_StopCor = StartCoroutine(ChangeBGMCor(m_NextBGM, m_StopCurve, 1 / _changeTime, 0, () => { m_NextBGM.Stop(); m_StopCor = null; }));
                    m_StartCor = StartCoroutine(ChangeBGMCor(m_CurBGM, m_PlayCurve, 1 / _changeTime, 1, () => m_StartCor = null));
                }
                else
                {
                    m_CurBGM.volume = 1.0f;
                    m_NextBGM.volume = 0f;
                    m_NextBGM.Stop();
                }
            }
        }
        /// <summary>
        /// BGM을 멈춘다.
        /// </summary>
        public void StopBGM(AnimationCurve _curve = null, float _time = 0)
        {
            if (_curve != null && 0 < _time)
            {
                m_StopCor = CoroutineUtil.Stop(this, m_StopCor);
                m_StopCor = StartCoroutine(ChangeBGMCor(m_CurBGM, _curve, 1 / _time, 0, () => { m_CurBGM.Stop(); m_StopCor = null; }));
            }
            else
                m_CurBGM.volume = 0;
        }
        /// <summary>
        /// BGM을 다시 재생한다.
        /// </summary>
        public void ResumeBGM(AnimationCurve _curve = null, float _time = 0)
        {
            m_CurBGM.Play();
            if (_curve != null && 0 < _time)
            {
                m_StartCor = CoroutineUtil.Stop(this, m_StartCor);
                m_StartCor = StartCoroutine(ChangeBGMCor(m_CurBGM, _curve, 1 / _time, 1, () => m_StartCor = null));
            }
            else
                m_CurBGM.volume = 1.0f;
        }

        /// <summary>
        /// 사운드이펙트를 플레이합니다.
        /// </summary>
        /// <param name="_pos"></param>
        /// <param name="_clip"></param>
        /// <param name="_blend"></param>
        public void PlaySE(Vector3 _pos, AudioClip _clip, float _blend3D = 0.8f)
        {
            if (_clip == null)
                return;

            var se = m_SEs[m_SEIndex];
            se.transform.position = _pos;
            se.outputAudioMixerGroup = m_SEGroup;
            se.PlayOneShot(_clip);
            se.spatialBlend = _blend3D;
            m_SEIndex = (m_SEIndex + 1) % m_SEs.Length;
        }

        /// <summary>
        /// UI 사운드이펙트를 플레이합니다.
        /// </summary>
        /// <param name="_pos"></param>
        /// <param name="_clip"></param>
        /// <param name="_blend"></param>
        public void PlayUI(Vector3 _pos, AudioClip _clip, float _blend3D = 0.8f)
        {
            if (_clip == null)
                return;

            var se = m_SEs[m_SEIndex];
            se.transform.position = _pos;
            se.outputAudioMixerGroup = m_UIGroup;
            se.PlayOneShot(_clip);
            se.spatialBlend = _blend3D;
            m_SEIndex = (m_SEIndex + 1) % m_SEs.Length;
        }
       

        //Private
        /// <summary>
        /// BGM 변경 코루틴
        /// </summary>
        /// <param name="_audio"></param>
        /// <param name="_curve"></param>
        /// <param name="_spd"></param>
        /// <param name="_fac"></param>
        /// <param name="_onEnd"></param>
        /// <returns></returns>
        private IEnumerator ChangeBGMCor(AudioSource _audio, AnimationCurve _curve, float _spd, float _fac, Action _onEnd = null)
        {
            float timer = 0;
            while (timer <= 1.0f)
            {
                timer += Time.unscaledDeltaTime * _spd;
                _audio.volume = _curve.Evaluate(timer);
                yield return null;
            }
            _audio.volume = _fac;
            _onEnd?.Invoke();
        }
        #endregion
    }
}