using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CulterLib.Utils
{
    public static class UITextUtil
    {
        /// <summary>
        /// 텍스트의 사이즈를 가져옵니다.
        /// </summary>
        /// <param name="_text"></param>
        /// <returns></returns>
        public static Vector2 GetSize(Text _text, bool _isHoriOver = true, bool _isVertiOver = true)
        {
            //텍스트 오버플로우 기본값 저장 및 설정
            var horiOri = _text.horizontalOverflow;
            var vertiOri = _text.verticalOverflow;
            if (_isHoriOver)
                _text.horizontalOverflow = HorizontalWrapMode.Overflow;
            if (_isVertiOver)
                _text.verticalOverflow = VerticalWrapMode.Overflow;

            //사이즈 계산
            Vector2 size = new Vector2(_text.preferredWidth, _text.preferredHeight);

            //복구
            _text.horizontalOverflow = horiOri;
            _text.verticalOverflow = vertiOri;

            return size;
        }
    }
}