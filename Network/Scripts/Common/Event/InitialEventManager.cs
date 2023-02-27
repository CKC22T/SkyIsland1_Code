using UnityEngine;
using System;
using Utils;
using System.Collections.Generic;

[Obsolete("사용하지 않음")]
public class InitialEventManager : LocalSingleton<InitialEventManager>
{
    [Sirenix.OdinInspector.Button]
    public void SetupEventTriggers()
    {
        mEventTriggers.Clear();

        var childrens = transform.GetComponentsInChildren<BaseLocationEventTrigger>();

        foreach (var trigger in childrens)
        {
            mEventTriggers.Add(new TimerEvent(trigger));
        }
    }

    [SerializeField]
    private List<TimerEvent> mEventTriggers;

    public NetworkMode NetworkMode { get; private set; } = NetworkMode.None;

    public void ClearAsMaster()
    {
        mIsInitialize = false;

    }

    public void ClearAsRemote()
    {
        mIsInitialize = false;

    }

    private bool mIsInitialize = false;

    public void InitializeByManager(NetworkMode networkMode)
    {
        if (mIsInitialize)
        {
            return;
        }

        mIsInitialize = true;
        NetworkMode = networkMode;
    }
}
