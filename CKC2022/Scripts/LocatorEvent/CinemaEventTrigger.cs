using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class CinemaEventTrigger : BaseLocationEventTrigger
{
    public CinemaType PlayCinemaType = CinemaType.None;

    public override void TriggeredEvent(BaseEntityData other)
    {
        if (!CinemaManager.TryGetInstance(out var cinemaManager))
        {
            return;
        }

        if (ServerConfiguration.IS_CLIENT)
        {
            cinemaManager.ForcePlayCinemaOnRemote(PlayCinemaType);
        }

        if (ServerConfiguration.IS_SERVER)
        {
            cinemaManager.PlayCinemaOnMaster(PlayCinemaType);
        }
    }
}
