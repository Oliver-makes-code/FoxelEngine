using Voxel.Core.Util;

namespace Voxel.Core.Assets;

public sealed class FileSystemPack : Pack {
    public readonly string PackRoot;

    public FileSystemPack(string packRoot) {
        while (packRoot.EndsWith('/'))
            packRoot = packRoot[0..^1];
        PackRoot = packRoot;
    }

    public IEnumerable<string> ListGroups() {
        foreach (var dir in Directory.GetDirectories(PackRoot))
            yield return dir[(PackRoot.Length + 1)..];
    }

    public IEnumerable<ResourceKey> ListResources(AssetType type, string prefix = "", string suffix = "") {
        foreach (var group in ListGroups()) {
            string rootPath = $"{PackRoot}/{group}/{type.AsString()}/";

            if (!Directory.Exists(rootPath))
                continue;
            
            Queue<string> toSearch = [];
            toSearch.Enqueue(rootPath);

            while (toSearch.Count != 0) {
                string path = toSearch.Dequeue();
                foreach (var file in Directory.GetFiles(path)) {
                    string value = file[rootPath.Length..];

                    if (
                        value.StartsWith(prefix)
                        && value.EndsWith(suffix)
                        && value.Length >= prefix.Length + suffix.Length
                    )
                        yield return new(group, value);
                }
                Array.ForEach(Directory.GetDirectories(path), toSearch.Enqueue);
            }
        }
    }
    
    public Stream? OpenRoot(string path) {
        string file = $"{PackRoot}/{path}";
        if (File.Exists(file))
            return File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return null;
    }

    public void Dispose() {}
}
