using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryCreditSetEvent : BaseLocationEventTrigger
{
    public override void TriggeredEvent(BaseEntityData other)
    {
        GlobalNetworkCache.SetOnVictoryCredit(true);
    }
}
