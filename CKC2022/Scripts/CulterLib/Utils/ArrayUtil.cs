using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Utils
{
    public static class ArrayUtil
    {
        /// <summary>
        /// 새로운 배열을 만들어서 _initData로 채웁니다.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="_length"></param>
        /// <param name="_initData"></param>
        /// <returns></returns>
        public static TType[] NewArray<TType>(int _length, TType _initData)
        {
            TType[] arr = new TType[_length];
            for (int i = 0; i < arr.Length; ++i)
                arr[i] = _initData;

            return arr;
        }
        /// <summary>
        /// 새로운 배열을 만듭니다.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="_length"></param>
        /// <param name="_initData"></param>
        /// <returns></returns>
        public static TType[] NewArray<TType>(int _length, Func<int, TType> _initDataFunc)
        {
            TType[] arr = new TType[_length];
            for (int i = 0; i < arr.Length; ++i)
                arr[i] = _initDataFunc(i);

            return arr;
        }
        /// <summary>
        /// 해당 배열을 카피(깊은복사)합니다.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static TType[] GetCopy<TType>(TType[] arr)
        {
            TType[] arrCopy = new TType[arr.Length];
            arr.CopyTo(arrCopy, 0);
            return arrCopy;
        }
        /// <summary>
        /// 해당 배열의 특정 위치의 값 하나를 가져옵니다.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="_arr"></param>
        /// <param name="_index"></param>
        /// <returns></returns>
        public static TType GetValue<TType>(TType[] _arr, int _index)
        {
            if (0 <= _index && _index < _arr.Length)
                return _arr[_index];
            else
                return default;
        }
        /// <summary>
        /// 해당 배열의 특정 데이터의 index 위치를 가져옵니다.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="_arr"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetIndex<TType>(TType[] _arr, TType data)
        {
            for (int i = 0; i < _arr.Length; ++i)
                if (_arr[i].Equals(data))
                    return i;

            return -1;
        }
    }
}