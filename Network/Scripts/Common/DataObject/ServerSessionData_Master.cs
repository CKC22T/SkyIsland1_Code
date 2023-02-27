using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Common
{
    public class ServerSessionData_Master : MasterNetObject
    {
        // Reliable Data
        [Sirenix.OdinInspector.ShowInInspector] public readonly NetStringData CurrentScene = new("Some Initial Scene Name");
        [Sirenix.OdinInspector.ShowInInspector] public readonly NetBooleanData IsServerLoaded = new(false);

        public override void InitializeData(in MasterReplicationObject assignee)
        {
            assignee.AssignDataAsReliable(CurrentScene);
            assignee.AssignDataAsReliable(IsServerLoaded);
        }
    }
}
