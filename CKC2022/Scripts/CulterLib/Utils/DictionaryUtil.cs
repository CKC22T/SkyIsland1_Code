using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Utils
{
    public static class DictionaryUtil
    {
        /// <summary>
        /// Dictionary의 Value들을 전부 가져옵니다.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static TKey[] GetKeys<TKey, TValue>(this Dictionary<TKey, TValue> dic)
        {
            TKey[] key = new TKey[dic.Count];
            dic.Keys.CopyTo(key, 0);

            return key;
        }
        /// <summary>
        /// 해당 Dictionary의 Value들을 전부 가져옵니다.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static TValue[] GetValues<TKey, TValue>(this Dictionary<TKey, TValue> dic)
        {
            TValue[] values = new TValue[dic.Count];
            dic.Values.CopyTo(values, 0);

            return values;
        }
    }
}