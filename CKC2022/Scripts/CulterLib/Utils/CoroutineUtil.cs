using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Utils
{
    public static class CoroutineUtil
    {
        /// <summary>
        /// 코루틴을 멈추고, null을 리턴합니다.
        /// </summary>
        /// <param name="_mono"></param>
        /// <param name="_coroutine"></param>
        public static Coroutine Stop(MonoBehaviour _mono, Coroutine _stop)
        {
            if (_stop != null)
                _mono.StopCoroutine(_stop);

            return null;
        }
        /// <summary>
        /// 코루틴을 멈추고, 다음 코루틴을 실행한 뒤 리턴합니다.
        /// </summary>
        /// <param name="_mono"></param>
        /// <param name="_stop"></param>
        /// <param name="_start"></param>
        /// <returns></returns>
        public static Coroutine Change(MonoBehaviour _mono, Coroutine _stop, IEnumerator _start)
        {
            if (_stop != null)
                _mono.StopCoroutine(_stop);

            if (_start != null)
                return _mono.StartCoroutine(_start);
            else
                return null;
        }
    }
}