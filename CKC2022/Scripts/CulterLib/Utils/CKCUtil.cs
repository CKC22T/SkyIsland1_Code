using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public static class CKCUtil
{
    #region Notifier
    /// <summary>
    /// OnDataChanged + 해당 함수 즉시 호출을 해주는 확장함수
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_noti"></param>
    /// <param name="_onDataChanged"></param>
    /// <param name="_isCallNow"></param>
    public static void AddDataChanged<T>(this INotifiable<T> _noti, Action<T> _onDataChanged, bool _isCallNow = true)
    {
        _noti.OnDataChanged += _onDataChanged;
        if (_isCallNow)
            _onDataChanged(_noti.Value);
    }
    /// <summary>
    /// OnDataChanged + 해당 함수 즉시 호출을 해주는 확장함수
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_noti"></param>
    /// <param name="_onDataChanged"></param>
    /// <param name="_isCallNow"></param>
    public static void AddUpdateNotify<T>(this INotifiable<T> _noti, Action<T> _onDataChanged, bool _isCallNow = true)
    {
        _noti.OnUpdateNotify += _onDataChanged;
        if (_isCallNow)
            _onDataChanged(_noti.Value);
    }
    #endregion
}
