using System;
using UnityEngine;

public class BlockingWaveStartEventTrigger : BaseLocationEventTrigger
{
    [SerializeField] private BlockingWaveEventManager mWaveEventManager;

    public override void TriggeredEvent(BaseEntityData other)
    {
        mWaveEventManager.StartWave();
    }
}
