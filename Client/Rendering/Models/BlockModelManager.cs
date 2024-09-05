using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Foxel.Common.Content;
using Foxel.Common.Tile;
using Foxel.Core.Util;
using Foxel.Core.Assets;
using System.Threading.Tasks;

namespace Foxel.Client.Rendering.Models;

public static class BlockModelManager {
    public static readonly PackManager.ReloadTask ReloadTask = PackManager.RegisterResourceLoader(AssetType.Assets, Reload);
    public static readonly BakedModel.Builder Builder = new();

    private static readonly Dictionary<ResourceKey, BakedModel> Models = [];
    private static readonly List<BakedModel?> ModelsByRawID = [];

    public static void RegisterModel(ResourceKey name, BakedModel model) => Models[name] = model;
    public static bool TryGetModel(Block block, [NotNullWhen(true)] out BakedModel? model) {
        lock (ModelsByRawID) {
            model = ModelsByRawID[(int)block.id];
            return model != null;
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

        foreach (var (block, key, id) in ContentDatabase.Instance.Registries.Blocks.Entries()) {
            var modelKey = key.PrefixValue("block/");

            if (ModelManager.TryGetModel(modelKey, out var model)) {
                lock (Builder) {
                    Builder.Clear();
                    ModelManager.EmitVertices(model, atlas, Builder);
                    RegisterModel(key, Builder.Build());
                }
            }
        }

        BakeRawBlockModels();
    }


    public static void BakeRawBlockModels() {
        lock (ModelsByRawID) {
            ModelsByRawID.Clear();

            foreach ((var entry, ResourceKey id, uint raw) in ContentDatabase.Instance.Registries.Blocks.Entries()) {
                if (Models.TryGetValue(id, out var mdl))
                    ModelsByRawID.Add(mdl);
                else
                    ModelsByRawID.Add(null);
            }
        }
    }
}
