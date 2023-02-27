using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class CollectionExtension
    {
        public static T GetRandom<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static T GetRandom<T>(this List<T> list, Predicate<T> pred)
        {
            var matchedList = list.Where(new Func<T, bool>(pred)).ToList();
            return matchedList[UnityEngine.Random.Range(0, matchedList.Count)];
        }

        public static T Next<T>(this List<T> list, T value)
        {
            var idx = list.IndexOf(value);
            return list[(idx + 1) % list.Count];
        }


        //make tryget static extension
        public static bool TryGet<T>(this List<T> list, int index, out T value)
        {
            if (index < 0 || index >= list.Count)
            {
                value = default(T);
                return false;
            }
            value = list[index];
            return true;
        }
        
        public static void MoveTo<T>(this List<T> list, ref List<T> to)
        {
            to.AddRange(list);
            list.Clear();
        }

        //public static bool IsEmpty<T>(this List<T> list)
        //{
        //    return list.Count <= 0;
        //}

        public static bool IsEmpty<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary.Count <= 0;
        }

        public static void AddWhenNotContains<T>(this List<T> list, in T item)
        {
            if (list.Contains(item))
                return;

            list.Add(item);
        }

        public static bool IsEmpty(this ICollection collection)
        {
            return collection.Count <= 0;
        }

        public static TValue GetValueOrNull<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : class
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            else
            {
                return null;
            }
        }
    }
}