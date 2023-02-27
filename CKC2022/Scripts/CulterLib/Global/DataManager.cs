using CulterLib.Global.Chart;
using CulterLib.Types;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace CulterLib.Global.Data
{
    /// <summary>
    /// Table, Save 데이터를 관리하는 매니저
    /// 단, SaveData는 다른곳에서 관리될 수 있다.   
    /// </summary>
    public abstract class DataManager : MonoBehaviour
    {
        #region Inspector
        [Title("DataManager")]
        [SerializeField, TabGroup("Manager"), LabelText("클라이언트 차트")] private ChartManager_Resources m_ClientChartMgr;
        [SerializeField, TabGroup("Manager"), LabelText("서버 차트")] private ChartManager_Resources m_ServerChartMgr;
        #endregion
        #region Get,Set
        //매니저
        /// <summary>
        /// 클라이언트 차트
        /// </summary>
        public ChartManager_Resources ClientChart { get => m_ClientChartMgr; }
        /// <summary>
        /// 서버 차트
        /// </summary>
        public ChartManager ServerChart { get => m_ServerChartMgr; }
        
        //초기화
        /// <summary>
        /// 테이블 초기화 완료 여부
        /// </summary>
        public bool IsInitTableDone { get; private set; }
        /// <summary>
        /// 변수 초기화 완료 여부
        /// </summary>
        public bool IsInitValueDone { get; private set; }
        /// <summary>
        /// 게임 초기화 완료 여부
        /// </summary>
        public bool IsInitGameDone { get; private set; }

        //테이블 데이터
        /// <summary>
        /// 현재 로드된 테이블들에 각각 들어있는 키값
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<string>> TableKeys { get; private set; }
        /// <summary>
        /// 현재 로드된 (테이블)데이터들
        /// </summary>
        public IReadOnlyDictionary<string, object> TableDatas { get; private set; }

        //기본 테이블
        /// <summary>
        /// TextTable에 존재하는 ID들
        /// </summary>
        public IReadOnlyList<string> TextTableID { get; private set; }
        #endregion

        #region Event
        /// <summary>
        /// 초기화를 실행합니다.
        /// </summary>
        public void Init()
        {
            //변수 초기화
            TableKeys = new Dictionary<string, IReadOnlyList<string>>();
            TableDatas = new Dictionary<string, object>();

            if (m_ClientChartMgr)
                m_ClientChartMgr.Init(this);

            //기본 데이터 로드
            (TableKeys as Dictionary<string, IReadOnlyList<string>>).Add("TextTable", new List<string>());
            TextTableID = TableKeys["TextTable"];

            //나머지 데이터 로드
            OnInitData();
        }
        /// <summary>
        /// 테이블 데이터를 초기화 직후의 초기화를 수행합니다.
        /// </summary>
        internal void InitTable()
        {
            OnInitTable();

            IsInitTableDone = true;
        }
        /// <summary>
        /// 변수(세이브) 데이터 초기화 직후의 초기화를 수행합니다.
        /// </summary>
        internal void InitValue()
        {
            OnInitValue();

            IsInitValueDone = true;
        }
        /// <summary>
        /// 게임 시작 직전 초기화를 수행합니다. 
        /// </summary>
        internal void InitGame()
        {
            OnInitGame();

            IsInitGameDone = true;
        }

        //DataManager Event
        /// <summary>
        /// 여기서 const데이터를 해당 차트에서 가져옵니다.
        /// </summary>
        internal virtual void OnInitConst(Dictionary<string, string> _chart) { }
        /// <summary>
        /// 여기서 기본 테이블 데이터를 등록합니다.
        /// </summary>
        protected virtual void OnInitData() { }

        //LoadManager Event
        /// <summary>
        /// 테이블 데이터를 초기화 직후의 초기화를 수행합니다.
        /// </summary>
        protected virtual void OnInitTable() { }
        /// <summary>
        /// 변수(세이브) 데이터 초기화 직후의 초기화를 수행합니다.
        /// </summary>
        protected virtual void OnInitValue() { }
        /// <summary>
        /// 게임 시작 직전 초기화를 수행합니다. 
        /// </summary>
        protected virtual void OnInitGame() { }
        #endregion
        #region Function
        //Public - Load
        /// <summary>
        /// 테이블 데이터를 추가합니다.
        /// </summary>
        /// <param name="_tableID"></param>
        /// <param name="_id"></param>
        /// <param name="_data"></param>
        public void AddTable(string _tableID, string _id, object _data)
        {
            if (!TableKeys.ContainsKey(_tableID))
                (TableKeys as Dictionary<string, IReadOnlyList<string>>).Add(_tableID, new List<string>());

            if (!TableDatas.TryGetValue(_id, out var d))
            {
                (TableKeys[_tableID] as List<string>).Add(_id);
                (TableDatas as Dictionary<string, object>).Add(_id, _data);
            }
        }
        /// <summary>
        /// Const 데이터를 로드합니다.
        /// </summary>
        /// <param name="_chartMgr"></param>
        /// <param name="_onEnd"></param>
        /// <returns></returns>
        public void LoadConst(ChartManager _chartMgr, Action<bool> _onEnd)
        {
            _chartMgr.LoadConst("Const", _onEnd);
        }
        /// <summary>
        /// 테이블 데이터를 로드합니다. 
        /// </summary>
        /// <param name="_chart"></param>
        /// <param name="_tableName"></param>
        /// <param name="_type"></param>
        /// <param name="_onEnd"></param>
        public void LoadTable(ChartManager _chart, string _tableName, Type _type, Action<bool> _onEnd)
        {
            _chart.LoadTable(_tableName, _type, _onEnd);
        }

        //Public - File
        /// <summary>
        /// 해당 ID의 아이콘을 가져옵니다.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public virtual Sprite GetIcon(string _id)
        {
            return Resources.Load<Sprite>($"Icon/{_id}");
        }

        //Public - Table
        /// <summary>
        /// 테이블 데이터를 가져옵니다.
        /// </summary>
        /// <typeparam name="T">가져올 데이터 타입, class</typeparam>
        /// <param name="_key">해당 데이터의 key</param>
        /// <returns></returns>
        public T GetTableData<T>(string _key) where T : class
        {
            if (_key == null)
            {
                Debug.LogError("DataManager.GetTableData Failed (key == null)");
                return null;
            }
            else if (TableDatas.TryGetValue(_key, out object data))
                return data as T;
            else
            {
                Debug.LogError($"DataManager.GetTableData Failed (key == {_key}, data not contain)");
                return null;
            }
        }
        /// <summary>
        /// 테이블 데이터를 가져옵니다.
        /// </summary>
        /// <param name="_key"></param>
        /// <returns></returns>
        public object GetTableData(string _key)
        {
            if (_key == null)
            {
                Debug.LogError("DataManager.GetTableData Failed (key == null)");
                return null;
            }
            else if (TableDatas.TryGetValue(_key, out object data))
                return data;
            else
            {
                Debug.LogError($"DataManager.GetTableData Failed (key == {_key}, data not contain)");
                return null;
            }
        }
        /// <summary>
        /// 테이블 데이터를 가져옵니다,
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_key"></param>
        /// <param name="_data"></param>
        /// <returns></returns>
        public bool TryGetTableData<T>(string _key, out T _data) where T : class
        {
            if (TableDatas.TryGetValue(_key, out var table))
            {
                _data = table as T;
                if (_data != null)
                    return true;
                else
                    return false;
            }
            else
            {
                _data = null;
                return false;
            }
        }
        /// <summary>
        /// 테이블데이터를 가져옵니다.
        /// </summary>
        /// <param name="_key"></param>
        /// <param name="_data"></param>
        /// <returns></returns>
        public bool TryGetTableData(string _key, out object _data)
        {
            if (TableDatas.TryGetValue(_key, out var table))
            {
                _data = table;
                if (_data != null)
                    return true;
                else
                    return false;
            }
            else
            {
                _data = null;
                return false;
            }
        }

        //Public - Table - 기본 테이블
        /// <summary>
        /// TextTable 데이터를 가져옵니다.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public TranslateText GetTextTableData(string _key)
        {
            if (_key == null)
            {
                Debug.LogError("DataManager.GetTableData Failed (key == null)");
                return null;
            }
            else if (TableDatas.TryGetValue(_key, out object data))
                return data as TranslateText;
            else
                return new TranslateText(_key, _key, _key, _key);
        }
        /// <summary>
        /// TextTable 데이터를 가져옵니다.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public bool TryGetTextTableData(string _key, out TranslateText _data)
        {
            _data = GetTextTableData(_key);
            return _data != null;
        }
        #endregion
        #region Function - Editor
#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button("Setup")]
        public void Setup()
        {
            //차트가 없으면 가져오기
            if (!m_ClientChartMgr)
                m_ClientChartMgr = GetComponentInChildren<ChartManager_Resources>();
        }
#endif
        #endregion
    }
}