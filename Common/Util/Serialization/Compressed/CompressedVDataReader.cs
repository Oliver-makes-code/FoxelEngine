using System.Buffers;
using ZstdSharp;

namespace Foxel.Common.Util.Serialization.Compressed;

public class CompressedVDataReader : VDataReader {


    public override void LoadData(Span<byte> data) {
        using var decompressor = new Decompressor();

        var expectedSize = Decompressor.GetDecompressedSize(data);
        var rented = ArrayPool<byte>.Shared.Rent((int)expectedSize);

        decompressor.Unwrap(data, rented.AsSpan());
        base.LoadData(rented);
        
        ArrayPool<byte>.Shared.Return(rented);
    }
}
