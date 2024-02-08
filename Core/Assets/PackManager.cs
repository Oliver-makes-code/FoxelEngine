using Voxel.Core.Util;

namespace Voxel.Core.Assets;

public class PackManager {
    public delegate IEnumerable<T> ListForPack<T>(ContentPack pack);

    public List<ContentPack> Packs = [];

    public IEnumerable<T> ListEach<T>(ListForPack<T> func) {
        HashSet<T> visited = [];
        foreach (var pack in Packs) {
            foreach (var item in func(pack)) {
                if (visited.Contains(item))
                    continue;
                visited.Add(item);
                yield return item;
            }
        }
    }

    public IEnumerable<string> ListGroups()
        => ListEach(pack => pack.ListGroups());

    public IEnumerable<ResourceKey> ListResources(AssetType type, string prefix = "", string suffix = "")
        => ListEach(pack => pack.ListResources(type, prefix, suffix));

    public IEnumerable<Stream> OpenRoot(string path) {
        foreach (var pack in Packs) {
            var root = pack.OpenRoot(path);
            if (root != null)
                yield return root;
        }
    }

    public IEnumerable<Stream> OpenStream(AssetType type, ResourceKey key)
        => OpenRoot(ContentPack.BuildPath(type, key));
}
