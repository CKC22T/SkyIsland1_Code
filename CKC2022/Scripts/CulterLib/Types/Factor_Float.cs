using System;
using System.Collections;
using System.Collections.Generic;
using CulterLib.Utils;
using UnityEngine;

namespace CulterLib.Types
{
    #region Type
    /// <summary>
    /// TotalFunc의 프리셋을 어떤것을 사용할지의 enum
    /// </summary>
    public enum EFloatFactorTotalType
    {
        Average,
        Add,
        Min,
        Max,
        Multifly
    }
    #endregion

    /// <summary>
    /// Factor<T> 클래스의 float형을 위한 확장기능 이 포함된 클래스
    /// </summary>
    public sealed class Factor_Float<TKey> : Factor<TKey, float>
    {
        #region Event
        public Factor_Float(Func<float[], float> _totalFunc) : base(_totalFunc)
        {
        }
        public Factor_Float(EFloatFactorTotalType _totalType) : base(null)
        {
            //TotalFunc의 프리셋 함수 설정
            if (_totalType == EFloatFactorTotalType.Average)
                SetTotalFunc((float[] _value) =>
                {
                    float all = 0;
                    foreach (var v in _value)
                        all += v;
                    return all / _value.Length;
                });
            else if(_totalType == EFloatFactorTotalType.Add)
                SetTotalFunc((float[] _value) =>
                {
                    float all = 0;
                    foreach (var v in _value)
                        all += v;
                    return all;
                });
            else if (_totalType == EFloatFactorTotalType.Min)
                SetTotalFunc((float[] _value) =>
                {
                    float min = float.MaxValue;
                    foreach (var v in _value)
                        if (v < min)
                            min = v;
                    return min;
                });
            else if (_totalType == EFloatFactorTotalType.Max)
                SetTotalFunc((float[] _value) =>
                {
                    float max = float.MinValue;
                    foreach (var v in _value)
                        if (max < v)
                            max = v;
                    return max;
                });
            else if (_totalType == EFloatFactorTotalType.Multifly)
                SetTotalFunc((float[] _value) =>
                {
                    float multifly = 1.0f;
                    foreach (var v in _value)
                        multifly *= v;
                    return multifly;
                });
        }
        #endregion
        #region Function
        /// <summary>
        /// factor가 0혹은 더 작은 효과를 제거합니다.
        /// </summary>
        public void RemoveMinZero()
        {
            bool isRemoved = false;
            foreach (var v in m_Factor)
            {
                if (v.Value < 0)
                {
                    m_Factor.Remove(v.Key);
                    isRemoved = true;
                }
            }

            if (isRemoved)
                PostChangeEvent();
        }
        /// <summary>
        /// 현재 활성화된 모든 효과에 숫자를 더합니다.
        /// </summary>
        /// <param name="add">추가할 숫자의 양</param>
        public void AddToAll(float add)
        {
            foreach (var v in DictionaryUtil.GetKeys(m_Factor))
                m_Factor[v] += add;

            PostChangeEvent();
        }
        #endregion
    }
}