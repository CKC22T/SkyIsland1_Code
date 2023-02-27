using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Utils
{
    public static class ListUtil
    {
        /// <summary>
        /// 해당 List의 값들을 전부 가져옵니다.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="_list"></param>
        /// <returns></returns>
        public static TType[] GetValues<TType>(List<TType> _list)
        {
            TType[] values = new TType[_list.Count];
            _list.CopyTo(values, 0);

            return values;
        }
        /// <summary>
        /// 해당 List의 특정 위치의 값 하나를 가져옵니다.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="_list"></param>
        /// <param name="_index"></param>
        /// <returns></returns>
        public static TType GetValue<TType>(List<TType> _list, int _index)
        {
            if (0 <= _index && _index < _list.Count)
                return _list[_index];
            else
                return default;
        }
    }
}