using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Common
{
    public class ServerSessionData_Remote : RemoteNetObject
    {
        // Reliable Data
        [Sirenix.OdinInspector.ShowInInspector] public readonly NetStringData CurrentScene = new();
        [Sirenix.OdinInspector.ShowInInspector] public readonly NetBooleanData IsServerLoaded = new();

        public override void InitializeData(in RemoteReplicationObject assignee)
        {
            assignee.AssignDataAsReliable(CurrentScene);
            assignee.AssignDataAsReliable(IsServerLoaded);
        }
    }
}
