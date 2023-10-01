using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace RenderSurface.Assets;

public class AssetReader : IDisposable {

    private ZipArchive _file;

    public AssetReader(string contentZip) {
        _file = ZipFile.OpenRead(contentZip);
    }

    public bool TryGetStream(string path, [NotNullWhen(true)] out Stream? assetStream) {
        var entry = _file.GetEntry(path);

        if (entry == null) {
            assetStream = null;
            return false;
        }

        assetStream = entry.Open();
        return true;
    }

    public void Dispose() {
        _file.Dispose();
    }
}
