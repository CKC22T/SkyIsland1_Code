using System;
using System.Collections;
using System.Collections.Generic;
using CulterLib.Global.Data;
using CulterLib.Types;
using CulterLib.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CulterLib.Global.Chart
{
    public abstract class ChartManager : MonoBehaviour
    {
        #region Get,Set
        /// <summary>
        /// 해당 ChartManager의 부모 DataManager
        /// </summary>
        public DataManager ParDataMgr { get; private set; }
        #endregion

        #region Event
        public void Init(DataManager _par)
        {
            ParDataMgr = _par;
            OnInit();
        }

        //ChartManager Event
        protected virtual void OnInit() { }
        protected virtual void OnLoad(string _chartName, Action<Dictionary<string, string>> _onEnd)
        {
            _onEnd?.Invoke(null);
        }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// 차트를 로드해서 const값을 넣을 수 있게 넘겨줍니다.
        /// </summary>
        /// <param name="_chartName"></param>
        /// <param name="_onLoad"></param>
        /// <param name="_onEnd"></param>
        public void LoadConst(string _chartName, Action<bool> _onEnd)
        {
            OnLoad(_chartName, (_chart) =>
            {
                if (_chart is { Count: > 0 })
                    ParDataMgr.OnInitConst(_chart);
                _onEnd?.Invoke(true);
            });
        }
        /// <summary>
        /// 해당 차트를 로드해서 테이블에 채웁니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_chartName"></param>
        /// <param name="_tableKeys"></param>
        /// <param name="_datas"></param>
        /// <param name="_type"></param>
        /// <param name="_onEnd"></param>
        public void LoadTable(string _chartName, Type _type, Action<bool> _onEnd)
        {
            OnLoad(_chartName, (_chart) =>
                {   //로드된 차트를 ParDataMgr의 테이블에 채우기
                    if (_chart != null)
                    {   //해당 테이블의 key list 가져오거나 새로 생성하기
                        if (!ParDataMgr.TableKeys.TryGetValue(_chartName, out var chartKeys))
                        {
                            chartKeys = new List<string>();
                            (ParDataMgr.TableKeys as Dictionary<string, IReadOnlyList<string>>).Add(_chartName, chartKeys);
                        }
                        //해당 테이블 새로 만들거나 덮어쓰기
                        foreach (var v in _chart)
                        {
                            if (ParDataMgr.TableDatas.TryGetValue(v.Key, out var d))
                                JsonUtility.FromJsonOverwrite(v.Value, d);
                            else
                            {
                                (chartKeys as List<string>).Add(v.Key);
                                (ParDataMgr.TableDatas as Dictionary<string, object>).Add(v.Key, JsonUtility.FromJson(v.Value, _type));
                            }
                        }
                        _onEnd?.Invoke(true);
                    }
                    else
                        _onEnd?.Invoke(false);
                });
        }
        /// <summary>
        /// 해당 차트를 비동기로 로드해서 테이블에 채웁니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_chartName"></param>
        /// <param name="_tableKeys"></param>
        /// <param name="_datas"></param>
        public void LoadTable<T>(string _chartName, Action<bool> _onEnd) where T : class
        {
            LoadTable(_chartName, typeof(T), _onEnd);
        }
        #endregion
    }
}