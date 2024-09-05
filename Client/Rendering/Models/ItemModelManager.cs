using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Foxel.Core.Util;
using Foxel.Core.Assets;
using System.Threading.Tasks;
using Foxel.Common.World.Content;

namespace Foxel.Client.Rendering.Models;

public static class ItemModelManager {
    public static readonly PackManager.ReloadTask ReloadTask = PackManager.RegisterResourceLoader(AssetType.Assets, Reload);
    public static readonly BakedModel.Builder Builder = new();

    private static readonly Dictionary<ResourceKey, BakedModel> Models = [];
    private static readonly List<BakedModel?> ModelsByRawID = [];

    public static void RegisterModel(ResourceKey name, BakedModel model) => Models[name] = model;
    public static bool TryGetModel(ResourceKey key, [NotNullWhen(true)] out BakedModel? model) {
        lock (Models) {
            return Models.TryGetValue(key, out model);
        }
    }

    public static IEnumerable<(ResourceKey, BakedModel)> GetModels() {
        foreach (var model in Models.Keys)
            yield return (model, Models[model]);
    }

    public static async Task Reload(PackManager manager) {
        // Wait fro dependencies
        await VoxelClient.instance!.gameRenderer!.ReloadTask;
        await VoxelClient.instance!.gameRenderer!.WorldRenderer.ChunkRenderer.TerrainAtlas.ReloadTask;
        await ModelManager.ReloadTask;
        var atlas = VoxelClient.instance!.gameRenderer!.WorldRenderer.ChunkRenderer.TerrainAtlas.value!;
        Models.Clear();

        foreach (var key in ContentStores.Items.Keys()) {
            var modelKey = key.PrefixValue("item/");

            if (ModelManager.TryGetModel(modelKey, out var model)) {
                lock (Builder) {
                    Builder.Clear();
                    ModelManager.EmitVertices(model, atlas, Builder);
                    RegisterModel(key, Builder.Build());
                }
            }
        }
    }
}
