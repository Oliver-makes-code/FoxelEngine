using Newtonsoft.Json;
using Foxel.Core.Assets;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;
using Greenhouse.Libs.Serialization.Reader;

namespace Foxel.Common.World.Content;

public abstract class ServerContentManager<TInput, TOutput> where TOutput : notnull {
    public const AssetType Assets = AssetType.Content;

    public readonly PackManager.ReloadTask ReloadTask;

    private readonly Codec<TInput> Codec;
    private readonly ContentStore<TOutput> Store;

    public ServerContentManager(Codec<TInput> codec, ContentStore<TOutput> store) {
        Codec = codec;
        Store = store;
        ReloadTask = PackManager.RegisterResourceLoader(Assets, Reload);
    }

    public async void Reload(PackManager manager) {
        Store.Clear();
        string contentDir = ContentDir() + "/";
        await PreLoad();
        foreach (var key in manager.ListResources(Assets, prefix: contentDir, suffix: ".json")) {
            var outputKey = key.WithValue(key.Value.Substring(contentDir.Length, key.Value.Length - contentDir.Length - 5));

            using var stream = manager.OpenStream(Assets, key).First();
            using var sr = new StreamReader(stream);
            using var jr = new JsonTextReader(sr);
            var reader = new JsonDataReader(jr);

            var json = Codec.ReadGeneric(reader);

            if (json != null)
                Store.Register(outputKey, Load(outputKey, json));
        }
        Store.Freeze();
        await PostLoad();
    }

    public virtual Task PreLoad()
        => Task.CompletedTask;
    
    public virtual Task PostLoad()
        => Task.CompletedTask;

    public abstract string ContentDir();

    public abstract TOutput Load(ResourceKey key, TInput json);
}
