using Network;
using Network.Packet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Network
{
    public class BaseReplicationObject
    {
        public ObjectType BaseObjectType { get; protected set; }
        public int ID { get; protected set; }
        public int ReliableDataMaxMemorySize { get; protected set; }
        public int UnreliableDataMaxMemorySize { get; protected set; }
        public int Count => mReliableObjects.Count + mUnreliableObjects.Count;
        public bool IsFull => (mReliableObjects.Count + mUnreliableObjects.Count == byte.MaxValue);
        public INetStreamable this[int index]
        {
            get
            {
                int offsetSize = mReliableObjects.Count;
                return (index < offsetSize) ? mReliableObjects[index] : mUnreliableObjects[index - offsetSize];
            }
        }

        protected List<INetStreamable> mReliableObjects = new();
        protected List<INetStreamable> mUnreliableObjects = new();

        public static void SetReplicatorID(ref BaseReplicationObject instance, int guid) => instance.ID = guid;

        protected BaseReplicationObject()
        {
            ID = -1;
        }

        public void AssignDataAsReliable(INetStreamable streamObject)
        {
            if (IsFull)
            {
                return;
            }

            mReliableObjects.Add(streamObject);
            ReliableDataMaxMemorySize += streamObject.MemorySize;
        }

        public void AssignDataAsUnreliable(INetStreamable streamObject)
        {
            if (IsFull)
            {
                return;
            }

            mUnreliableObjects.Add(streamObject);
            UnreliableDataMaxMemorySize += streamObject.MemorySize;
        }
    }

    public class RemoteReplicationObject : BaseReplicationObject//, IRemoteReplicable
    {
        private RemoteReplicationObject(int replicatorID, Action<RemoteReplicationObject> onDisposed = null)
        {
            ID = replicatorID;
            OnDisposed = onDisposed;
        }

        public event Action<RemoteReplicationObject> OnDisposed;
        public void Dispose() => OnDisposed?.Invoke(this);

        public static RemoteReplicationObject CreateAsRemoted(int guid = -1, Action<RemoteReplicationObject> onDisposed = null) => new RemoteReplicationObject(guid, onDisposed);

        public void ReadFromBuffer(ref NetBuffer buffer)
        {
            int reliableObjectCount = mReliableObjects.Count;
            byte repeatCount = buffer.ReadByte();

            for (int i = 0; i < repeatCount; i++)
            {
                byte propertyIndex = buffer.ReadByte();

                if (propertyIndex < reliableObjectCount)
                {
                    mReliableObjects[propertyIndex].ReadFromBuffer(ref buffer);
                }
                else
                {
                    mUnreliableObjects[propertyIndex - reliableObjectCount].ReadFromBuffer(ref buffer);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var obj in mReliableObjects)
            {
                sb.Append(obj);
            }

            foreach (var obj in mUnreliableObjects)
            {
                sb.Append(obj);
            }

            return sb.ToString();
        }
    }

    public class MasterReplicationObject : BaseReplicationObject//, IMasterReplicable
    {

        public event Action<MasterReplicationObject> OnDisposed;
        public void Dispose() => OnDisposed?.Invoke(this);
        private MasterReplicationObject(int replicatorID, Action<MasterReplicationObject> onDisposed = null)
        {
            ID = replicatorID;
            OnDisposed = onDisposed;
        }
        public static MasterReplicationObject CreateAsMaster(int guid, Action<MasterReplicationObject> onDisposed) => new MasterReplicationObject(guid, onDisposed);

        private int appendChangedReliableDataToStream(ref NetBuffer buffer)
        {
            int dataSize = mReliableObjects.Count;
            int appendCount = 0;

            for (byte i = 0; i < dataSize; i++)
            {
                var currentData = mReliableObjects[i];

                if (currentData.IsDirty)
                {
                    appendCount++;
                    buffer.Append((byte)i);
                    currentData.AppendToStream(ref buffer);
                    currentData.SetPristine();
                }
            }

            return appendCount;
        }

        private int appendChangedUnreliableDataToStream(ref NetBuffer buffer)
        {
            int offsetSize = mReliableObjects.Count;
            int dataSize = mUnreliableObjects.Count;
            int appendCount = 0;

            for (byte i = 0; i < dataSize; i++)
            {
                var currentData = mUnreliableObjects[i];

                if (currentData.IsDirty)
                {
                    appendCount++;
                    buffer.Append((byte)(i + offsetSize));
                    currentData.AppendToStream(ref buffer);
                    currentData.SetPristine();
                }
            }

            return appendCount;
        }

        private int appendReliableDataToStream(ref NetBuffer buffer)
        {
            int dataSize = mReliableObjects.Count;

            for (byte i = 0; i < dataSize; i++)
            {
                buffer.Append(i);
                mReliableObjects[i].AppendToStream(ref buffer);
            }

            return dataSize;
        }

        private int appendUnreliableDataToStream(ref NetBuffer buffer)
        {
            int offsetSize = mReliableObjects.Count;
            int dataSize = mUnreliableObjects.Count;

            for (byte i = 0; i < dataSize; i++)
            {
                buffer.Append((byte)(i + offsetSize));
                mUnreliableObjects[i].AppendToStream(ref buffer);
            }

            return dataSize;
        }

        public void AppendAllUnreliableDataToStream(ref NetBuffer buffer)
        {
            // Append empty space to set properties count
            int bufferAppendCountIndex = buffer.AppendEmptySpace(1);
            buffer.Buffer[bufferAppendCountIndex] = (byte)appendUnreliableDataToStream(ref buffer);
        }

        public void AppendChangedReliableDataToStream(ref NetBuffer buffer)
        {
            // Append empty space to set properties count
            int bufferAppendCountIndex = buffer.AppendEmptySpace(1);
            buffer.Buffer[bufferAppendCountIndex] = (byte)appendChangedReliableDataToStream(ref buffer);
        }

        public void AppendChangedUnreliableDataToStream(ref NetBuffer buffer)
        {
            // Append empty space to set properties count
            int bufferAppendCountIndex = buffer.AppendEmptySpace(1);
            buffer.Buffer[bufferAppendCountIndex] = (byte)appendChangedUnreliableDataToStream(ref buffer);
        }

        public void AppendChangedAllDataToStream(ref NetBuffer buffer)
        {
            // Append empty space to set properties count
            int bufferAppendCountIndex = buffer.AppendEmptySpace(1);

            int appendCount = appendChangedReliableDataToStream(ref buffer);
            appendCount += appendChangedUnreliableDataToStream(ref buffer);

            buffer.Buffer[bufferAppendCountIndex] = (byte)appendCount;
        }

        public void AppendAllDataToStream(ref NetBuffer buffer)
        {
            // Append empty space to set properties count
            int bufferAppendCountIndex = buffer.AppendEmptySpace(1);

            int appendCount = appendReliableDataToStream(ref buffer);
            appendCount += appendUnreliableDataToStream(ref buffer);

            buffer.Buffer[bufferAppendCountIndex] = (byte)appendCount;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var obj in mReliableObjects)
            {
                sb.Append(obj);
            }

            foreach (var obj in mUnreliableObjects)
            {
                sb.Append(obj);
            }

            return sb.ToString();
        }
    }
}