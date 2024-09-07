using Greenhouse.Libs.Serialization;
using LiteNetLib.Utils;

namespace Foxel.Common.Network.Serialization;

public record PacketDataWriter(NetDataWriter Packet) : DataWriter {
    public ArrayDataWriter Array(int length) {
        Primitive().Int(length);
        return new ArrayWriter(this, length);
    }

    public MapDataWriter Map(int length) {
        Primitive().Int(length);
        return new MapWriter(this, length);
    }

    public ArrayDataWriter FixedArray(int length)
        => new ArrayWriter(this, length);

    public ObjectDataWriter Object(int keys)
        => new ObjectWriter(this);

    public PrimitiveDataWriter Primitive()
        => new PrimitiveWriter(Packet);

    private class ArrayWriter(PacketDataWriter writer, int expectedSize) : ArrayDataWriter {
        public readonly PacketDataWriter Writer = writer;
        public readonly int ExpectedSize = expectedSize;
        public int count = 0;

        public override void End() {
            if (ExpectedSize != count)
                throw new ArgumentException($"Expected {ExpectedSize} elements in array. Got {count}");
        }

        public override DataWriter Value() {
            count++;
            return Writer;
        }
    }

    private class MapWriter(PacketDataWriter writer, int expectedSize) : MapDataWriter {
        public readonly PacketDataWriter Writer = writer;
        public readonly int ExpectedSize = expectedSize;
        public int count = 0;

        public override void End() {
            if (ExpectedSize != count)
                throw new ArgumentException($"Expected {ExpectedSize} elements in map. Got {count}");
        }

        public override DataWriter Field(string name) {
            Writer.Primitive().String(name);
            count++;
            return Writer;
        }
    }

    private class ObjectWriter(PacketDataWriter writer) : ObjectDataWriter {
        public readonly PacketDataWriter Writer = writer;

        public override void End() {}
        public override DataWriter Field(string name)
            => Writer;
        public override NullableFieldDataWriter NullableField(string name)
            => new NullableFieldWriter(Writer);
    }

    private class NullableFieldWriter(PacketDataWriter writer) : NullableFieldDataWriter {
        public readonly PacketDataWriter Writer = writer;

        public override void End() {}
        public override DataWriter NotNull() {
            Writer.Primitive().Bool(true);
            return Writer;
        }
        public override void Null() {
            Writer.Primitive().Bool(false);
        }
    }

    private record PrimitiveWriter(NetDataWriter Packet) : PrimitiveDataWriter {
        public void Bool(bool value)
            => Packet.Put(value);
        public void Byte(byte value)
            => Packet.Put(value);
        public void Char(char value)
            => Packet.Put(value);
        public void Double(double value)
            => Packet.Put(value);
        public void Float(float value)
            => Packet.Put(value);
        public void Int(int value)
            => Packet.Put(value);
        public void Long(long value)
            => Packet.Put(value);
        public void SByte(sbyte value)
            => Packet.Put(value);
        public void Short(short value)
            => Packet.Put(value);
        public void String(string value)
            => Packet.Put(value);
        public void UInt(uint value)
            => Packet.Put(value);
        public void ULong(ulong value)
            => Packet.Put(value);
        public void UShort(ushort value)
            => Packet.Put(value);
    }
}
