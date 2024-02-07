using System.Buffers;
using System.Text;
using GlmSharp;
using Voxel.Core.Util;

namespace Voxel.Common.Util.Serialization;

/// <summary>
/// Used to write binary data directly, contains a few nice helper functions that default C# stuff doesn't have, like pulling the current bytes as a span.
/// </summary>
public class VDataWriter : IDisposable {
    private byte[] dataBuffer = new byte[256];
    /// <summary>
    /// Where we're currently writing in the data buffer.
    /// </summary>
    private int position = 0;

    /// <summary>
    /// Bytes that we have left to write to
    /// </summary>
    private int freeBytes => dataBuffer.Length - position;


    public virtual Span<byte> currentBytes => dataBuffer.AsSpan(0, position);
    public byte[] pooledByteArray {
        get {
            var arr = ArrayPool<byte>.Shared.Rent(position);
            currentBytes.CopyTo(arr);

            return arr;
        }
    }

    public void Reset() {
        position = 0;
    }

    public void Dispose() {
        ArrayPool<byte>.Shared.Return(dataBuffer);
        dataBuffer = null;
    }

    private void EnsureFreeBytes(int number) {
        while (freeBytes < number) {
            var oldBytes = dataBuffer;
            var newBytes = ArrayPool<byte>.Shared.Rent(oldBytes.Length * 2);

            dataBuffer = newBytes;

            oldBytes.CopyTo(newBytes.AsSpan());
            ArrayPool<byte>.Shared.Return(oldBytes);
        }
    }

    private Span<byte> GetBytes(int length) {
        EnsureFreeBytes(length);
        var span = dataBuffer.AsSpan(position, length);
        position += length;

        return span;
    }

    public void Write(byte data)
        => GetBytes(1)[0] = data;

    public void Write(ushort data)
        => BitConverter.TryWriteBytes(GetBytes(sizeof(ushort)), data);
    public void Write(short data)
        => BitConverter.TryWriteBytes(GetBytes(sizeof(short)), data);

    public void Write(uint data)
        => BitConverter.TryWriteBytes(GetBytes(sizeof(uint)), data);
    public void Write(int data)
        => BitConverter.TryWriteBytes(GetBytes(sizeof(int)), data);

    public void Write(ulong data)
        => BitConverter.TryWriteBytes(GetBytes(sizeof(ulong)), data);
    public void Write(long data)
        => BitConverter.TryWriteBytes(GetBytes(sizeof(long)), data);

    public void Write(float data)
        => BitConverter.TryWriteBytes(GetBytes(sizeof(float)), data);
    public void Write(double data)
        => BitConverter.TryWriteBytes(GetBytes(sizeof(double)), data);

    public void Write(Guid data)
        => data.TryWriteBytes(GetBytes(16));

    public void Write(string data)
        => Write(data, Encoding.UTF8);

    public void Write(string data, Encoding encoding) {
        if (data.Length == 0)
            throw new InvalidOperationException("Cannot write empty string");
        var len = encoding.GetByteCount(data);

        Write(len);
        encoding.GetBytes(data, GetBytes(len));
    }

    public void Write(Span<byte> data) {
        data.CopyTo(GetBytes(data.Length));
    }

    public void Write(vec3 data) {
        Write(data.x);
        Write(data.y);
        Write(data.z);
    }

    public void Write(ivec3 data) {
        Write(data.x);
        Write(data.y);
        Write(data.z);
    }

    public void Write(dvec3 data) {
        Write(data.x);
        Write(data.y);
        Write(data.z);
    }

    public void Write(dvec2 data) {
        Write(data.x);
        Write(data.y);
    }

    public void Write(lvec3 data) {
        Write(data.x);
        Write(data.y);
        Write(data.z);
    }

    public void Write(VSerializable serializable)
        => serializable.Write(this);

    public void Write(ResourceKey key) {
        Write(key.Group);
        Write(key.Value);
    }
}
