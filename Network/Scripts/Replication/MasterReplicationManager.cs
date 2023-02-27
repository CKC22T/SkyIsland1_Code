using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Network
{
    public interface IMasterNetworkalbe
    {
        public int MaxUdpPacketLength { get; set; }
        public int MaxTcpPacketLength { get; set; }
        public int UdpHeartbeatFrequency { get; set; } // Milliseconds
        public void AppendUdpPackets(ref List<NetBuffer> packets);
        public void AppendTcpPackets(ref List<NetBuffer> packets);
        public void AppendInitialDataPackets(ref List<NetBuffer> packets);
    }

    public class MasterReplicationManager : IMasterNetworkalbe
    {
        public int MaxUdpPacketLength { get; set; } = ServerConfiguration.MAX_UDP_PACKET_LENGTH;
        public int MaxTcpPacketLength { get; set; } = ServerConfiguration.MAX_TCP_PACKET_LENGTH;
        public int UdpHeartbeatFrequency { get; set; } = ServerConfiguration.UDP_HEARTBEAT_FREQUENCY;

        private const int TEMP_BUFFER_INITIAL_CAPACITY = 256;

        private Dictionary<int, MasterReplicationObject> mMasterReplicators = new();
        private int NewReplicatorID => mReplicatorCounter++;
        private int mReplicatorCounter = ServerConfiguration.REPLICATOR_INITIAL_COUNTER_INDEX_OFFSET;

        private DateTime mPreviousUdpHeartbeatSentTime;

        public MasterReplicationManager()
        {
            mPreviousUdpHeartbeatSentTime = DateTime.Now;
        }

        public MasterReplicationManager(IList<MasterReplicationObject> replicableList = null)
        {
            if (replicableList != null)
            {
                foreach (var r in replicableList)
                {
                    mMasterReplicators.Add(NewReplicatorID, r);
                }
            }
        }

        public MasterReplicationObject ForceCreateMasterReplicationObjectByID(int id)
        {
            if (id >= ServerConfiguration.REPLICATOR_INITIAL_COUNTER_INDEX_OFFSET)
            {
                Debug.Log($"You cannot create master replication object id more than {ServerConfiguration.REPLICATOR_INITIAL_COUNTER_INDEX_OFFSET}!");
            }

            var replicator = MasterReplicationObject.CreateAsMaster(id, onDisposed);
            mMasterReplicators.Add(id, replicator);
            return replicator;
        }

        public MasterReplicationObject CreateMasterReplicationObject()
        {
            int newID = NewReplicatorID;

            var replicator = MasterReplicationObject.CreateAsMaster(newID, onDisposed);
            mMasterReplicators.Add(newID, replicator);
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

        private void onDisposed(MasterReplicationObject disposedReplicator)
        {
            if (mMasterReplicators.ContainsKey(disposedReplicator.ID))
            {
                mMasterReplicators.Remove(disposedReplicator.ID);
            }
        }

        public void AppendUdpPackets(ref List<NetBuffer> packets)
        {
            //if (isTimeToSendUdpHeartbeat())
            //{
            //    NetBuffer tempHeartbeatBuffer = new NetBuffer(TEMP_BUFFER_INITIAL_CAPACITY);

            //    int tempCounterStartIndex = tempHeartbeatBuffer.AppendEmptySpace(sizeof(int));
            //    int heartbeatPacketCounter = 0;

            //    foreach (var r in mMasterReplicators.Values)
            //    {
            //        tempHeartbeatBuffer.Append(r.ID);
            //        r.AppendAllUnreliableDataToStream(ref tempHeartbeatBuffer);
            //        heartbeatPacketCounter++;
            //    }

            //    tempHeartbeatBuffer.ForceBindInt32(heartbeatPacketCounter, tempCounterStartIndex);

            //    if (heartbeatPacketCounter > 0)
            //    {
            //        packets.Add(tempHeartbeatBuffer);
            //    }
            //}

            NetBuffer packetBuffer = new NetBuffer(TEMP_BUFFER_INITIAL_CAPACITY);
            NetBuffer tempBuffer = new NetBuffer(TEMP_BUFFER_INITIAL_CAPACITY);

            int counterStartIndex = packetBuffer.AppendEmptySpace(sizeof(int));
            int packetCounter = 0;

            foreach (var r in mMasterReplicators.Values)
            {
                tempBuffer.Append(r.ID);
                r.AppendChangedUnreliableDataToStream(ref tempBuffer);

                if (tempBuffer.Size > 5)
                {
                    packetBuffer.Append(tempBuffer.Buffer, 0, tempBuffer.Size);
                    packetCounter++;
                }

                tempBuffer.Clear();
            }

            packetBuffer.ForceBindInt32(packetCounter, counterStartIndex);

            if (packetCounter > 0)
            {
                packets.Add(packetBuffer);
            }
        }

        public void AppendTcpPackets(ref List<NetBuffer> packets)
        {
            NetBuffer packetBuffer = new NetBuffer(TEMP_BUFFER_INITIAL_CAPACITY);
            NetBuffer tempBuffer = new NetBuffer(TEMP_BUFFER_INITIAL_CAPACITY);

            int counterStartIndex = packetBuffer.AppendEmptySpace(sizeof(int));
            int packetCounter = 0;

            foreach (var r in mMasterReplicators.Values)
            {
                tempBuffer.Append(r.ID);
                r.AppendChangedReliableDataToStream(ref tempBuffer);

                if (tempBuffer.Size > 5)
                {
                    packetBuffer.Append(tempBuffer.Buffer, 0, tempBuffer.Size);
                    packetCounter++;
                }

                tempBuffer.Clear();
            }

            packetBuffer.ForceBindInt32(packetCounter, counterStartIndex);

            if (packetCounter > 0)
            {
                packets.Add(packetBuffer);
            }
        }

        public void AppendInitialDataPackets(ref List<NetBuffer> packets)
        {
            NetBuffer packetBuffer = new NetBuffer(TEMP_BUFFER_INITIAL_CAPACITY);
            int counterStartIndex = packetBuffer.AppendEmptySpace(sizeof(int));
            int packetCounter = 0;

            foreach (var r in mMasterReplicators.Values)
            {
                packetBuffer.Append(r.ID);
                r.AppendAllDataToStream(ref packetBuffer);
                packetCounter++;
            }

            packetBuffer.ForceBindInt32(packetCounter, counterStartIndex);

            if (packetCounter > 0)
            {
                packets.Add(packetBuffer);
            }
        }

        private bool isTimeToSendUdpHeartbeat()
        {
            TimeSpan elapse = DateTime.Now - mPreviousUdpHeartbeatSentTime;

            if (elapse.Milliseconds > UdpHeartbeatFrequency)
            {
                mPreviousUdpHeartbeatSentTime = DateTime.Now;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var obj in mMasterReplicators.Values)
            {
                sb.Append(obj);
            }

            return sb.ToString();
        }
    }
}
