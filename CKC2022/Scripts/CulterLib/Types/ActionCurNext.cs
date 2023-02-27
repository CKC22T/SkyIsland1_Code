using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CulterLib.Types
{
    public class ActionCurNext1<T>
    {
        #region Get,Set
        /// <summary>
        /// true일 경우 Next에 들어감
        /// </summary>
        public bool IsLock { get; set; }
        #endregion
        #region Value
        private Action<T> m_Cur;
        private Action<T> m_Next;
        #endregion

        #region Function
        /// <summary>
        /// 액션을 추가합니다.
        /// </summary>
        /// <param name="_add"></param>
        public void Add(Action<T> _add)
        {
            if (IsLock)
                m_Cur += _add;
            else
                m_Next += _add;
        }
        /// <summary>
        /// 액션을 제거합니다.
        /// </summary>
        /// <param name="_remove"></param>
        public void Remove(Action<T> _remove)
        {
            if (IsLock)
                m_Cur -= _remove;
            else
                m_Next -= _remove;
        }
        /// <summary>
        /// 함수 호출, Cur/Next 교체, Unlock
        /// </summary>
        /// <param name="_value"></param>
        public void Invoke(T _value)
        {
            m_Cur?.Invoke(_value);
            m_Cur = m_Next;
            m_Next = null;
            IsLock = false;
        }
        #endregion
    }
}