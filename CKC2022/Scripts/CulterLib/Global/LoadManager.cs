using System;
using System.Collections;
using System.Collections.Generic;
using CulterLib.Global.Chart;
using CulterLib.Presets;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CulterLib.Global.Load
{
    public abstract class LoadManager : MonoBehaviour
    {
        #region Type
        /// <summary>
        /// 차트 로딩 데이터
        /// </summary>
        [System.Serializable] public struct STableLoad
        {
            public ChartManager chart;
            public string tableName;
            public Type tableType;

            public STableLoad(ChartManager _chart, string _tableName, Type _tableType)
            {
                chart = _chart;
                tableName = _tableName;
                tableType = _tableType;
            }
        }
        #endregion

        #region Inspector - Editor
#if UNITY_EDITOR
        [TabGroup("Debug"), SerializeField] public bool IsDebugLog;
#endif
        #endregion
        #region Get,Set
        /// <summary>
        /// 로딩한적이 있는지
        /// </summary>
        public bool IsLoaded { get; private set; }
        /// <summary>
        /// 로딩 퍼센트
        /// </summary>
        public float Persent { get => (float)m_LoadCount / GetLoadCount(1); }
        #endregion
        #region Value
        private ChartManager[] m_Const;
        private STableLoad[] m_Table;
        private int m_LoadCount;
        #endregion

        #region Event
        internal void Init()
        {
        }

        /// <summary>
        /// 1. 로딩 시작
        /// </summary>
        protected virtual void OnLoadStart() { }
        /// <summary>
        /// 2. 차트에서 따로 고정값 로딩이 필요할 때 호출됩니다.
        /// </summary>
        /// <returns></returns>
        protected abstract ChartManager[] OnNeedConstLoad();
        /// <summary>
        /// 3. 차트에서 따로 테이블 로딩이 필요할 때 호출됩니다.
        /// </summary>
        protected abstract STableLoad[] OnNeedTableLoad();
        /// <summary>
        /// 4. 로딩 완료
        /// </summary>
        protected virtual void OnLoadEnd()
        {
#if CULTERLIB_THEBACKEND
            //기본값(뒤끝) : 뒤끝의 경우 자동저장으로 설정
            foreach (var v in _DataMgr.SaveMgr)
                if (v is SaveManager_TheBackend_Single)
                    v.IsAutoSave = true;
#endif
        }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// 로딩을 진행합니다.
        /// </summary>
        /// <param name=""></param>
        /// <param name="_onEnd"></param>
        public void Load_All(Action<float> _onProgress, Action<bool> _onEnd)
        {
            if (IsLoaded)
            {
                _onProgress?.Invoke(1.0f);
                _onEnd?.Invoke(true);
                return;
            }

            //변수 초기화
            m_Const = OnNeedConstLoad();
            m_Table = OnNeedTableLoad();

            //로딩 시작
            OnLoadStart();
            ConstLoad(_onProgress, _onEnd);
        }
        /// <summary>
        /// 초기화만 진행합니다.
        /// 테스트용으로 GameScene에서 시작할 때만 사용됨!
        /// </summary>
        public void Load_InitOnly()
        {
            if (IsLoaded)
                return;

            GlobalManager.Instance.DataMgr.InitTable();
            GlobalManager.Instance.DataMgr.InitValue();
            GlobalManager.Instance.DataMgr.InitGame();
            IsLoaded = true;
        }

        //Private
        private void ConstLoad(Action<float> _onProgress, Action<bool> _onEnd)
        {
            //로드
            if (m_Const == null || m_Const.Length <= 0)
                TableLoad(_onProgress, _onEnd);
            else
                foreach (var v in m_Const)
                    GlobalManager.Instance.DataMgr.LoadConst(v, (_isSuc) => 
                    {
                        if (_isSuc)
                        {
#if UNITY_EDITOR
                            if (IsDebugLog)
                                Debug.Log($"[LoadManager] ConstLoad");
#endif
                            ++m_LoadCount;
                            _onProgress?.Invoke(Persent);
                            if (GetLoadCount(0) <= m_LoadCount)
                                TableLoad(_onProgress, _onEnd);
                        }
                        else
                            EndLoad(false, _onEnd);
                    });
        }
        private void TableLoad(Action<float> _onProgress, Action<bool> _onEnd)
        {
            //임시함수
            void onend()
            {
                GlobalManager.Instance.DataMgr.InitTable();
                GlobalManager.Instance.DataMgr.InitValue();
                GlobalManager.Instance.DataMgr.InitGame();
                OnLoadEnd();
                EndLoad(true, _onEnd);
            }

            //로드
            if (m_Table == null || m_Table.Length <= 0)
                onend();
            else
                foreach (var v in m_Table)
                    GlobalManager.Instance.DataMgr.LoadTable(v.chart, v.tableName, v.tableType, (_isSuc) =>
                    {
                        if (_isSuc)
                        {
#if UNITY_EDITOR
                            if (IsDebugLog)
                                Debug.Log($"[LoadManager] TableLoad\ntableName : {v.tableName}");
#endif
                            ++m_LoadCount;
                            _onProgress?.Invoke(Persent);
                            if (GetLoadCount(1) <= m_LoadCount)
                                onend();
                        }
                        else
                            EndLoad(false, _onEnd);
                    });
        }
        private void EndLoad(bool _isSuc, Action<bool> _onEnd)
        {
            if (_isSuc)
                IsLoaded = true;

            _onEnd?.Invoke(_isSuc);
        }

        private int GetLoadCount(int _index)
        {
            int count = 0;
            if (0 <= _index)
                count += m_Const.Length;
            if (1 <= _index)
                count += m_Table.Length;

            return count;
        }
        #endregion
    }
}