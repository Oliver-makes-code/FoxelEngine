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

    private const float BlueTintAmount = 1f;
    private const string Suffix = ".json";
    private const string Prefix = "models/block/";

    private static readonly Dictionary<ResourceKey, BlockModel> Models = [];
    private static readonly List<BlockModel?> ModelsByRawID = [];

    private static readonly vec3 LightColor = new(0.95f, 0.95f, 1f);
    private static readonly vec3 LeftColor = new(0.8f * LightColor);
    private static readonly vec3 RightColor = new(0.77f * LightColor);
    private static readonly vec3 BottomColor = new(0.6f * LightColor);
    private static readonly vec3 BackwardColor = new(0.7f * LightColor);
    private static readonly vec3 ForwardColor = new(0.67f * LightColor);

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

    public static BlockModel GetDefault(Atlas.Sprite sprite) {
        return new BlockModel.Builder()
            //Left
            .AddVertex(0, new(new(0, 0, 0), LeftColor, sprite.GetTrueUV(new vec2(0, 1)), new(0, 0), sprite))
            .AddVertex(0, new(new(0, 0, 1), LeftColor, sprite.GetTrueUV(new vec2(1, 1)), new(1, 0), sprite))
            .AddVertex(0, new(new(0, 1, 1), LeftColor, sprite.GetTrueUV(new vec2(1, 0)), new(1, 1), sprite))
            .AddVertex(0, new(new(0, 1, 0), LeftColor, sprite.GetTrueUV(new vec2(0, 0)), new(0, 1), sprite))

            //Right
            .AddVertex(1, new(new(1, 0, 0), RightColor, sprite.GetTrueUV(new vec2(1, 1)), new(0, 0), sprite))
            .AddVertex(1, new(new(1, 1, 0), RightColor, sprite.GetTrueUV(new vec2(1, 0)), new(1, 0), sprite))
            .AddVertex(1, new(new(1, 1, 1), RightColor, sprite.GetTrueUV(new vec2(0, 0)), new(1, 1), sprite))
            .AddVertex(1, new(new(1, 0, 1), RightColor, sprite.GetTrueUV(new vec2(0, 1)), new(0, 1), sprite))

            //Bottom
            .AddVertex(2, new(new(0, 0, 0), BottomColor, sprite.GetTrueUV(new vec2(0, 1)), new(0, 0), sprite))
            .AddVertex(2, new(new(1, 0, 0), BottomColor, sprite.GetTrueUV(new vec2(1, 1)), new(1, 0), sprite))
            .AddVertex(2, new(new(1, 0, 1), BottomColor, sprite.GetTrueUV(new vec2(1, 0)), new(1, 1), sprite))
            .AddVertex(2, new(new(0, 0, 1), BottomColor, sprite.GetTrueUV(new vec2(0, 0)), new(0, 1), sprite))

            //Top
            .AddVertex(3, new(new(0, 1, 0), vec3.Ones, sprite.GetTrueUV(new vec2(0, 0)), new(0, 0), sprite))
            .AddVertex(3, new(new(0, 1, 1), vec3.Ones, sprite.GetTrueUV(new vec2(1, 0)), new(1, 0), sprite))
            .AddVertex(3, new(new(1, 1, 1), vec3.Ones, sprite.GetTrueUV(new vec2(1, 1)), new(1, 1), sprite))
            .AddVertex(3, new(new(1, 1, 0), vec3.Ones, sprite.GetTrueUV(new vec2(0, 1)), new(0, 1), sprite))

            //Backward
            .AddVertex(4, new(new(0, 0, 0), BackwardColor, sprite.GetTrueUV(new vec2(1, 1)), new(0, 0), sprite))
            .AddVertex(4, new(new(0, 1, 0), BackwardColor, sprite.GetTrueUV(new vec2(1, 0)), new(1, 0), sprite))
            .AddVertex(4, new(new(1, 1, 0), BackwardColor, sprite.GetTrueUV(new vec2(0, 0)), new(1, 1), sprite))
            .AddVertex(4, new(new(1, 0, 0), BackwardColor, sprite.GetTrueUV(new vec2(0, 1)), new(0, 1), sprite))

            //Forward
            .AddVertex(5, new(new(0, 0, 1), ForwardColor, sprite.GetTrueUV(new vec2(0, 1)), new(0, 0), sprite))
            .AddVertex(5, new(new(1, 0, 1), ForwardColor, sprite.GetTrueUV(new vec2(1, 1)), new(1, 0), sprite))
            .AddVertex(5, new(new(1, 1, 1), ForwardColor, sprite.GetTrueUV(new vec2(1, 0)), new(1, 1), sprite))
            .AddVertex(5, new(new(0, 1, 1), ForwardColor, sprite.GetTrueUV(new vec2(0, 0)), new(0, 1), sprite))
            .Build();
    }
    public static BlockModel GetGrass(Atlas.Sprite top, Atlas.Sprite bottom, Atlas.Sprite side) {
        return new BlockModel.Builder()
            //Left
            .AddVertex(0, new(new(0, 0, 0), LeftColor, side.GetTrueUV(new vec2(0, 1)), new(0, 0), side))
            .AddVertex(0, new(new(0, 0, 1), LeftColor, side.GetTrueUV(new vec2(1, 1)), new(1, 0), side))
            .AddVertex(0, new(new(0, 1, 1), LeftColor, side.GetTrueUV(new vec2(1, 0)), new(1, 1), side))
            .AddVertex(0, new(new(0, 1, 0), LeftColor, side.GetTrueUV(new vec2(0, 0)), new(0, 1), side))

            //Right
            .AddVertex(1, new(new(1, 0, 0), RightColor, side.GetTrueUV(new vec2(1, 1)), new(0, 0), side))
            .AddVertex(1, new(new(1, 1, 0), RightColor, side.GetTrueUV(new vec2(1, 0)), new(1, 0), side))
            .AddVertex(1, new(new(1, 1, 1), RightColor, side.GetTrueUV(new vec2(0, 0)), new(1, 1), side))
            .AddVertex(1, new(new(1, 0, 1), RightColor, side.GetTrueUV(new vec2(0, 1)), new(0, 1), side))

            //Bottom
            .AddVertex(2, new(new(0, 0, 0), BottomColor, bottom.GetTrueUV(new vec2(0, 1)), new(0, 0), bottom))
            .AddVertex(2, new(new(1, 0, 0), BottomColor, bottom.GetTrueUV(new vec2(1, 1)), new(1, 0), bottom))
            .AddVertex(2, new(new(1, 0, 1), BottomColor, bottom.GetTrueUV(new vec2(1, 0)), new(1, 1), bottom))
            .AddVertex(2, new(new(0, 0, 1), BottomColor, bottom.GetTrueUV(new vec2(0, 0)), new(0, 1), bottom))

            //Top
            .AddVertex(3, new(new(0, 1, 0), vec3.Ones, top.GetTrueUV(new vec2(0, 0)), new(0, 0), top))
            .AddVertex(3, new(new(0, 1, 1), vec3.Ones, top.GetTrueUV(new vec2(1, 0)), new(1, 0), top))
            .AddVertex(3, new(new(1, 1, 1), vec3.Ones, top.GetTrueUV(new vec2(1, 1)), new(1, 1), top))
            .AddVertex(3, new(new(1, 1, 0), vec3.Ones, top.GetTrueUV(new vec2(0, 1)), new(0, 1), top))

            //Backward
            .AddVertex(4, new(new(0, 0, 0), BackwardColor, side.GetTrueUV(new vec2(1, 1)), new(0, 0), side))
            .AddVertex(4, new(new(0, 1, 0), BackwardColor, side.GetTrueUV(new vec2(1, 0)), new(1, 0), side))
            .AddVertex(4, new(new(1, 1, 0), BackwardColor, side.GetTrueUV(new vec2(0, 0)), new(1, 1), side))
            .AddVertex(4, new(new(1, 0, 0), BackwardColor, side.GetTrueUV(new vec2(0, 1)), new(0, 1), side))

            //Forward
            .AddVertex(5, new(new(0, 0, 1), ForwardColor, side.GetTrueUV(new vec2(0, 1)), new(0, 0), side))
            .AddVertex(5, new(new(1, 0, 1), ForwardColor, side.GetTrueUV(new vec2(1, 1)), new(1, 0), side))
            .AddVertex(5, new(new(1, 1, 1), ForwardColor, side.GetTrueUV(new vec2(1, 0)), new(1, 1), side))
            .AddVertex(5, new(new(0, 1, 1), ForwardColor, side.GetTrueUV(new vec2(0, 0)), new(0, 1), side))
            .Build();
    }

    public static async Task Reload(PackManager manager) {
        await VoxelClient.instance!.gameRenderer!.ReloadTask;
        await VoxelClient.instance!.gameRenderer!.WorldRenderer.ChunkRenderer.TerrainAtlas.ReloadTask;
        var atlas = VoxelClient.instance!.gameRenderer!.WorldRenderer.ChunkRenderer.TerrainAtlas.value!;
        Models.Clear();
        foreach (var resource in manager.ListResources(AssetType.Assets, Prefix, Suffix)) {
            using var stream = manager.OpenStream(AssetType.Assets, resource).First();
            using var sr = new StreamReader(stream);
            using var jr = new JsonTextReader(sr);
            var reader = new JsonDataReader(jr);
            string texture = ModelJson.Codec.ReadGeneric(reader).Texture ?? "";

            int start = Prefix.Length;
            int end = resource.Value.Length - Suffix.Length;
            string name = resource.Value[start..end];
            var blockName = new ResourceKey(resource.Group, name);

            if (atlas.TryGetSprite(new(texture), out var sprite))
                RegisterModel(blockName, GetDefault(sprite));
        }
        if (
            atlas.TryGetSprite(new("terrain/grass_top"), out var top) &&
            atlas.TryGetSprite(new("terrain/grass_side"), out var side) &&
            atlas.TryGetSprite(new("terrain/dirt"), out var bottom)
        )
            RegisterModel(new("grass"), GetGrass(top, bottom, side));
        
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
