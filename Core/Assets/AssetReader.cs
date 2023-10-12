using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace RenderSurface.Assets;

public sealed class AssetReader : IDisposable {
    public delegate bool ConditionDelegate(string path);
    public delegate void LoadDelegate(string path, Stream stream, int length);

    private readonly ZipArchive File;

    public AssetReader(string contentZip) {
        File = ZipFile.OpenRead(contentZip);
    }

    public bool TryGetStream(string path, [NotNullWhen(true)] out Stream? assetStream) {
        var entry = File.GetEntry(path);

        if (entry == null) {
            assetStream = null;
            return false;
        }

        assetStream = entry.Open();
        return true;
    }

    public IEnumerable<(string, Stream, int)> LoadAll(string prefix, string suffix) {
        foreach (var entry in File.Entries) {
            if (!(entry.FullName.StartsWith(prefix) && entry.FullName.EndsWith(suffix)))
                continue;

            using var str = entry.Open();
            yield return (entry.FullName, str, (int)entry.Length);
        }
    }
    
    public IEnumerable<(string, Stream, int)> LoadAll(string suffix)
        => LoadAll("", suffix);

    public void Dispose() {
        File.Dispose();
    }
}
