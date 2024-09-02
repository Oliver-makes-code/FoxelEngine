using Newtonsoft.Json;
using Foxel.Core.Assets;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;
using Greenhouse.Libs.Serialization.Reader;

namespace Foxel.Common.World.Content;

public abstract class ServerContentManager<TJson, TOutput> {
    public const AssetType Assets = AssetType.Content;

    private readonly Dictionary<ResourceKey, TOutput> Registry = [];
    private readonly Codec<TJson> Codec;

    public TOutput this[ResourceKey key] {
        get => Registry[key];
    }

    public ServerContentManager(Codec<TJson> codec) {
        Codec = codec;
        PackManager.RegisterResourceLoader(Assets, Reload);
    }

    public void Reload(PackManager manager) {
        Registry.Clear();
        string contentDir = ContentDir();
        PreLoad();
        foreach (var key in manager.ListResources(Assets, prefix: contentDir, suffix: ".json")) {
            var outputKey = key.WithValue(key.Value.Substring(contentDir.Length, key.Value.Length - contentDir.Length - 5));

            using var stream = manager.OpenStream(Assets, key).First();
            using var sr = new StreamReader(stream);
            using var jr = new JsonTextReader(sr);
            var reader = new JsonDataReader(jr);

            var json = Codec.ReadGeneric(reader);

            if (json != null)
                Registry[outputKey] = Load(outputKey, json);
        }
        PostLoad();
    }

    public virtual void PreLoad() {}
    
    public virtual void PostLoad() {}

    public abstract string ContentDir();

    public abstract TOutput Load(ResourceKey key, TJson json);
}
