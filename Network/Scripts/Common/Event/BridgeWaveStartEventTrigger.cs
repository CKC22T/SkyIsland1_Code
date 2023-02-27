using UnityEngine;

class BridgeWaveStartEventTrigger : BaseLocationEventTrigger
{
    [SerializeField] private BridgeWaveEventManager mBridgeWaveEventManager;

    public override void TriggeredEvent(BaseEntityData other)
    {
        mBridgeWaveEventManager.StartWave();

    }
}
