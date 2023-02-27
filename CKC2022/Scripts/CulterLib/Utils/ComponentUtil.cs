using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Utils
{
    public static class ComponentUtil
    {
        /// <summary>
        /// GetComponent를 합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_target"></param>
        /// <param name="_component"></param>
        /// <returns></returns>
        public static bool GetComp<T>(Component _target, out T _component) where T : Component
        {
            if (_target != null)
            {
                _component = _target.GetComponent<T>();
                return _component;
            }
            else
            {
                _component = null;
                return false;
            }
        }
        /// <summary>
        /// GetComponentInParent를 합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_target"></param>
        /// <param name="_component"></param>
        /// <returns></returns>
        public static bool GetCompInPar<T>(Component _target, out T _component) where T : Component
        {
            if (_target != null)
            {
                _component = _target.GetComponentInParent<T>();
                return _component;
            }
            else
            {
                _component = null;
                return false;
            }
        }
        /// <summary>
        /// GetComponentInChildren를 합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_target"></param>
        /// <param name="_component"></param>
        /// <returns></returns>
        public static bool GetCompInChild<T>(Component _target, out T _component) where T : Component
        {
            if (_target != null)
            {
                _component = _target.GetComponentInChildren<T>();
                return _component;
            }
            else
            {
                _component = null;
                return false;
            }
        }
    }
}