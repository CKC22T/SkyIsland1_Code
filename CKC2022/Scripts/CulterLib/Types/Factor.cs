using System;
using System.Collections.Generic;
using CulterLib.Utils;

namespace CulterLib.Types
{
    /// <summary>
    /// 특정 효과의 데이터를 저장하기 위한 클래스입니다.
    /// </summary>
    public class Factor<TKey, TValue>
    {
        #region Get,Set
        /// <summary>
        /// 현재 효과가 존재하는지
        /// </summary>
        public bool IsHave { get => 0 < m_Factor.Count; }
        /// <summary>
        /// 현재 저장된 팩터의 갯수
        /// </summary>
        public int Count { get => m_Factor.Count; }
        /// <summary>
        /// 현재 저장된 팩터를 전부 합하면 어떻게 되는지
        /// 참고 : TotalFunc를 넣지 않으면 에러남
        /// </summary>
        public TValue Total { get => m_OnNeedTotalFunc(DictionaryUtil.GetValues(m_Factor)); }
        #endregion
        #region Value
        protected Dictionary<TKey, TValue> m_Factor = new Dictionary<TKey, TValue>();   //팩터 리스트
        private Func<TValue[], TValue> m_OnNeedTotalFunc;                               //팩터를 전부 합힐 때의 함수
        private Action<object> m_OnChangedFunc;                                         //팩터가 변할 때 호출되는 이벤트
        #endregion

        #region Event
        public Factor(Func<TValue[], TValue> _totalFunc)
        {
            SetTotalFunc(_totalFunc);
        }
        #endregion
        #region Function
        /// <summary>
        /// 효과 데이터를 저장합니다.
        /// </summary>
        /// <param name="id">효과 ID</param>
        /// <param name="factor">효과 정보</param>
        public void Add(TKey id, TValue factor = default)
        {
            if(m_Factor.ContainsKey(id))
                m_Factor[id] = factor;      //이미 들어있는 경우는 수정을 한다.
            else
                m_Factor.Add(id, factor);   //없는 경우 추가한다.

            PostChangeEvent();
        }
        /// <summary>
        /// 해당 Id의 효과 데이터가 있는지 가져옵니다.
        /// </summary>
        /// <param name="id"></param>
        public bool GetContains(TKey id)
        {
            return m_Factor.ContainsKey(id);
        }
        /// <summary>
        /// 효과 데이터를 가져옵니다.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TValue Get(TKey id)
        {
            if (m_Factor.TryGetValue(id, out var v))
                return v;
            else
                return default;
        }
        /// <summary>
        /// 존재하는 경우 해당 ID의 효과 데이터를 제거합니다.
        /// </summary>
        /// <param name="id">효과의 ID</param>
        public void Remove_ByID(TKey id)
        {
            if (m_Factor.Remove(id))
                PostChangeEvent();
        }
        /// <summary>
        /// 현재 존재하는 키들을 순회합니다. 제거나 그런거할때 유용함
        /// </summary>
        /// <param name="_foreachAct"></param>
        public void ForEach_Key(Action<TKey> _foreachAct)
        {
            foreach (var v in m_Factor.Keys)
                _foreachAct(v);
        }

        /// <summary>
        /// 팩터를 전부 합칠 때의 함수를 설정합니다. (Total값을 가져올 때 사용함)
        /// </summary>
        /// <param name="_func"></param>
        public void SetTotalFunc(Func<TValue[], TValue> _func)
        {
            m_OnNeedTotalFunc = _func;
        }
        /// <summary>
        /// 팩터를 전부 합칠때의 함수를 제거해버립니다.
        /// </summary>
        public void ClearTotalFunc()
        {
            m_OnNeedTotalFunc = null;
        }

        /// <summary>
        /// Factor 내용이 변했을 때 호출되는 이벤트를 추가합니다.
        /// </summary>
        /// <param name="_func"></param>
        /// <param name="_isCallNow"></param>
        public void AddFactorChangeEvent(Action<object> _func, bool _isCallNow = true)
        {
            m_OnChangedFunc += _func;

            if (_isCallNow)
                PostChangeEvent();
        }
        /// <summary>
        /// Factor 내용이 변했을 때 호출되던 이벤트를 제거합니다.
        /// </summary>
        /// <param name="_func"></param>
        public void RemoveFactorChangeEvent(Action<object> _func)
        {
            m_OnChangedFunc -= _func;
        }
        /// <summary>
        /// 강제로 Factor 내용이 변했을 때의 이벤트를 호출합니다.
        /// </summary>
        public void PostChangeEvent()
        {
            m_OnChangedFunc?.Invoke(this);
        }
        #endregion
    }
}