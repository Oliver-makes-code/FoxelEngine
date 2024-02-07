using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Voxel.Core.Util;

namespace Voxel.Core.Assets;

public sealed class AssetReader : IDisposable {
    public delegate bool ConditionDelegate(string path);
    public delegate void LoadDelegate(string path, Stream stream, int length);

    public static readonly JsonSerializer Serializer = new();

    private readonly ZipArchive File;

    public readonly string Group;

    public AssetReader(string contentZip) {
        File = ZipFile.OpenRead(contentZip);
        var entry = File.GetEntry("root.json");
        if (entry == null) {
            Group = ResourceKey.DefaultGroup;
            return;
        }
        using var str = entry.Open();
        using var sr = new StreamReader(str);
        using var jsonTextReader = new JsonTextReader(sr);

        var root = Serializer.Deserialize<RootJson>(jsonTextReader);
        Group = root.Group;
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

    public class RootJson {
        public String Group { get; set; }
    }
}
