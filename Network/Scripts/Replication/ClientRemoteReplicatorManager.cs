using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

public class ClientRemoteReplicatorManager : LocalSingleton<ClientRemoteReplicatorManager>
{
    //[SerializeField] public List<RemoteNetObject> PresetNetObjectList;
    [SerializeField] public List<Tuple<int, RemoteNetObject>> NetObjectPresetByID;

    private RemoteReplicationManager mReplicatorManager;

    public void Start()
    {
        mReplicatorManager = new RemoteReplicationManager();

        foreach (var preset in NetObjectPresetByID)
        {
            preset.Item2.BindViaRemoteManager(mReplicatorManager.ForceCreateRemoteReplicationObject(preset.Item1));
        }
    }

    public void ReadFromBuffer(ref NetBuffer buffer)
    {
        while (buffer.CanReadBytesByLength())
        {
            mReplicatorManager.ReadFromBuffer(ref buffer);
        }
    }
}