using Voxel.Core.Util;

namespace Voxel.Core.Assets;

public class PackManager {
    public delegate IEnumerable<T> ListForPack<T>(Pack pack);
    public delegate Task LoadResourceCallback(PackManager manager);

    private static event LoadResourceCallback? Loaders;

    private static readonly List<Func<Pack>> BuiltinPacks = [];

    public readonly AssetType AssetType;

    public readonly List<Pack> Packs = [];

    static PackManager() {
        RegisterBuiltinPack(() => new FileSystemPack("content"));
    }

    public PackManager(AssetType type) {
        AssetType = type;
    }

    public static void RegisterBuiltinPack(Func<Pack> packSupplier)
        => BuiltinPacks.Add(packSupplier);

    public static void RegisterResourceLoader(LoadResourceCallback loader)
        => Loaders += loader;

    public Task ReloadPacks() {
        Packs.Clear();
        BuiltinPacks.ForEach(it => Packs.Add(it()));
        return Loaders?.Invoke(this) ?? Task.CompletedTask;
    }

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
        => OpenRoot(Pack.BuildPath(type, key));
}
