using Network;
using Network.Packet;
using Network.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UserSessionData_Master : MasterNetObject
{
    [Sirenix.OdinInspector.ShowInInspector]
    public SessionSlotCollection SessionSlots = new();

    public override void InitializeData(in MasterReplicationObject assignee)
    {
        SessionSlots.InitializeDataAsMaster(assignee);
    }

    #region Connection Handling

    public void Connected(int sessionID)
    {
        SessionSlots.OnConnected(sessionID);
    }

    public void Disconnected(int sessionID)
    {
        SessionSlots.OnDisconnected(sessionID);
    }

    #endregion

    #region Handle


    #endregion
}
