using Voxel.Core.Util;

namespace Voxel.Core.Assets;

public class PackManager {
    public delegate IEnumerable<T> ListForPack<T>(Pack pack);
    public delegate Task LoadResourceCallback(PackManager manager);
    public delegate void SyncLoadResourceCallback(PackManager manager);

    private static List<ReloadTask> Loaders = [];

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

    public static ReloadTask RegisterResourceLoader(LoadResourceCallback loader) {
        var task = new ReloadTask(loader);
        Loaders.Add(task);
        return task;
    }

    public static ReloadTask RegisterResourceLoader(SyncLoadResourceCallback loader)
        => RegisterResourceLoader((manager) => Task.Run(() => loader(manager)));

    public async Task ReloadPacks() {
        Game.Logger.Info("Reloading packs...");
        Packs.Clear();
        foreach (var packConstructor in BuiltinPacks) {
            var pack = packConstructor();
            var metadata = pack.GetMetadata();
            if (metadata == null)
                continue;
            Packs.Add(pack);
            Game.Logger.Info($"Found pack {metadata.Name}");
        }
        // TODO: Load packs dynamically

        Game.Logger.Info($"Loading {Packs.Count} pack{(Packs.Count == 1 ? "" : "s")}");
        
        Task[] tasks = new Task[Loaders.Count];
        for (int i = 0; i < tasks.Length; i++)
            Loaders[i].Reset();
        for (int i = 0; i < tasks.Length; i++)
            tasks[i] = Loaders[i].Run(this);
        foreach (var task in tasks)
            await task;
        Game.Logger.Info("Done reloading");
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

    public record ReloadTask(LoadResourceCallback Callback) : IAsyncResult {
        public object? AsyncState => null;

        public WaitHandle AsyncWaitHandle => manualResetEvent;

        public bool CompletedSynchronously => !active;

        public bool IsCompleted => !active;

        private bool active = false;
        private ManualResetEvent manualResetEvent = new(false);

        public void Reset() {
            active = true;
            manualResetEvent.Reset();
        }
        
        public async Task Run(PackManager packs) {
            await Callback(packs);
            active = false;
            manualResetEvent.Set();
        }
    }
}
