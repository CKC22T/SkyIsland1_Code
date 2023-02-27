using System;
using UnityEngine;

public class BlockingWaveCreateBlockingStoneEventTrigger : BaseLocationEventTrigger
{
    [SerializeField] private BlockingWaveEventManager mWaveEventManager;

    public override void TriggeredEvent(BaseEntityData other)
    {
        mWaveEventManager.CreateBlockingStone();
    }
}
