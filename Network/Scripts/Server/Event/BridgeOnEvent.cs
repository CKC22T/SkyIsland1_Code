using Network.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeOnEvent : BaseLocationEventTrigger
{
    public List<TimerEvent> Bridges;


    public override void TriggeredEvent(BaseEntityData other)
    {
        foreach (var bridge in Bridges)
        {
            StartCoroutine(bridgeOn(bridge));
        }

        IEnumerator bridgeOn(TimerEvent timerEvent)
        {
            yield return new WaitForSeconds(timerEvent.Timer);
            var bridge = timerEvent.EventTrigger as BridgeEvent;
            bridge.BridgeOn();
        }
    }
}
