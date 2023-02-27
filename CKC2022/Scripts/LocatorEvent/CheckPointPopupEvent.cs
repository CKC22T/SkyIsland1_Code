using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointPopupEvent : BaseLocationEventTrigger
{
    public int CheckPointNumber;

    public override void TriggeredEvent(BaseEntityData other)
    {
        string areaName = GlobalAreaName.GetAreaName(CheckPointNumber);

        CheckPointPopup.Instance.Open(areaName);
    }
}
