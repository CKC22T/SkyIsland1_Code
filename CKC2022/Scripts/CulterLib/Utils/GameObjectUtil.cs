using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Utils
{
    public static class GameObjectUtil
    {
        /// <summary>
        /// 게임오브젝트를 Vector3.zero위치와 Quaternion.identity각도에 Vector3.one 크기로 생성합니다.
        /// </summary>
        /// <param name="_prefab"></param>
        /// <param name="_root"></param>
        /// <returns></returns>
        public static GameObject Instantiate(GameObject _prefab, Transform _root)
        {
            return Instantiate(_prefab, _root, Vector3.zero, Quaternion.identity, Vector3.one);
        }
        /// <summary>
        /// 게임오브젝트를 생성합니다.
        /// Instantiate가 맨날 이상하게 작동하거나 코드 길어지거나 해서 만들었음
        /// </summary>
        /// <param name="_prefab"></param>
        /// <param name="root"></param>
        /// <param name="localPos"></param>
        /// <param name="localRot"></param>
        /// <param name="localScale"></param>
        /// <returns></returns>
        public static GameObject Instantiate(GameObject _prefab, Transform _root, Vector3 _localPos, Quaternion _localRot, Vector3 _localScale)
        {
            if (_prefab != null)
            {
                GameObject go = GameObject.Instantiate(_prefab);

                if (go)
                {
                    go.transform.SetParent(_root);
                    go.transform.localPosition = _localPos;
                    go.transform.localRotation = _localRot;
                    go.transform.localScale = _localScale;
                }

                return go;
            }
            else
                return null;
        }
    }
}