using System.Buffers;
using System.Text;
using GlmSharp;
using Voxel.Core.Util;

namespace Voxel.Common.Util.Serialization;

/// <summary>
/// Used to read binary data directly, contains a few nice helper functions that default C# stuff doesn't have, like pulling the current bytes as a span.
/// </summary>
public class VDataReader : IDisposable {
    private byte[] dataBuffer;

    /// <summary>
    /// Where we're currently reading from in the data buffer.
    /// </summary>
    private int position = 0;

    /// <summary>
    /// Number of bytes we have left to read.
    /// </summary>
    private int remainingBytes = 0;

    public VDataReader(byte[]? data = null) {
        dataBuffer = data ?? new byte[256];
    }

    protected void EnsureSize(int size) {
        //TODO - replace with single CNPOT instead of while loop.
        while (dataBuffer.Length < size) {
            var oldBytes = dataBuffer;
            var newBytes = ArrayPool<byte>.Shared.Rent(oldBytes.Length * 2);

            dataBuffer = newBytes;

            oldBytes.CopyTo(newBytes.AsSpan());
            ArrayPool<byte>.Shared.Return(oldBytes);
        }
    }

    public virtual void LoadData(Span<byte> data) {
        EnsureSize(data.Length);
        data.CopyTo(dataBuffer);

        position = 0;
        remainingBytes = data.Length;
    }

    public void Dispose() {
        ArrayPool<byte>.Shared.Return(dataBuffer);
        dataBuffer = null;
    }

    private Span<byte> GetBytes(int length) {
        if (length > remainingBytes)
            throw new InvalidOperationException("Cannot read past end of data");

        var span = dataBuffer.AsSpan(position, length);

        remainingBytes -= length;
        position += length;

        return span;
    }

    public byte ReadByte()
        => GetBytes(1)[0];

    public ushort ReadUShort()
        => BitConverter.ToUInt16(GetBytes(sizeof(ushort)));
    public short ReadShort()
        => BitConverter.ToInt16(GetBytes(sizeof(short)));

    public uint ReadUInt()
        => BitConverter.ToUInt32(GetBytes(sizeof(uint)));
    public int ReadInt()
        => BitConverter.ToInt32(GetBytes(sizeof(int)));

    public ulong ReadULong()
        => BitConverter.ToUInt64(GetBytes(sizeof(ulong)));
    public long ReadLong()
        => BitConverter.ToInt64(GetBytes(sizeof(long)));

    public float ReadFloat()
        => BitConverter.ToSingle(GetBytes(sizeof(float)));
    public double ReadDouble()
        => BitConverter.ToDouble(GetBytes(sizeof(double)));

    public Guid ReadGuid()
        => new(GetBytes(16));

    public string ReadString()
        => ReadString(Encoding.UTF8);
    
    public string ReadString(Encoding encoding) {
        var byteCount = ReadInt();
        return encoding.GetString(GetBytes(byteCount));
    }

    public byte[] ReadByteArray() {
        var count = ReadInt();
        return GetBytes(count).ToArray();
    }

    public void ReadBytes(Span<byte> bytes)
        => GetBytes(bytes.Length).CopyTo(bytes);

    public vec3 ReadVec3()
        => new(ReadFloat(), ReadFloat(), ReadFloat());

    public ivec3 ReadIVec3()
        => new(ReadInt(), ReadInt(), ReadInt());

    public dvec3 ReadDVec3()
        => new(ReadDouble(), ReadDouble(), ReadDouble());
    public dvec2 ReadDVec2()
        => new(ReadDouble(), ReadDouble());

    public lvec3 ReadLVec3()
        => new(ReadLong(), ReadLong(), ReadLong());

    public void ReadSerializable(VSerializable serializable)
        => serializable.Read(this);

    public ResourceKey ReadResourceKey()
        => ResourceKey.Of(ReadString(), ReadString());
}
