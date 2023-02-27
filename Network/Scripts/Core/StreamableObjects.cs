using Network.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

namespace Network
{
    public interface INetStreamable
    {
        public int MemorySize { get; }
        public bool IsDirty { get; }

        public void SetPristine();
        public void AppendToStream(ref NetBuffer streamBuffer);
        public void ReadFromBuffer(ref NetBuffer streamBuffer);
    }

    public class NetVector3Data : Notifier<Vector3>, INetStreamable
    {
        public NetVector3Data(Vector3 value = new Vector3()) : base(value) { }
        public int MemorySize => sizeof(float) * 3;
        public void AppendToStream(ref NetBuffer streamBuffer)
        {
            streamBuffer.Append(this.Value.x);
            streamBuffer.Append(this.Value.y);
            streamBuffer.Append(this.Value.z);
        }
        public void ReadFromBuffer(ref NetBuffer streamBuffer)
        {
            Vector3 data;
            data.x = streamBuffer.ReadFloat();
            data.y = streamBuffer.ReadFloat();
            data.z = streamBuffer.ReadFloat();
            this.Value = data;
        }
    }

    public class NetQuaternionData : Notifier<Quaternion>, INetStreamable
    {
        public NetQuaternionData(Quaternion value = new Quaternion()) : base(value) { }
        public int MemorySize => sizeof(float) * 4;
        public void AppendToStream(ref NetBuffer streamBuffer)
        {
            streamBuffer.Append(this.Value.x);
            streamBuffer.Append(this.Value.y);
            streamBuffer.Append(this.Value.z);
            streamBuffer.Append(this.Value.w);
        }
        public void ReadFromBuffer(ref NetBuffer streamBuffer)
        {
            Quaternion data;
            data.x = streamBuffer.ReadFloat();
            data.y = streamBuffer.ReadFloat();
            data.z = streamBuffer.ReadFloat();
            data.w = streamBuffer.ReadFloat();
            this.Value = data;
        }
    }

    public class NetBooleanData : Notifier<bool>, INetStreamable
    {
        public NetBooleanData(bool value = false) : base(value) { }
        public int MemorySize => sizeof(byte);
        public void AppendToStream(ref NetBuffer streamBuffer) => streamBuffer.Append(Value);
        public void ReadFromBuffer(ref NetBuffer streamBuffer) => Value = streamBuffer.ReadBoolean();
    }

    public class NetByteData : Notifier<byte>, INetStreamable
    {
        public NetByteData(byte value = 0) : base(value) { }
        public int MemorySize => sizeof(byte);
        public void AppendToStream(ref NetBuffer streamBuffer) => streamBuffer.Append(Value);
        public void ReadFromBuffer(ref NetBuffer streamBuffer) => Value = streamBuffer.ReadByte();
    }

    public class NetShortData : Notifier<short>, INetStreamable
    {
        public NetShortData(short value = 0) : base(value) { }
        public int MemorySize => sizeof(short);
        public void AppendToStream(ref NetBuffer streamBuffer) => streamBuffer.Append(Value);
        public void ReadFromBuffer(ref NetBuffer streamBuffer) => Value = streamBuffer.ReadInt16();
    }

    public class NetIntData : Notifier<int>, INetStreamable
    {
        public NetIntData(int value = 0) : base(value) { }
        public int MemorySize => sizeof(int);
        public void AppendToStream(ref NetBuffer streamBuffer) => streamBuffer.Append(Value);
        public void ReadFromBuffer(ref NetBuffer streamBuffer) => Value = streamBuffer.ReadInt32();
    }

    public class NetLongData : Notifier<long>, INetStreamable
    {
        public NetLongData(long value = 0) : base(value) { }
        public int MemorySize => sizeof(long);
        public void AppendToStream(ref NetBuffer streamBuffer) => streamBuffer.Append(Value);
        public void ReadFromBuffer(ref NetBuffer streamBuffer) => Value = streamBuffer.ReadInt64();
    }

    public class NetFloatData : Notifier<float>, INetStreamable
    {
        public NetFloatData(float value = 0) : base(value) { }
        public int MemorySize => sizeof(float);
        public void AppendToStream(ref NetBuffer streamBuffer) => streamBuffer.Append(Value);
        public void ReadFromBuffer(ref NetBuffer streamBuffer) => Value = streamBuffer.ReadFloat();
    }

    public class NetDoubleData : Notifier<double>, INetStreamable
    {
        public NetDoubleData(double value = 0) : base(value) { }
        public int MemorySize => sizeof(double);
        public void AppendToStream(ref NetBuffer streamBuffer) => streamBuffer.Append(Value);
        public void ReadFromBuffer(ref NetBuffer streamBuffer) => Value = streamBuffer.ReadDouble();
    }

    public class NetStringData : Notifier<string>, INetStreamable
    {
        public NetStringData(string value = "") : base(value) { }
        public int MemorySize => Encoding.UTF8.GetByteCount(Value);
        public void AppendToStream(ref NetBuffer streamBuffer) => streamBuffer.Append(Value);
        public void ReadFromBuffer(ref NetBuffer streamBuffer) => Value = streamBuffer.ReadString();
    }

    public class NetEntityTypeData : Notifier<EntityType>, INetStreamable
    {
        public NetEntityTypeData(EntityType value = EntityType.kNoneEntityType) : base(value) { }
        public int MemorySize => sizeof(int);
        public void AppendToStream(ref NetBuffer streamBuffer) => streamBuffer.Append(Value);
        public void ReadFromBuffer(ref NetBuffer streamBuffer) => Value = streamBuffer.ReadEntityType();
    }

    public class NetItemTypeData : Notifier<ItemType>, INetStreamable
    {
        public NetItemTypeData(ItemType value = ItemType.kNoneItemType) : base(value) { }
        public int MemorySize => sizeof(int);
        public void AppendToStream(ref NetBuffer streamBuffer) => streamBuffer.Append(Value);
        public void ReadFromBuffer(ref NetBuffer streamBuffer) => Value = streamBuffer.ReadItemType();
    }

    public class NetEnumTypeData<T> : INetStreamable where T : Enum
    {
        public event Action<T> OnDataChanged;
        public NetEnumTypeData(T value) => enumValue = (int)(object)value;
        public int enumValue;
        public T Value
        {
            get
            {
                return (T)(object)enumValue;
            }
            set
            {
                IsDirty = true;
                enumValue = (int)(object)value;
                OnDataChanged?.Invoke(Value);
            }
        }
        public int MemorySize => sizeof(int);
        public bool IsDirty { get; private set; }
        public void AppendToStream(ref NetBuffer streamBuffer) => streamBuffer.Append(enumValue);
        public void ReadFromBuffer(ref NetBuffer streamBuffer) => Value = (T)(object)streamBuffer.ReadInt32();
        public void SetPristine()
        {
            IsDirty = false;
        }
    }
}