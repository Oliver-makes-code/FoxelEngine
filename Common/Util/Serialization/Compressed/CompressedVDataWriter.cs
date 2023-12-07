using ZstdSharp;

namespace Common.Util.Serialization.Compressed;

public class CompressedVDataWriter : VDataWriter {

    public override Span<byte> currentBytes {
        get {
            var b = base.currentBytes;
            using var compressor = new Compressor();
            return compressor.Wrap(b);
        }
    }
}
