using System;
using System.Collections;
using UnityEngine;

public class LocalEntityData : BaseEntityData
{
    public event Action<ReplicatedDetectedInfo> OnHitAction;

    private void OnEnable()
    {
        IsEnabled.Value = true;
        IsAlive.Value = true;
    }

    public void RaiseHitInfo(ReplicatedDetectedInfo info)
    {
        OnHitAction?.Invoke(info);
    }
}