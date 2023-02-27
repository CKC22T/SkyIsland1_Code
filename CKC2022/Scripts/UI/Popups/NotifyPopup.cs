using System;
using System.Collections;
using System.Collections.Generic;
using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using CulterLib.UI.Controls;
using CulterLib.Utils;

namespace CulterLib.UI.Popups
{
    public class NotifyPopup : PopupWindow
    {
        public static NotifyPopup Instance { get; private set; }
        #region Type
        /// <summary>
        /// 버튼 컴포넌트 저장용
        /// </summary>
        [Serializable] private struct SBtnInspector
        {
            public Control_Button btn;
            public Control_Text text;
        }
        /// <summary>
        /// 버튼에 들어갈 데이터 저장용
        /// </summary>
        public struct SBtnData
        {
            public string id;
            public Action eve;

            public SBtnData(string _id, Action _eve)
            {
                id = _id;
                eve = _eve;
            }
        }
        /// <summary>
        /// 알림 정보
        /// </summary>
        private struct SNotifyData
        {
            public string mainText;
            public string subText;
            public SBtnData[] btnDatas;

            public SNotifyData(string _mainText, string _subText, params SBtnData[] _btnData)
            {
                mainText = _mainText;
                subText = _subText;
                btnDatas = _btnData;
            }
        }
        #endregion

        #region Inspector
        [Title("NotifyPopup")]
        [TabGroup("Component"), SerializeField] private RectTransform m_RootFrame;
        [TabGroup("Component"), SerializeField] private Text m_MainText;
        [TabGroup("Component"), SerializeField] private RectTransform m_SubFrmae;
        [TabGroup("Component"), SerializeField] private Text m_SubText;
        [TabGroup("Component"), SerializeField] private SBtnInspector[] m_Btn;

        [Title("NotifyPopup")]
        [TabGroup("Option"), SerializeField] private float m_SubFrameBlank;
        #endregion
        #region Value
        private float m_RootFrame_MinHeight;
        private Queue<SNotifyData> m_Queue = new Queue<SNotifyData>();  //다음 알림 정보들
        #endregion

        #region Event
        public void Open(string _mainText, string _subText, params SBtnData[] _btnData)
        {
            m_Queue.Enqueue(new SNotifyData(_mainText, _subText, _btnData));

            //알림큐에 들어있는것이 하나뿐이면 (=현재 출력된 알림이 없음) 바로 출력
            if (m_Queue.Count == 1)
            {
                Open();
                SetNotify(m_Queue.Peek());
            }
        }

        //PopupWindow Event
        protected override void OnInitSingleton()
        {
            base.OnInitSingleton();
            Instance = this;
        }
        protected override void OnInitData()
        {
            base.OnInitData();

            //변수 초기화
            m_RootFrame_MinHeight = m_RootFrame.sizeDelta.y - m_SubFrmae.sizeDelta.y;

            //이벤트 초기화
            foreach (var v in m_Btn)
                v.btn.OnBtnClickFunc += (btn) =>
                {
                    //눌린 버튼에 따른 이벤트 호출
                    var data = m_Queue.Dequeue();
                    for (int i = 0; i < m_Btn.Length; ++i)
                        if (m_Btn[i].btn == btn)
                            data.btnDatas[i].eve?.Invoke();

                    //팝업 종료
                    Close();
                };
        }
        protected override void OnEndClose()
        {
            base.OnEndClose();

            if (0 < m_Queue.Count)
            {   //알림큐가 남아있으면 바로 다음알림 출력
                Open();
                SetNotify(m_Queue.Peek());
            }
        }
        #endregion
        #region Function
        //Private
        /// <summary>
        /// 해당 알림으로 내용을 설정합니다.
        /// </summary>
        /// <param name="_data"></param>
        private void SetNotify(SNotifyData _data)
        {
            //메인텍스트
            m_MainText.text = _data.mainText;

            //서브텍스트
            m_SubText.text = _data.subText;
            if (string.IsNullOrEmpty(_data.subText))
                m_SubFrmae.sizeDelta = new Vector2(m_SubFrmae.sizeDelta.x, 0);
            else
                m_SubFrmae.sizeDelta = new Vector2(m_SubFrmae.sizeDelta.x, UITextUtil.GetSize(m_SubText).y + m_SubFrameBlank);
            m_RootFrame.sizeDelta = new Vector2(m_RootFrame.sizeDelta.x, m_RootFrame_MinHeight + m_SubFrmae.sizeDelta.y);

            //버튼 설정
            for (int i = 0; i < m_Btn.Length; ++i)
                if (i < _data.btnDatas.Length)
                {
                    m_Btn[i].btn.gameObject.SetActive(true);
                    m_Btn[i].text.ID.Value = _data.btnDatas[i].id;
                    m_Btn[i].text.UpdateLanguage();
                }
                else
                    m_Btn[i].btn.gameObject.SetActive(false);
        }
        #endregion
    }
}