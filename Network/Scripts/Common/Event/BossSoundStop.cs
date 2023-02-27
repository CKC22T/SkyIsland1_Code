using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSoundStop : BaseLocationEventTrigger
{
    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ServerConfiguration.IS_CLIENT)
        {
            CKC2022.GameSoundManager.PlayBGM(CKC2022.SoundType.Game_Background);
        }
    }
}
