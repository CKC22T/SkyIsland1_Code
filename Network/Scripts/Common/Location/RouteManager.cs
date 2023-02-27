using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class RouteManager : LocalSingleton<RouteManager>
{
    #region Inspector
    [TabGroup("Component"), SerializeField] private Transform mStageFinish;
    #endregion
    #region Get,Set
    /// <summary>
    /// 현재 스테이지의 도착지점
    /// </summary>
    public Transform StageFinish { get => mStageFinish; }
    #endregion
}
