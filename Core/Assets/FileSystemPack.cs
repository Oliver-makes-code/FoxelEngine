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
        if (!Directory.Exists(PackRoot))
            yield break;
        
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

                if (!Directory.Exists(path))
                    continue;
                
                foreach (var file in Directory.GetFiles(path.Replace('/', Path.DirectorySeparatorChar))) {
                    string value = file[rootPath.Length..].Replace(Path.DirectorySeparatorChar, '/');

                    if (
                        value.StartsWith(prefix)
                        && value.EndsWith(suffix)
                        && value.Length >= prefix.Length + suffix.Length
                    )
                        yield return new(group, value);
                }
                foreach (string dir in Directory.GetDirectories(path.Replace('/', Path.DirectorySeparatorChar)))
                    toSearch.Enqueue(dir.Replace(Path.DirectorySeparatorChar, '/'));
            }
        }
    }
    
    public Stream? OpenRoot(string path) {
        string file = $"{PackRoot}/{path}".Replace('/', Path.DirectorySeparatorChar);
        
        if (File.Exists(file))
            return File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        
        return null;
    }

    public void Dispose() {}
}
