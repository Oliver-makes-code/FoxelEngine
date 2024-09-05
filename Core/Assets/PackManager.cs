using System.Runtime.CompilerServices;
using NLog;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;
using Newtonsoft.Json;
using Greenhouse.Libs.Serialization.Reader;

namespace Foxel.Core.Assets;

public class PackManager {
    public const string JsonSuffix = ".json";

    public delegate IEnumerable<T> ListForPack<T>(Pack pack);
    public delegate Task LoadResourceCallback(PackManager manager);
    public delegate void SyncLoadResourceCallback(PackManager manager);

    private static readonly List<ReloadTask>[] Loaders = [[],[]];

    private static readonly List<Func<Pack>> BuiltinPacks = [];

    public readonly AssetType AssetType;

    public readonly List<Pack> Packs = [];

    private readonly ILogger Logger;

    static PackManager() {
        RegisterBuiltinPack(() => new FileSystemPack("builtin"));
    }

    public PackManager(AssetType type, ILogger logger) {
        AssetType = type;
        Logger = logger;
    }

    public static void RegisterBuiltinPack(Func<Pack> packSupplier)
        => BuiltinPacks.Add(packSupplier);

    public static ReloadTask RegisterResourceLoader(AssetType assetType, LoadResourceCallback loader) {
        var task = new ReloadTask(loader);
        Loaders[assetType == AssetType.Assets ? 1 : 0].Add(task);
        return task;
    }

    public static ReloadTask RegisterResourceLoader(AssetType assetType, SyncLoadResourceCallback loader)
        => RegisterResourceLoader(assetType, (manager) => Task.Run(() => loader(manager)));

    public async Task ReloadPacks() {
        Logger.Info($"Reloading packs for {AssetType}");
        Packs.Clear();
        foreach (var packConstructor in BuiltinPacks) {
            var pack = packConstructor();
            var metadata = pack.GetMetadata();
            if (metadata == null)
                continue;
            Packs.Add(pack);
            Logger.Info($"Found pack {metadata.Name}");
        }
        // TODO: Load packs dynamically

        Logger.Info($"Loading {Packs.Count} pack{(Packs.Count == 1 ? "" : "s")}");

        int idx = AssetType == AssetType.Assets ? 1 : 0;
        
        Task[] tasks = new Task[Loaders[idx].Count];
        for (int i = 0; i < tasks.Length; i++)
            Loaders[idx][i].Reset();
        for (int i = 0; i < tasks.Length; i++)
            tasks[i] = Loaders[idx][i].Run(this);
        foreach (var task in tasks)
            await task;
        Logger.Info("Done reloading");
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
    
    public IEnumerable<(ResourceKey, TValue)> OpenJsons<TValue>(AssetType type, Codec<TValue> codec, string prefix = "") {
        foreach (var resource in ListResources(type, prefix, JsonSuffix)) {
            int start = prefix.Length;
            int end = resource.Value.Length - JsonSuffix.Length;
            string name = resource.Value[start..end];
            var key = new ResourceKey(resource.Group, name);

            // Todo: Apply patches.
            using var stream = OpenStream(AssetType.Assets, resource).Last();
            using var sr = new StreamReader(stream);
            using var jr = new JsonTextReader(sr);
            var reader = new JsonDataReader(jr);
            
            yield return (key, codec.ReadGeneric(reader));
        }
    }

    public IEnumerable<Stream> OpenRoot(string path) {
        foreach (var pack in Packs) {
            var root = pack.OpenRoot(path);
            if (root != null)
                yield return root;
        }
    }

    public IEnumerable<Stream> OpenStream(AssetType type, ResourceKey key)
        => OpenRoot(Pack.BuildPath(type, key));

    public record ReloadTask(LoadResourceCallback Callback) : IAsyncResult, INotifyCompletion {
        public object? AsyncState => null;

        public WaitHandle AsyncWaitHandle => manualResetEvent;

        public bool CompletedSynchronously => !active;

        public bool IsCompleted => !active;

        private bool active = false;
        private ManualResetEvent manualResetEvent = new(false);
        private List<Action> continuations = [];

        public void Reset() {
            active = true;
            manualResetEvent.Reset();
        }
        
        public async Task Run(PackManager packs) {
            await Callback(packs);
            active = false;
            manualResetEvent.Set();
            Complete();
        }

        public ReloadTask GetAwaiter() {
            return this;
        }

        public void OnCompleted(Action continuation) {
            if (IsCompleted) {
                continuation.Invoke();
                return;
            }
            continuations.Add(continuation);
        }

        public void GetResult() {}

        private void Complete() {
            foreach (var continuation in continuations)
                continuation.Invoke();
        }
    }
}
