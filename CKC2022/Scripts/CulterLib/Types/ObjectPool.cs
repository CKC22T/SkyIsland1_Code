using System;
using System.Collections;
using System.Collections.Generic;
using CulterLib.Utils;
using UnityEngine;

using static UnityEngine.Object;

namespace CulterLib.Types
{
    public class UIObjectPool<T> where T : MonoBehaviour
    {
        #region Get,Set
        /// <summary>
        /// 오브젝트 풀의 크기
        /// </summary>
        public int Count { get => m_Pool.Length; }
        /// <summary>
        /// 오브젝트 풀 자체 (왠만하면 사용ㄴㄴ)
        /// </summary>
        public T[] AllObj { get => m_Pool; }
        /// <summary>
        /// 현재 켜져있는 오브젝트의 갯수
        /// </summary>
        public int ActCnt
        {
            get
            {
                int cnt = 0;
                foreach (var v in m_Pool)
                    if (m_IsActFunc(v))
                        ++cnt;
                return cnt;
            }
        }
        /// <summary>
        /// 현재 켜져있는 오브젝트들
        /// </summary>
        public T[] ActObj
        {
            get
            {
                List<T> actObjs = new List<T>();
                foreach (var v in m_Pool)
                    if (m_IsActFunc(v))
                        actObjs.Add(v);

                return actObjs.ToArray();
            }
        }
        #endregion
        #region Value
        private GameObject m_Prefab;            //풀링할 오브젝트 프리팹
        private Transform m_Root;               //오브젝트 생성할곳
        public Action<T> m_InitFunc;            //오브젝트 생성함수
        public Action<T, bool> m_SetActFunc;    //오브젝트 켜기/끄기 설정 함수
        public Func<T, bool> m_IsActFunc;       //오브젝트 켜기/끄기 상태 확인 함수
        private T[] m_Pool = null;              //오브젝트 풀
        #endregion

        #region Event
        /// <summary>
        /// 프리팹으로 오브젝트 풀을 생성합니다.
        /// </summary>
        /// <param name="_prefab">사용할 프리팹</param>
        /// <param name="_size">오브젝트 풀의 최대 크기</param>
        /// <param name="_isPreGen">오브젝트 미리 생성 여부</param>
        /// <param name="_initFunc">오브젝트 초기화 함수 (오브젝트, true:사용시/false:재활용시)</param>
        /// <param name="_objectUsingFunc">오브젝트 사용중 판별 함수 (없을 시 ActiveSelf로 판별함)</param>
        public UIObjectPool(GameObject _prefab, Transform _root, int _size, bool _isPreGen, Action<T> _initFunc = null, Action<T, bool> _setActFunc = null, Func<T, bool> _isActFunc = null)
        {
            //변수들 대입
            m_Prefab = _prefab;
            m_Root = _root;
            m_Pool = new T[_size];
            m_InitFunc = _initFunc;
            m_SetActFunc = (_setActFunc != null) ? _setActFunc : DefaultSetActFunc;
            m_IsActFunc = (_isActFunc != null) ? _isActFunc : DefaultIsActFunc;

            //PreGen 옵션 실행
            if (_isPreGen)
            {
                for (int i = 0; i < m_Pool.Length; ++i)
                {
                    m_Pool[i] = Instantiate(m_Prefab, m_Root).GetComponent<T>();
                    m_InitFunc?.Invoke(m_Pool[i]);
                    m_SetActFunc(m_Pool[i], false);
                }
            }
        }
        /// <summary>
        /// 기존에 생성된 오브젝트로 오브젝트풀을 생성합니다.
        /// Original Object가 변경된 이후에 복제되도 상관 없는 경우에만 사용할것!
        /// </summary>
        /// <param name="_prefab">사용할 프리팹</param>
        /// <param name="_size">오브젝트 풀의 최대 크기</param>
        /// <param name="_isPreGen">오브젝트 미리 생성 여부</param>
        /// <param name="_initFunc">오브젝트 초기화 함수 (오브젝트, true:사용시/false:재활용시)</param>
        /// <param name="_objectUsingFunc">오브젝트 사용중 판별 함수 (없을 시 ActiveSelf로 판별함)</param>
        public UIObjectPool(T[] _original, int _size, bool _isPreGen, Action<T> _initFunc = null, Action<T, bool> _setActFunc = null, Func<T, bool> _isActFunc = null)
        {
            //변수들 대입
            m_Prefab = _original[0].gameObject;
            m_Root = _original[0].transform.parent;
            m_Pool = new T[_size];
            m_InitFunc = _initFunc;
            m_SetActFunc = (_setActFunc != null) ? _setActFunc : DefaultSetActFunc;
            m_IsActFunc = (_isActFunc != null) ? _isActFunc : DefaultIsActFunc;

            //오브젝트 넣기/생성
            int count = Mathf.Min(_original.Length, m_Pool.Length);
            for (int i = 0; i < count; ++i)
                m_Pool[i] = _original[i];
            for (int i = count; i < _original.Length; ++i)
                _original[i].gameObject.SetActive(false);
            if (_isPreGen)
                for (int i = count; i < m_Pool.Length; ++i)
                    m_Pool[i] = Instantiate(m_Prefab, m_Root).GetComponent<T>();

            //오브젝트 초기화
            for (int i = 0; i < m_Pool.Length; ++i)
            {
                if (m_Pool[i])
                {
                    _initFunc?.Invoke(m_Pool[i]);
                    m_SetActFunc(m_Pool[i], false);
                }
                else
                    break;
            }
        }
        #endregion
        #region Function
        //Public
        /// <summary>
        /// 재활용 가능한 상태의 오브젝트를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public T GetObject()
        {
            for (int i = 0; i < m_Pool.Length; ++i)
            {
                //오브젝트 사용중인 경우 지나감
                if (m_Pool[i] && m_IsActFunc(m_Pool[i]))
                    continue;

                //풀이 비어있으면 새로 생성
                if (m_Pool[i] == null)
                {
                    m_Pool[i] = Instantiate(m_Prefab, m_Root).GetComponent<T>();
                    m_InitFunc?.Invoke(m_Pool[i]);
                }

                //사용중으로 설정하고 리턴
                m_SetActFunc(m_Pool[i], true);
                return m_Pool[i];
            }

            //사용 가능한게 없으면...
            return null;
        }
        /// <summary>
        /// 재활용 가능한 상태의 오브젝트들을 해당 갯수만큼 가져옵니다.
        /// 해당 갯수만큼 가져올 수 없으면 가져올 수 있는 만큼만 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public T[] GetObjects(int _count)
        {
            //오브젝트 가져오기
            List<T> objects = new List<T>();
            for (int i = 0; i < m_Pool.Length && objects.Count < _count; ++i)
            {
                if(m_Pool[i] == null)
                {
                    m_Pool[i] = Instantiate(m_Prefab, m_Root).GetComponent<T>();
                    m_InitFunc?.Invoke(m_Pool[i]);
                    objects.Add(m_Pool[i]);
                }
                else
                {
                    if (!m_IsActFunc(m_Pool[i]))
                    {
                        m_SetActFunc(m_Pool[i], true);
                        objects.Add(m_Pool[i]);
                    }
                }
            }

            //리턴
            return objects.ToArray();
        }
        /// <summary>
        /// 전부 재활용 가능한 상태로 청소합니다.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < m_Pool.Length; ++i)
                if (m_Pool[i])
                    m_SetActFunc(m_Pool[i], false);
        }

        //Public Static - Default Functions
        /// <summary>
        /// 기본 오브젝트 생성함수
        /// </summary>
        /// <param name="_prefab"></param>
        /// <returns></returns>
        public static T DefaultSpawnFunc(GameObject _prefab, Transform _root)
        {
            return Instantiate(_prefab, _root).GetComponent<T>();
        }
        /// <summary>
        /// 기본 오브젝트 켜기/끄기 설정 함수
        /// </summary>
        /// <param name="_obj"></param>
        /// <param name="_isAct"></param>
        public static void DefaultSetActFunc(T _obj, bool _isAct)
        {
            _obj.gameObject.SetActive(_isAct);
        }
        /// <summary>
        /// 기본 오브젝트 켜기/끄기 상태 확인 함수
        /// </summary>
        /// <param name="_obj"></param>
        /// <returns></returns>
        public static bool DefaultIsActFunc(T _obj)
        {
            return _obj.gameObject.activeSelf;
        }
        #endregion
    }
}