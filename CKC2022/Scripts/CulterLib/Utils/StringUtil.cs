using CulterLib.Presets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Utils
{
    public static class StringUtil
    {
        #region Function
        /// <summary>
        /// 해당 초를 n시간 n분 n초 형태로 변경해서 가져옵니다.
        /// </summary>
        /// <param name="_sec"></param>
        /// <returns></returns>
        public static string GetTimeText(int _sec)
        {
            string time = string.Format(GlobalManager.Instance.DataMgr.GetTextTableData("Text_Common_Sec").GetText(), _sec % 60);
            if (0 < _sec / 60)
                time = $"{string.Format(GlobalManager.Instance.DataMgr.GetTextTableData("Text_Common_Min").GetText(), (_sec / 60) % 60)} {time}";
            if (0 < _sec / 3600)
                time = $"{string.Format(GlobalManager.Instance.DataMgr.GetTextTableData("Text_Common_Hour").GetText(), _sec / 3600)} {time}";
            return time;
        }

        /// <summary>
        /// 인덱스 구분용으로 붙어있는 ID를 int로 만들어서 가져옵니다.
        /// 예 : Quest_AdsAchieve_1 => 1
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static int GetIndexID(string _id)
        {
            var split = _id.Split('_');
            if (int.TryParse(split[split.Length - 1], out var index))
                return index;
            else
                return 0;
        }
        /// <summary>
        /// 시스템 구분용으로 붙어있는 ID를 제거한 ID를 가져옵니다.
        /// (맨앞이 소문자인 ID부분들을 제거합니다.)
        /// 예 : plyr_statis_Statis_Quest_ClearCnt => Statis_Quest_ClearCnt
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static string RemoveSystemID(string _id)
        {
            var split = _id.Split('_');
            for (int i = 0; i < split.Length; ++i)
                if (char.IsUpper(split[i][0]))
                    return string.Join("_", split, i, split.Length - i);

            for (int i = 0; i < _id.Length; ++i)
                if (_id[i] == '_')
                    return _id.Substring(i + 1);

            return _id;
        }
        /// <summary>
        /// 인덱스 구분용으로 붙어있는 ID를 제거한 ID를 가져옵니다.
        /// (맨뒤가 숫자인 경우 해당 부분을 제거합니다.
        /// 예 : Quest_AdsAchieve_1 => Quest_AdsAchieve
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static string RemoveIndexID(string _id)
        {
            var split = _id.Split('_');
            if (int.TryParse(split[split.Length - 1], out var index))
                return string.Join("_", split, 0, split.Length - 1);
            else
                return _id;
        }
        #endregion
    }
}