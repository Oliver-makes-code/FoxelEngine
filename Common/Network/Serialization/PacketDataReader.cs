using Greenhouse.Libs.Serialization;
using LiteNetLib.Utils;

namespace Foxel.Common.Network.Serialization;

public record PacketDataReader(NetDataReader Packet) : DataReader {
    public ArrayDataReader Array() 
        => new ArrayReader(this, Primitive().Int());
    public ArrayDataReader FixedArray(int length)
        => new ArrayReader(this, length);
    public ObjectDataReader Object()
        => new ObjectReader(this);
    public PrimitiveDataReader Primitive()
        => new PrimitiveReader(Packet);

    private class ArrayReader(PacketDataReader reader, int expectedSize) : ArrayDataReader {
        public readonly PacketDataReader Reader = reader;
        public readonly int ExpectedSize = expectedSize;
        public int count = 0;

        public override void End() {
            if (count != ExpectedSize)
                throw new ArgumentException($"Expected {ExpectedSize} elements in array. Got {count}");
        }

        public override int Length()
            => ExpectedSize;

        public override DataReader Value() {
            count++;
            return Reader;
        }
    }
    
    private class ObjectReader(PacketDataReader reader) : ObjectDataReader {
        public readonly PacketDataReader Reader = reader;

        public override void End() {}
        public override DataReader Field(string name)
            => Reader;
        public override NullableFieldDataReader NullableField(string name)
            => new NullableFieldReader(Reader, Reader.Primitive().Bool());
    }

    private class NullableFieldReader(PacketDataReader reader, bool isNull) : NullableFieldDataReader {
        public readonly PacketDataReader Reader = reader;
        public readonly bool _IsNull = isNull;

        public override void End() {}
        public override bool IsNull()
            => _IsNull;
        public override DataReader NotNull() {
            if (_IsNull)
                throw new Exception("Cannot get NotNull on null value.");
            return Reader;
        }
    }

    private record PrimitiveReader(NetDataReader Reader) : PrimitiveDataReader {
        public bool Bool()
            => Reader.GetBool();
        public byte Byte()
            => Reader.GetByte();
        public char Char()
            => Reader.GetChar();
        public double Double()
            => Reader.GetDouble();
        public float Float()
            => Reader.GetFloat();
        public int Int()
            => Reader.GetInt();
        public long Long()
            => Reader.GetLong();
        public sbyte SByte()
            => Reader.GetSByte();
        public short Short()
            => Reader.GetShort();
        public string String()
            => Reader.GetString();
        public uint UInt()
            => Reader.GetUInt();
        public ulong ULong()
            => Reader.GetULong();
        public ushort UShort()
            => Reader.GetUShort();
    }
}