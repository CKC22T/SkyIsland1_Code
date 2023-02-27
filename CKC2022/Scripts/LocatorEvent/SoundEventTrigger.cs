using CKC2022;
using UnityEngine;

public class SoundEventTrigger : BaseLocationEventTrigger
{
    public SoundType SoundType;
    public SoundPlayData SoundPlayData;
    public Transform SoundOrigin;

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (GameSoundManager.TryGetInstance(out var soundManager))
        {
            SoundPlayData.Position = SoundOrigin.position;
            GameSoundManager.Play(SoundType, SoundPlayData);
        }
    }
}