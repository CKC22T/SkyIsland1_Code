using UnityEngine;
using System;
using Network;

public class CinemaRemoteCallEventTrigger : BaseLocationEventTrigger
{
    public CinemaType PlayCinemaType = CinemaType.None;

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (ServerConfiguration.IS_SERVER)
        {
            if (CinemaManager.TryGetInstance(out var cinemaManager))
            {
                cinemaManager.RemotePlayCinema(PlayCinemaType);
            }
        }
    }
}
