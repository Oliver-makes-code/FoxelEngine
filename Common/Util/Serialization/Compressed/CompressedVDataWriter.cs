using System.Buffers;
using ZstdSharp;

namespace Foxel.Common.Util.Serialization.Compressed;

public class CompressedVDataWriter : VDataWriter {

    public override Span<byte> currentBytes {
        get {
            var b = base.currentBytes;
            using var compressor = new Compressor();

            int expectedSize = Compressor.GetCompressBound(b.Length);
            var rented = ArrayPool<byte>.Shared.Rent(expectedSize);

            int written = compressor.Wrap(b, rented.AsSpan());
            Reset();

            Write(rented.AsSpan(0, written));

            ArrayPool<byte>.Shared.Return(rented);

            return base.currentBytes;
        }
    }
}
