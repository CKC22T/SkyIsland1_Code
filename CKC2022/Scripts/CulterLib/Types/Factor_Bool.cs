using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Types
{
    public class Factor_Bool<TKey> : Factor<TKey, bool>
    {
        #region Type
        /// <summary>
        /// TotalFunc의 프리셋을 어떤것을 사용할지의 enum
        /// </summary>
        public enum ETotalType
        {
            And,
            Or,
        }
        #endregion

        #region Event
        public Factor_Bool(Func<bool[], bool> _totalFunc) : base(_totalFunc)
        {
        }
        public Factor_Bool(ETotalType _totalType) : base(null)
        {
            //TotalFunc의 프리셋 함수 설정
            if (_totalType == ETotalType.And)
                SetTotalFunc((bool[] _value) =>
                {
                    bool and = true;
                    foreach (var v in _value)
                        and &= v;
                    return and;
                });
            else if (_totalType == ETotalType.Or)
                SetTotalFunc((bool[] _value) =>
                {
                    bool or = false;
                    foreach (var v in _value)
                        or |= v;
                    return or;
                });
        }
        #endregion
    }
}