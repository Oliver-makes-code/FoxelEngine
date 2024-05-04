using Newtonsoft.Json;
using Voxel.Core.Assets;
using Voxel.Core.Util;

namespace Voxel.Common.World.Content;

public abstract class ServerContentManager<TJson, TOutput> {
    public const AssetType Assets = AssetType.Content;

    private static readonly JsonSerializer Serializer = new();

    private readonly Dictionary<ResourceKey, TOutput> Registry = [];

    public TOutput this[ResourceKey key] {
        get => Registry[key];
    }

    public ServerContentManager() {
        PackManager.RegisterResourceLoader(Assets, Reload);
    }

    public void Reload(PackManager manager) {
        Registry.Clear();
        string contentDir = ContentDir();
        foreach (var key in manager.ListResources(Assets, prefix: contentDir, suffix: ".json")) {
            var outputKey = key.WithValue(key.Value.Substring(contentDir.Length, key.Value.Length - contentDir.Length - 5));

            using var stream = manager.OpenStream(Assets, key).First();
            using var sr = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(sr);

            var json = Serializer.Deserialize<TJson>(jsonTextReader);

            if (json != null)
                Registry[outputKey] = Load(outputKey, json);
        }
    }

    public abstract string ContentDir();

    public abstract TOutput Load(ResourceKey key, TJson json);
}
