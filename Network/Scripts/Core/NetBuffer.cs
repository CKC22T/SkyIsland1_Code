using Network.Packet;
using System;
using System.Diagnostics;
using System.Text;

namespace Network
{
    public class NetBuffer
    {
        private const int mMaxBufferCapacity = ServerConfiguration.MAX_BUFFER_CAPACITY_SIZE;
        private const int mInitialBufferSize = ServerConfiguration.DATA_BUFFER_SIZE;

        private byte[] mBuffer;
        public int Size { get; private set; }
        public int ReadIndex { get; private set; }

        public byte[] Buffer => mBuffer;
        public bool IsEmpty => (mBuffer == null || Size == 0);
        public int Capacity => mBuffer.Length;

        public NetBuffer()
        {
            mBuffer = new byte[mInitialBufferSize];
            ReadIndex = 0;
        }

        public NetBuffer(Response data)
        {
            var packetData = data.ToByteArray();
            mBuffer = new byte[packetData.Length + 1 + sizeof(int)];
            Append(PrimitivePacketType.RESPONSE_SERVER_PROTOBUF);
            AppendByteWithLength(packetData);
        }

        public NetBuffer(Request data)
        {
            var packetData = data.ToByteArray();
            mBuffer = new byte[packetData.Length + 1 + sizeof(int)];
            Append(PrimitivePacketType.REQUEST_CLIENT_PROTOBUF);
            AppendByteWithLength(packetData);
        }

        public NetBuffer(Response.Builder data) : this(data.Build()) { }
        public NetBuffer(Request.Builder data) : this(data.Build()) { }

        public NetBuffer(PrimitivePacketType packetType)
        {
            mBuffer = new byte[mInitialBufferSize];
            Append(packetType);
        }

        public NetBuffer(PrimitivePacketType packetType, NetBuffer rawData)
        {
            mBuffer = new byte[mInitialBufferSize];
            Append(packetType);
            Append(rawData.Buffer, 0, rawData.Size);
        }

        public NetBuffer(int capacity)
        {
            mBuffer = new byte[capacity];
            ReadIndex = 0;
        }

        public NetBuffer(byte[] data)
        {
            mBuffer = data;
            Size = data.Length;
            ReadIndex = 0;
        }

        public void Clear()
        {
            Size = 0;
            ReadIndex = 0;
        }

        public void ResetRead()
        {
            ReadIndex = 0;
        }

        public void ForceSetSize(int size)
        {
            Size = size;
        }

        public void Reserve(int capacity)
        { 
            Debug.Assert((capacity >= 0), LogManager.GetLogMessage("Wrong reserve capacity", NetworkLogType.Buffer, true));
            if (capacity < 0)
                throw new ArgumentException("Invalid reserve capacity!", nameof(capacity));

            if (capacity > mMaxBufferCapacity)
            {
                capacity = mMaxBufferCapacity;
            }

            if (capacity > this.Capacity)
            {
                byte[] newBuffer = new byte[Math.Max(capacity, this.Capacity * 2)];
                Array.Copy(mBuffer, newBuffer, Size);
                mBuffer = newBuffer;
            }
        }

        /// <summary>데이터를 amount만큼 앞으로 이동시킵니다.</summary>
        public void ShiftToFront(int amount, bool shouldShiftReadIndex = false)
        {
            Debug.Assert(Size - amount >= 0, LogManager.GetLogMessage("Wrong shift amount!", NetworkLogType.TcpNetworkCore, true));
            if (Size - amount < 0)
            {
                return;
            }

            // Shift to front
            int newSize = Size - amount; // 
            for (int i = 0; i < newSize; i++)
            {
                mBuffer[i] = mBuffer[i + amount];
            }

            Size = newSize;

            if (shouldShiftReadIndex)
            {
                ReadIndex -= amount;
                if (ReadIndex < 0)
                {
                    ReadIndex = 0;
                }
            }
        }

        /// <summary>데이터를 읽은만큼 데이터를 이동시킵니다.</summary>
        public void ShiftToFrontByReadIndex()
        {
            ShiftToFront(ReadIndex);
            ReadIndex = 0;
        }

        #region Force bind operations

        public void ForceBindInt32(int data, int index)
        {
            byte[] convertedData = BitConverter.GetBytes(data);
            ForceBindBytes(convertedData, index);
        }

        public void ForceBindByte(byte data, int index)
        {
            mBuffer[index] = data;
        }

        public void ForceBindBytes(byte[] data, int index)
        {
            Array.Copy(data, 0, mBuffer, index, data.Length);
        }

        #endregion

        #region Data append operations

        /// <summary>마지막 위치에 'count'만큼 비어있는 버퍼를 생성합니다.</summary>
        /// <param name="count">비어있는 위치 입니다.</param>
        /// <returns></returns>
        public int AppendEmptySpace(int count)
        {
            int emptySpaceStartIndex = Size;
            Reserve(Size + count);
            Size += count;
            return emptySpaceStartIndex;
        }

        public void Append(PrimitivePacketType packetType)
        {
            Append((byte)packetType);
        }

        public void Append(EntityType entityType)
        {
            Append((int)entityType);
        }

        public void Append(ItemType itemType)
        {
            Append((int)itemType);
        }

        public void Append(NetBuffer buffer)
        {
            Append((int)buffer.Size);
            Append(buffer.Buffer, 0, buffer.Size);
        }

        public void Append(byte[] buffer, int offset, int count)
        {
            Reserve(Size + count);
            Array.Copy(buffer, offset, mBuffer, Size, count);
            Size += count;
        }

        public void Append(bool data)
        {
            Reserve(Size + sizeof(byte));
            Buffer[Size] = data ? (byte)1 : (byte)0;
            Size += sizeof(byte);
        }

        public void Append(byte data)
        {
            Reserve(Size + sizeof(byte));
            Buffer[Size] = data;
            Size += sizeof(byte);
        }

        public void Append(byte[] data)
        {
            Reserve(Size + data.Length);
            Array.Copy(data, 0, mBuffer, Size, data.Length);
            Size += data.Length;
        }

        public void Append(short data)
        {
            byte[] convertedData = BitConverter.GetBytes(data);
            Append(convertedData);
        }

        public void Append(int data)
        {
            byte[] convertedData = BitConverter.GetBytes(data);
            Append(convertedData);
        }

        public void Append(long data)
        {
            byte[] convertedData = BitConverter.GetBytes(data);
            Append(convertedData);
        }

        public void Append(float data)
        {
            byte[] convertedData = BitConverter.GetBytes(data);
            Append(convertedData);
        }

        public void Append(double data)
        {
            byte[] convertedData = BitConverter.GetBytes(data);
            Append(convertedData);
        }

        public void Append(string data)
        {
            var stringData = Encoding.UTF8.GetBytes(data);
            AppendByteWithLength(stringData);
        }

        public void AppendByteWithLength(byte[] data)
        {
            Append(data.Length);
            Append(data);
        }

        public void Append(Request data) => AppendByteWithLength(data.ToByteArray());

        public void Append(Response data) => AppendByteWithLength(data.ToByteArray());

        public void Append(Response.Builder data) => AppendByteWithLength(data.Build().ToByteArray());

        public void Append(Request.Builder data) => AppendByteWithLength(data.Build().ToByteArray());

        #endregion

        #region Data peek operations

        public byte PickByte() => mBuffer[ReadIndex];

        public int PickInt32() => BitConverter.ToInt32(mBuffer, ReadIndex);

        public long PickInt64() => BitConverter.ToInt64(mBuffer, ReadIndex);

        public float PickFloat() => BitConverter.ToSingle(mBuffer, ReadIndex);

        public double PickDouble() => BitConverter.ToDouble(mBuffer, ReadIndex);

        public string PickString()
        {
            int length = PickInt32();
            string data = Encoding.UTF8.GetString(mBuffer, ReadIndex + sizeof(int), length);
            return data;
        }

        #endregion

        #region Data read operations

        public PrimitivePacketType ReadPrimitivePacketType()
        {
            return (PrimitivePacketType)ReadByte();
        }

        public EntityType ReadEntityType()
        {
            return (EntityType)ReadInt32();
        }

        public ItemType ReadItemType()
        {
            return (ItemType)ReadInt32();
        }

        public bool ReadBoolean()
        {
            bool value = mBuffer[ReadIndex] > 0 ? true : false;
            ReadIndex += sizeof(byte);
            return value;
        }

        public byte ReadByte()
        {
            byte data = mBuffer[ReadIndex];
            ReadIndex += sizeof(byte);
            return data;
        }

        public short ReadInt16()
        {
            short data = BitConverter.ToInt16(mBuffer, ReadIndex);
            ReadIndex += sizeof(short);
            return data;
        }

        public int ReadInt32()
        {
            int data = BitConverter.ToInt32(mBuffer, ReadIndex);
            ReadIndex += sizeof(int);
            return data;
        }

        public long ReadInt64()
        {
            long data = BitConverter.ToInt64(mBuffer, ReadIndex);
            ReadIndex += sizeof(long);
            return data;
        }

        public float ReadFloat()
        {
            float data = BitConverter.ToSingle(mBuffer, ReadIndex);
            ReadIndex += sizeof(float);
            return data;
        }

        public double ReadDouble()
        {
            double data = BitConverter.ToDouble(mBuffer, ReadIndex);
            ReadIndex += sizeof(double);
            return data;
        }

        public string ReadString()
        {
            int length = ReadInt32();
            string data = Encoding.UTF8.GetString(mBuffer, ReadIndex, length);
            ReadIndex += length;
            return data;
        }

        public byte[] ReadBytes(int readSize)
        {
            byte[] data = new byte[readSize];
            Array.Copy(mBuffer, ReadIndex, data, 0, readSize);
            ReadIndex += readSize;
            return data;
        }

        public byte[] ReadBytesByLength()
        {
            int length = ReadInt32();
            return ReadBytes(length);
        }

        public NetBuffer ReadNetBuffer()
        {
            int readSize = ReadInt32();
            byte[] data = ReadBytes(readSize);
            return new NetBuffer(data);
        }

        public NetBuffer PopNetBuffer()
        {
            int readSize = ReadInt32();
            byte[] data = ReadBytes(readSize);

            ShiftToFront(sizeof(int) + readSize, true);

            return new NetBuffer(data);
        }

        public Request ReadRequest() => Request.ParseFrom(ReadBytesByLength());

        public Response ReadResponse() => Response.ParseFrom(ReadBytesByLength());

        #endregion

        #region Readable Check Function

        public bool CanReadNetBuffer()
        {
            if (!CanReadInt32())
            {
                return false;
            }

            int netBufferReadSize = PickInt32();

            if (netBufferReadSize <= 0)
            {
                return false;
            }

            int currentReadIndex = ReadIndex;

            if (currentReadIndex + sizeof(int) + netBufferReadSize <= Size)
            {
                return true; // You can read net buffer.
            }
            else
            {
                return false; // You can't read net buffer. it's too short to read.
            }
        }

        public bool CanReadInt32() => (ReadIndex + sizeof(int) <= Size);

        public bool CanReadByte() => (ReadIndex + sizeof(byte) <= Size);

        public bool CanReadBytesByLength()
        {
            if (!CanReadInt32())
                return false;

            int byteLength = PickInt32();

            return (ReadIndex + sizeof(int) + byteLength <= Size);
        }

        public bool IsValidPacketType()
        {
            if (!CanReadByte())
                return false;

            return PickByte().IsValidPacketType();
        }

        #endregion
    }
}
