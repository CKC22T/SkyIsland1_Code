using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Network
{
    public interface IRemoteNetworkalbe
    {
        public void ReadFromBuffer(ref NetBuffer buffer);
    }

    public class RemoteReplicationManager : IRemoteNetworkalbe
    {
        private const int TEMP_BUFFER_INITIAL_CAPACITY = 256;

        private Dictionary<int, RemoteReplicationObject> mRemoteReplicators = new();
        //private int NewReplicatorID => mReplicatorCounter++;
        //private int mReplicatorCounter = ServerConfiguration.REPLICATOR_INITIAL_COUNTER_INDEX_OFFSET;

        public RemoteReplicationObject ForceCreateRemoteReplicationObject(int id)
        {
            if (mRemoteReplicators.ContainsKey(id))
            {
                Debug.LogError($"There is already exist remote replication object id : {id}");
                return null;
            }

            var replicator = RemoteReplicationObject.CreateAsRemoted(id, onDisposed);
            mRemoteReplicators.Add(id, replicator);
            return replicator;
        }

        //public void AddReplicationObject(IReplicableObject replicationObject)
        //{
        //    if (mReplicators.ContainsKey(replicationObject.ID) == false)
        //    {
        //        throw new InvalidOperationException(
        //            LogManager.GetLogMessage($"Current replication object [{replicationObject.ID}] is already exists!\n" +
        //            $"AddReplicationObject failed!"));
        //    }

        //    replicationObject.OnDisposed += onDisposed;
        //    mReplicators.Add(replicationObject.ID, replicationObject);
        //}

        private void onDisposed(RemoteReplicationObject disposedReplicator)
        {
            if (mRemoteReplicators.ContainsKey(disposedReplicator.ID))
            {
                mRemoteReplicators.Remove(disposedReplicator.ID);
            }
        }

        public void ReadFromBuffer(ref NetBuffer buffer)
        {
            int objectCount = buffer.ReadInt32();

            for (int i = 0; i < objectCount; i++)
            {
                int objectID = buffer.ReadInt32();

                if (mRemoteReplicators.ContainsKey(objectID))
                {
                    mRemoteReplicators[objectID].ReadFromBuffer(ref buffer);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var obj in mRemoteReplicators.Values)
            {
                sb.Append(obj);
            }

            return sb.ToString();
        }
    }
}
