using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CulterLib.Types;
using Sirenix.OdinInspector;
using Utils;

namespace CulterLib.Types
{
    /// <summary>
    /// 데이터 사용 관련 처리들이 들어있는 기본 클래스입니다. 상속해서 사용하면됨
    /// </summary>
    public abstract class Mono_Data : MonoBehaviour
    {
        #region Type
        /// <summary>
        /// Notifier.DataChanged를 저장하기 위한 구조체입니다.
        /// </summary>
        private struct SDataChanged
        {
            public object data;
            public object eve;
            public Action add;
            public Action remove;

            public SDataChanged(object _data, object _eve, Action _add, Action _remove)
            {
                data = _data;
                eve = _eve;
                add = _add;
                remove = _remove;
            }
        }
        #endregion

        #region Inspector
        [Title("Mono_Data")]
        [TabGroup("Option"), SerializeField, LabelText("기본 ID")] string m_DefaultID = "";
        #endregion
        #region Get,Set
        /// <summary>
        /// 해당 오브젝트의 ID입니다.
        /// </summary>
        public Notifier<string> ID { get; private set; }
        /// <summary>
        /// 해당 오브젝트가 초기화되었는지
        /// </summary>
        public bool IsInited { get; private set; }
        /// <summary>
        /// 해당 오브젝트가 데이터 이벤트를 사용할지 여부입니다.
        /// </summary>
        public bool IsDataUse
        {
            get => m_IsDataUse;
            set
            {
                if (m_IsDataUse == value)
                    return; //현재 상태와 같으면 무시

                m_IsDataUse = value;
                UpdateData();
            }
        }
        /// <summary>
        /// 현재 데이터 이벤트가 활성화되었는지 여부입니다.
        /// </summary>
        public bool IsDataActive { get; private set; }
        #endregion
        #region Value
        private bool m_IsDataUse;
        private List<SDataChanged> m_DataChangeds;
        #endregion

        #region Event
        public void Init(string _id = null)
        {
            //중복 초기화 방지
            if (IsInited)
                return;

            //변수 초기화
            ID = new Notifier<string>(OnInitID(_id));
            IsInited = true;
            IsDataUse = true;

            //구성요소 초기화
            OnInitData();
        }

        //Unity Event
        protected virtual void OnEnable()
        {
            //켜질땐 자신의 Data 이벤트를 연결해준다.
            if (IsDataUse)
                UseData();
        }
        protected virtual void OnDisable()
        {
            //꺼질땐 자신의 Data 이벤트를 해제해준다.
            StopData();
        }
        protected virtual void OnDestroy()
        {
            //삭제시 자신의 Data 이벤트르 해제해준다.
            StopData();
        }

        //Mono_Data Event
        /// <summary>
        /// ID설정이 필요할 때 호출됩니다.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        protected virtual string OnInitID(string _id)
        {
            return string.IsNullOrEmpty(m_DefaultID) ? _id : m_DefaultID;
        }
        /// <summary>
        /// 초기화시점에 호출됩니다.
        /// </summary>
        protected virtual void OnInitData()
        {
        }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// 데이터 Instant이벤트를 추가합니다.
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="_eve"></param>
        public void AddDataChangeEvent<T>(INotifiable<T> _data, Action<T> _eve, bool _isCallNow = false)
        {
            if (m_DataChangeds == null)
                m_DataChangeds = new List<SDataChanged>();

            //추가
            m_DataChangeds.Add(new SDataChanged(_data, _eve, () => _data.AddDataChanged(_eve, true), () => _data.OnDataChanged -= _eve));

            //데이터 이벤트 연결
            if (IsDataActive)
                _data.AddDataChanged(_eve, _isCallNow);
            else if (_isCallNow)
                _eve(_data.Value);
        }
        /// <summary>
        /// 데이터 Instant이벤트를 제거합니다.
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="_instantEvent"></param>
        public bool RemoveDataChangeEvent<T>(INotifiable<T> _data, Action<T> _eve) where T : struct
        {
            if (m_DataChangeds == null)
                return false;

            //제거
            for (int i = 0; i < m_DataChangeds.Count; i++)
                if (m_DataChangeds[i].data == _data && (m_DataChangeds[i].eve as Action<T>) == _eve)
                {
                    //제거
                    m_DataChangeds.RemoveAt(i);

                    //데이터 이벤트 해제
                    if (IsDataActive)
                        _data.OnDataChanged -= _eve;

                    return true;
                }

            return false;
        }

        /// <summary>
        /// 데이터 Update이벤트를 추가합니다.
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="_eve"></param>
        public void AddUpdateNotifyEvent<T>(Notifier<T> _data, Action<T> _eve, bool _isCallNow = false)
        {
            if (m_DataChangeds == null)
                m_DataChangeds = new List<SDataChanged>();

            //추가
            m_DataChangeds.Add(new SDataChanged(_data, _eve, () => _data.AddUpdateNotify(_eve, true), () => _data.OnUpdateNotify -= _eve));

            //데이터 이벤트 연결
            if (IsDataActive)
                _data.AddUpdateNotify(_eve, _isCallNow);
            else if (_isCallNow)
                _eve(_data.Value);
        }
        /// <summary>
        /// 데이터 Instant이벤트를 제거합니다.
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="_instantEvent"></param>
        public bool RemoveUpdateNotifyEvent<T>(Notifier<T> _data, Action<T> _eve) where T : struct
        {
            if (m_DataChangeds == null)
                return false;

            //제거
            for (int i = 0; i < m_DataChangeds.Count; i++)
                if (m_DataChangeds[i].data == _data && (m_DataChangeds[i].eve as Action<T>) == _eve)
                {
                    //제거
                    m_DataChangeds.RemoveAt(i);

                    //데이터 이벤트 해제
                    if (IsDataActive)
                        _data.OnUpdateNotify -= _eve;

                    return true;
                }

            return false;
        }

        //Private
        /// <summary>
        /// 현재 해당 오브젝트의 상태에 따라 이벤트 연결 또는 해제해줍니다. 
        /// </summary>
        private void UpdateData()
        {
            //오브젝트가 켜져있고, Data 이벤트 사용시면 연결, 아닌경우 해제 
            if (gameObject.activeInHierarchy && IsDataUse)
                UseData();
            else
                StopData();
        }
        /// <summary>
        /// 데이터 이벤트를 연결해줍니다.
        /// </summary>
        private void UseData()
        {
            //연결 안된상태일 경우 연결
            if (!IsDataActive)
            {
                if (m_DataChangeds != null)
                    foreach (var v in m_DataChangeds)
                        v.add();

                IsDataActive = true;
            }
        }
        /// <summary>
        /// 데이터이벤트를 해제해줍니다.
        /// </summary>
        private void StopData()
        {
            //연결된 상태인 경우 연결 해제
            if (IsDataActive)
            {
                if (m_DataChangeds != null)
                    foreach (var v in m_DataChangeds)
                        v.remove();

                IsDataActive = false;
            }
        }
        #endregion
    }
}