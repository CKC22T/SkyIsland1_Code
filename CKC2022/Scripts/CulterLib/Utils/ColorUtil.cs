using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Utils
{
    public static class ColorUtil
    {
        /// <summary>
        /// Color에서 Alpha값만 특정 값으로 만들어서 리턴합니다.
        /// </summary>
        /// <param name="_color"></param>
        /// <param name="_a"></param>
        /// <returns></returns>
        public static Color GetAChange(Color _color, float _a)
        {
            _color.a = _a;
            return _color;
        }
    }
}