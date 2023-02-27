using System;
using System.Collections;
using System.Collections.Generic;
using CulterLib.Types;
using UnityEngine;

namespace CulterLib.Utils
{
    public static class TypeUtil
    {
        #region Type
        /// <summary>
        /// 타입 (System.Type)의 종류를 저장하는 enum입니다.
        /// </summary>
        public enum ETypeKind
        {
            Value,      //int, float, string 등의 기본 타입
            Enum,       //enum
            Struct,     //struct
            Class,      //class
            Interface,  //interface
        }
        #endregion

        #region Function
        /// <summary>
        /// 해당 타입의 종류를 가져옵니다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ETypeKind GetTypeKind(Type type)
        {
            if (type.IsPrimitive || type == typeof(string))
                return ETypeKind.Value;
            else if (type.IsEnum)
                return ETypeKind.Enum;
            else if (type.IsInterface)
                return ETypeKind.Interface;
            else if (type.IsClass)
                return ETypeKind.Class;
            else
                return ETypeKind.Struct;
        }

        //Public - Type SaveLoad
        /// <summary>
        /// 한 개의 변수의 세이브용 텍스트를 가져옵니다. (ObsrvList, ObsrvDictionary 등에서 사용)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_data">세이브할 변수</param>
        /// <returns>세이브된 텍스트</returns>
        public static string GetSave<T>(T _data)
        {
            try
            {
                ETypeKind typeKind = GetTypeKind(typeof(T));

                //타입 상관없이 텍스트 형태로 정리
                if (_data == null)
                {   //null
                    return null;
                }
                else if (typeKind == ETypeKind.Value)
                {   //일반변수 (int, bool 등)
                    return _data.ToString();
                }
                else if (typeKind == ETypeKind.Enum)
                {   //Enum
                    int index = (int)Convert.ChangeType(_data, typeof(int));
                    return index.ToString();
                }
                else if (_data as IConvertible != null)
                {   //C# 기본 Convert 
                    return Convert.ToString(_data);
                }
                else if (typeKind == ETypeKind.Struct || typeKind == ETypeKind.Class)
                {   //구조체, 클래스
                    if (typeof(T) == typeof(Dictionary<string, object>) || typeof(T) == typeof(List<object>))
                        return MiniJSON.Json.Serialize(_data);
                    else
                        return JsonUtility.ToJson(_data);
                }

                Debug.LogError($"Obsrv.GetSave Failed");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Obsrv.GetSave Error : {e.Message}");
                return null;
            }
        }
        /// <summary>
        /// 한 개의 변수를 로드해옵니다. (ObsrvList, ObsrvDictionary 등에서 사용)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_data">로드할 텍스트</param>
        /// <param name="defaultData">기본값</param>
        /// <returns>로드된 변수</returns>
        public static bool GetLoad<T>(string _data, out T _result)
        {
            try
            {
                Type t = typeof(T);
                ETypeKind typeKind = GetTypeKind(t);

                //각각 파싱해서 사용
                if (typeKind == ETypeKind.Value)
                {   //일반변수 (int, bool 등)
                    _result = (T)Convert.ChangeType(_data, t);
                    return true;
                }
                else if (typeKind == ETypeKind.Enum)
                {   //Enum
                    if (int.TryParse(_data, out int index))
                    {
                        _result = (T)(object)index;
                        return true;
                    }
                }
                else if (typeof(IConvertible).IsAssignableFrom(t))
                {   //C# 기본 Convert 지원시
                    _result = (T)Convert.ChangeType(_data, t);
                    return true;
                }
                else if (typeKind == ETypeKind.Struct || typeKind == ETypeKind.Class)
                {   //구조체, 클래스
                    if (t == typeof(Dictionary<string, object>) || t == typeof(List<object>))
                        _result = (T)MiniJSON.Json.Deserialize(_data);
                    else
                        _result = JsonUtility.FromJson<T>(_data);
                    return true;
                }

                Debug.LogError($"Obsrv.GetLoad Failed");
                _result = default;
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Obsrv.GetLoad Error : {e.Message}");
                _result = default;
                return false;
            }
        }
#if CULTERLIB_THEBACKEND
        /// <summary>
        /// TheBackend 전용 / 한개의 변수를 로드해옵니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_data"></param>
        /// <param name="_defaultDat"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetLoad_TheBackend<T>(LitJson.JsonData _data, out T _result)
        {
            _result = default;  //기본값 설정

            //임시변수
            Type t = typeof(T);
            ETypeKind typeKind = GetTypeKind(t);
            string key = "";

            //실제 로드 처리
            if (typeKind == ETypeKind.Value || typeKind == ETypeKind.Enum)
            {
                if (t == typeof(bool))
                    key = "BOOL";   //bool인 경우 "BOOL"을 키로 로드
                else if (t == typeof(string))
                    key = "S";      //string인 경우 "S"를 키로 로드
                else
                    key = "N";      //기타 일반변수인 경우 (int, float, double...) "N"을 키로 로드
            }
            else
                key = "S";          //일반변수나 enum이 아닌 경우 string으로 통일

            if (_data.Keys.Contains(key))
                return GetLoad(_data[key].ToString(), out _result);
            else
                return false;
        }
#endif
        #endregion
    }
}