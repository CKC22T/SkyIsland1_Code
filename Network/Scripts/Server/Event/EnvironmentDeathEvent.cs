using Network.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentDeathEvent : BaseLocationEventTrigger
{
    public override void TriggeredEvent(BaseEntityData other)
    {
        var masterEntityData = other as MasterEntityData;

        if (masterEntityData == null)
            return;

        masterEntityData.ActionDie();
    }
}
