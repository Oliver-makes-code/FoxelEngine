using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GlmSharp;
using Newtonsoft.Json;
using Foxel.Client.Rendering.Texture;
using Foxel.Common.Content;
using Foxel.Common.Tile;
using Foxel.Core.Util;
using Foxel.Core.Assets;
using System.Linq;
using System.Threading.Tasks;
using Greenhouse.Libs.Serialization;
using Greenhouse.Libs.Serialization.Reader;

namespace Foxel.Client.Rendering.Models;

public static class BlockModelManager {
    public static readonly PackManager.ReloadTask ReloadTask = PackManager.RegisterResourceLoader(AssetType.Assets, Reload);
    public static readonly BlockModel.Builder Builder = new();

    private static readonly Dictionary<ResourceKey, BlockModel> Models = [];
    private static readonly List<BlockModel?> ModelsByRawID = [];

    public static void RegisterModel(ResourceKey name, BlockModel model) => Models[name] = model;
    public static bool TryGetModel(Block block, [NotNullWhen(true)] out BlockModel? model) {
        lock (ModelsByRawID) {
            model = ModelsByRawID[(int)block.id];
            return model != null;
        }
    }

    public static IEnumerable<(ResourceKey, BlockModel)> GetModels() {
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

    private record ModelJson(string? Texture) {
        public static readonly Codec<ModelJson> Codec = RecordCodec<ModelJson>.Create(
            Codecs.String.NullableField<string, ModelJson>("texture", it => it.Texture),
            (tex) => new(tex)
        );
    }
}
