using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Types
{
    /// <summary>
    /// Factor<T> 클래스의 Vector2형을 위한 확장기능 이 포함된 클래스
    /// </summary>
    public sealed class Factor_Vector2<TKey> : Factor<TKey, Vector2>
    {
        #region Type
        /// <summary>
        /// TotalFunc의 프리셋을 어떤것을 사용할지의 enum
        /// </summary>
        public enum ETotalType
        {
            Average,
            Add,
        }
        #endregion

        #region Event
        public Factor_Vector2(Func<Vector2[], Vector2> _totalFunc) : base(_totalFunc)
        {
        }
        public Factor_Vector2(ETotalType _totalType) : base(null)
        {
            //TotalFunc의 프리셋 함수 설정
            if (_totalType == ETotalType.Average)
                SetTotalFunc((Vector2[] _value) =>
                {
                    Vector2 all = Vector2.zero;
                    foreach (var v in _value)
                        all += v;
                    return all / _value.Length;
                });
            else if (_totalType == ETotalType.Add)
                SetTotalFunc((Vector2[] _value) =>
                {
                    Vector2 all = Vector2.zero;
                    foreach (var v in _value)
                        all += v;
                    return all;
                });
        }
        #endregion
    }
}