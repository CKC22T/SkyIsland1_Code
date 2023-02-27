using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Types
{
    /// <summary>
    /// Factor<T> 클래스의 float형을 위한 확장기능 이 포함된 클래스
    /// </summary>
    public sealed class Factor_Int<TKey> : Factor<TKey, int>
    {
        #region Type
        /// <summary>
        /// TotalFunc의 프리셋을 어떤것을 사용할지의 enum
        /// </summary>
        public enum ETotalType
        {
            Add,
            Min,
            Max,
        }
        #endregion

        #region Event
        public Factor_Int(Func<int[], int> _totalFunc) : base(_totalFunc)
        {
        }
        public Factor_Int(ETotalType _totalType) : base(null)
        {
            //TotalFunc의 프리셋 함수 설정
            if (_totalType == ETotalType.Add)
                SetTotalFunc((int[] _value) =>
                {
                    int all = 0;
                    foreach (var v in _value)
                        all += v;
                    return all;
                });
            else if (_totalType == ETotalType.Min)
                SetTotalFunc((int[] _value) =>
                {
                    int min = int.MaxValue;
                    foreach (var v in _value)
                        if (v < min)
                            min = v;
                    return min;
                });
            else if (_totalType == ETotalType.Max)
                SetTotalFunc((int[] _value) =>
                {
                    int max = int.MinValue;
                    foreach (var v in _value)
                        if (max < v)
                            max = v;
                    return max;
                });
        }
        #endregion
    }
}