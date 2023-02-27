using Network.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeOffEvent : BaseLocationEventTrigger
{
    public List<TimerEvent> Bridges;

    public override void TriggeredEvent(BaseEntityData other)
    {
        foreach (var bridge in Bridges)
        {
            StartCoroutine(bridgeOff(bridge));
        }

        IEnumerator bridgeOff(TimerEvent timerEvent)
        {
            yield return new WaitForSeconds(timerEvent.Timer);
            var bridge = timerEvent.EventTrigger as BridgeEvent;
            bridge.BridgeOff();
        }
    }
}
