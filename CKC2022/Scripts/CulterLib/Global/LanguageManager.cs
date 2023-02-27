using System.Collections;
using System.Collections.Generic;
using CulterLib.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace CulterLib.Global.Language
{
    public class LanguageManager : MonoBehaviour
    {
        #region Inspector
        [TabGroup("Option"), SerializeField] private SystemLanguage m_Default = SystemLanguage.English;
        [TabGroup("Option"), SerializeField] private bool m_IsUseKor;
        [TabGroup("Option"), SerializeField] private bool m_IsUseEng;
        [TabGroup("Option"), SerializeField] private bool m_IsUseJap;
        [TabGroup("Option"), SerializeField] private bool m_IsUseRus;
        #endregion
        #region Get,Set
        /// <summary>
        /// 현재 표시중인 언어
        /// </summary>
        public Notifier<SystemLanguage> Now { get; } = new Notifier<SystemLanguage>();
        /// <summary>
        /// 현재 사용 가능한 언어
        /// </summary>
        public IReadOnlyList<SystemLanguage> Support { get; private set; }
        #endregion

        #region Event
        public void Init()
        {
            //구성요소 초기화
            Now.Value = Application.systemLanguage;
            var support = new List<SystemLanguage>();
            if (m_IsUseKor)
                support.Add(SystemLanguage.Korean);
            if (m_IsUseEng)
                support.Add(SystemLanguage.English);
            if (m_IsUseJap)
                support.Add(SystemLanguage.Japanese);
            if (m_IsUseRus)
                support.Add(SystemLanguage.Russian);
            Support = support;

            //이벤트 초기화
            Now.OnDataChanged += (data) =>
            {   //없는 언어를 넣으면 사용 가능한 언어로 변경
                Now.Set(GetEnable(data), false);
            };
        }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// 해당 언어 사용자에게 보여줄 언어를 가져옵니다.
        /// </summary>
        /// <param name="_lang"></param>
        /// <returns></returns>
        public SystemLanguage GetEnable(SystemLanguage _lang)
        {
            switch (_lang)
            {
                case SystemLanguage.Korean:
                    return m_IsUseKor ? _lang : m_Default;
                case SystemLanguage.English:
                    return m_IsUseEng ? _lang : m_Default;
                case SystemLanguage.Japanese:
                    return m_IsUseJap ? _lang : m_Default;
                case SystemLanguage.Russian:
                    return m_IsUseRus ? _lang : m_Default;
                default:
                    return m_Default;
            }
        }
        /// <summary>
        /// 해당 언어의 다음 언어를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public SystemLanguage GetNext(SystemLanguage _lang)
        {
            SystemLanguage next = GetEnable(_lang);

            do
            {
                switch (next)
                {
                    case SystemLanguage.Korean:
                        next = SystemLanguage.English;
                        break;
                    case SystemLanguage.English:
                        next = SystemLanguage.Japanese;
                        break;
                    case SystemLanguage.Japanese:
                        next = SystemLanguage.Russian;
                        break;
                    case SystemLanguage.Russian:
                        next = SystemLanguage.Korean;
                        break;
                }
            } while (GetEnable(next) != next);

            return next;
        }
        #endregion
    }
}