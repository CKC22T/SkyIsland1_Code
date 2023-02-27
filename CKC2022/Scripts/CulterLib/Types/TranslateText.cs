using CulterLib.Presets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Types
{
    [Serializable] public partial class TranslateText : IName
    {
        #region Get,Set
        /// <summary>
        /// 한국어 텍스트입니다.
        /// </summary>
        public string Kor { get => kor; set => kor = value; }
        /// <summary>
        /// 영어 텍스트입니다.
        /// </summary>
        public string Eng { get => eng; set => eng = value; }
        /// <summary>
        /// 일본어
        /// </summary>
        public string Jap { get => jap; set => jap = value; }
        /// <summary>
        /// 러시아어
        /// </summary>
        public string Rus { get => rus; set => rus = value; }

        //IName
        TranslateText IName.Name
        {
            get => this;
            set
            {
                kor = value.kor;
                eng = value.eng;
                jap = value.jap;
            }
        }
        #endregion
        #region Value
        [SerializeField] private string kor;    //한국어
        [SerializeField] private string eng;    //영어
        [SerializeField] private string jap;    //일본어
        [SerializeField] private string rus;    //일본어
        #endregion

        #region Event
        /// <summary>
        /// 번역 텍스트를 생성합니다.
        /// </summary>
        /// <param name="_text"></param>
        public TranslateText(string _text)
        {
            kor = _text;
            eng = _text;
            jap = _text;
            rus = _text;
        }
        /// <summary>
        /// 번역 텍스트를 생성합니다.
        /// </summary>
        /// <param name="_kor"></param>
        /// <param name="_eng"></param>
        public TranslateText(string _kor, string _eng, string _jap, string _rus)
        {
            kor = _kor;
            eng = _eng;
            jap = _jap;
            rus = _rus;
        }
        #endregion
        #region Function
        //Public static
        /// <summary>
        /// 해당 TranslateText가 null 또는 비어있는 텍스트로 이루어져있는지 가져옵니다.
        /// </summary>
        /// <param name="_text"></param>
        /// <returns></returns>
        public static bool IsNullorEmpty(TranslateText _text)
        {
            if (_text == null)
                return true;

            if (string.IsNullOrEmpty(_text.kor) || string.IsNullOrEmpty(_text.eng) || string.IsNullOrEmpty(_text.jap) || string.IsNullOrEmpty(_text.rus))
                return true;

            return false;
        }

        //Public
        /// <summary>
        /// 특정 언어의 번역을 가져옵니다.
        /// </summary>
        /// <param name="_lang"></param>
        /// <returns></returns>
        public string GetText(SystemLanguage _lang)
        {
            switch (_lang)
            {
                case SystemLanguage.Korean:
                    return (kor != null) ? kor : "";
                case SystemLanguage.Japanese:
                    return (jap != null) ? jap : "";
                case SystemLanguage.Russian:
                    return (rus != null) ? rus : "";
                default:
                    return (eng != null) ? eng : "";
            }
        }
        /// <summary>
        /// 현재 언어로 번역을 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            return GetText(GlobalManager.Instance.LanguageMgr.Now.Value);
        }
        #endregion
    }
}