using ZstdSharp;

namespace Common.Util.Serialization.Compressed;

public class CompressedVDataReader : VDataReader {


    public override void LoadData(Span<byte> data) {
        using var decompressor = new Decompressor();

        var expectedSize = Decompressor.GetDecompressedSize(data);

        base.LoadData(decompressor.Unwrap(data)); //TODO - Allocation here...
    }
}
