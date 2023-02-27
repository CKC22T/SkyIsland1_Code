using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Utils
{
    public static class Vector3Util
    {
        /// <summary>
        /// Vector3에서 x값만 특정 값으로 만들어서 리턴합니다.
        /// </summary>
        /// <param name="_vec"></param>
        /// <param name="_x"></param>
        /// <returns></returns>
        public static Vector3 ChangeX(this Vector3 _vec, float _x)
        {
            _vec.x = _x;
            return _vec;
        }
        /// <summary>
        /// Vector3에서 y값만 특정 값으로 만들어서 리턴합니다.
        /// </summary>
        /// <param name="_vec"></param>
        /// <param name="_x"></param>
        /// <returns></returns>
        public static Vector3 ChangeY(this Vector3 _vec, float _y)
        {
            _vec.y = _y;
            return _vec;
        }
        /// <summary>
        /// Vector3에서 z값만 특정 값으로 만들어서 리턴합니다.
        /// </summary>
        /// <param name="_vec"></param>
        /// <param name="_z"></param>
        /// <returns></returns>
        public static Vector3 ChangeZ(this Vector3 _vec, float _z)
        {
            _vec.z = _z;
            return _vec;
        }
        /// <summary>
        /// Vector3에서 z값만 0으로 만들어서 리턴합니다.
        /// </summary>
        /// <param name="_vec"></param>
        /// <returns></returns>
        public static Vector3 GetZZero(Vector3 _vec) => ChangeZ(_vec, 0);

        /// <summary>
        /// a와 b사이의 랜덤한 벡터값을 가져옵니다.
        /// </summary>
        /// <param name="_a"></param>
        /// <param name="_b"></param>
        /// <returns></returns>
        public static Vector3 GetRandom(Vector3 _a, Vector3 _b) => new Vector3(Random.Range(_a.x, _b.x), Random.Range(_a.y, _b.y), Random.Range(_a.z, _b.z));
    }
}
