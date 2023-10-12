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

    public IEnumerable<(string path, Stream stream, int length)> LoadAll(ConditionDelegate condition) {
        foreach (var entry in File.Entries) {
            if (!condition(entry.FullName))
                continue;

            using var str = entry.Open();
            yield return (entry.FullName, str, (int)entry.Length);
        }
    }

    public void LoadAll(ConditionDelegate condition, LoadDelegate loader) {
        foreach ((string path, var stream, int length) in LoadAll(condition))
            loader(path, stream, length);
    }

    public void Dispose() {
        File.Dispose();
    }
}
