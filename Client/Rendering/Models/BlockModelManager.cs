using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GlmSharp;
using Newtonsoft.Json;
using RenderSurface.Assets;
using Voxel.Client.Rendering.Texture;
using Voxel.Common.Tile;
using Voxel.Rendering.Utils;

namespace Voxel.Client.Rendering.Models;

public static class BlockModelManager {
    private const float BlueTintAmount = 0.95f;
    private const string Suffix = ".json";
    
    private static readonly string Prefix = Path.Combine("models", "block");

    private static readonly Dictionary<Block, BlockModel> Models = new();

    private static readonly JsonSerializer Serializer = new();
    
    private static readonly vec3 LightColor = new(BlueTintAmount, BlueTintAmount, 1);
    private static readonly vec4 LeftColor = new(ColorFunctions.GetColorMultiplier(0.8f, LightColor), 1);
    private static readonly vec4 RightColor = new(ColorFunctions.GetColorMultiplier(0.77f, LightColor), 1);
    private static readonly vec4 BottomColor = new(ColorFunctions.GetColorMultiplier(0.6f, LightColor), 1);
    private static readonly vec4 BackwardColor = new(ColorFunctions.GetColorMultiplier(0.7f, LightColor), 1);
    private static readonly vec4 ForwardColor = new(ColorFunctions.GetColorMultiplier(0.67f, LightColor), 1);

    public static void RegisterModel(Block block, BlockModel model) => Models[block] = model;
    public static bool TryGetModel(Block block, [NotNullWhen(true)] out BlockModel? model) => Models.TryGetValue(block, out model);

    public static BlockModel GetDefault(Atlas.Sprite sprite) {
        return new BlockModel.Builder()
            //Left
            .AddVertex(0, new(new(0, 0, 0), LeftColor, sprite.GetTrueUV(new vec2(0, 1)), new(0, 0)))
            .AddVertex(0, new(new(0, 0, 1), LeftColor, sprite.GetTrueUV(new vec2(1, 1)), new(1, 0)))
            .AddVertex(0, new(new(0, 1, 1), LeftColor, sprite.GetTrueUV(new vec2(1, 0)), new(1, 1)))
            .AddVertex(0, new(new(0, 1, 0), LeftColor, sprite.GetTrueUV(new vec2(0, 0)), new(0, 1)))

            //Right
            .AddVertex(1, new(new(1, 0, 0), RightColor, sprite.GetTrueUV(new vec2(1, 1)), new(0, 0)))
            .AddVertex(1, new(new(1, 1, 0), RightColor, sprite.GetTrueUV(new vec2(1, 0)), new(1, 0)))
            .AddVertex(1, new(new(1, 1, 1), RightColor, sprite.GetTrueUV(new vec2(0, 0)), new(1, 1)))
            .AddVertex(1, new(new(1, 0, 1), RightColor, sprite.GetTrueUV(new vec2(0, 1)), new(0, 1)))

            //Bottom
            .AddVertex(2, new(new(0, 0, 0), BottomColor, sprite.GetTrueUV(new vec2(0, 1)), new(0, 0)))
            .AddVertex(2, new(new(1, 0, 0), BottomColor, sprite.GetTrueUV(new vec2(1, 1)), new(1, 0)))
            .AddVertex(2, new(new(1, 0, 1), BottomColor, sprite.GetTrueUV(new vec2(1, 0)), new(1, 1)))
            .AddVertex(2, new(new(0, 0, 1), BottomColor, sprite.GetTrueUV(new vec2(0, 0)), new(0, 1)))

            //Top
            .AddVertex(3, new(new(0, 1, 0), vec4.Ones, sprite.GetTrueUV(new vec2(0, 0)), new(0, 0)))
            .AddVertex(3, new(new(0, 1, 1), vec4.Ones, sprite.GetTrueUV(new vec2(1, 0)), new(1, 0)))
            .AddVertex(3, new(new(1, 1, 1), vec4.Ones, sprite.GetTrueUV(new vec2(1, 1)), new(1, 1)))
            .AddVertex(3, new(new(1, 1, 0), vec4.Ones, sprite.GetTrueUV(new vec2(0, 1)), new(0, 1)))

            //Backward
            .AddVertex(4, new(new(0, 0, 0), BackwardColor, sprite.GetTrueUV(new vec2(1, 1)), new(0, 0)))
            .AddVertex(4, new(new(0, 1, 0), BackwardColor, sprite.GetTrueUV(new vec2(1, 0)), new(1, 0)))
            .AddVertex(4, new(new(1, 1, 0), BackwardColor, sprite.GetTrueUV(new vec2(0, 0)), new(1, 1)))
            .AddVertex(4, new(new(1, 0, 0), BackwardColor, sprite.GetTrueUV(new vec2(0, 1)), new(0, 1)))

            //Forward
            .AddVertex(5, new(new(0, 0, 1), ForwardColor, sprite.GetTrueUV(new vec2(0, 1)), new(0, 0)))
            .AddVertex(5, new(new(1, 0, 1), ForwardColor, sprite.GetTrueUV(new vec2(1, 1)), new(1, 0)))
            .AddVertex(5, new(new(1, 1, 1), ForwardColor, sprite.GetTrueUV(new vec2(1, 0)), new(1, 1)))
            .AddVertex(5, new(new(0, 1, 1), ForwardColor, sprite.GetTrueUV(new vec2(0, 0)), new(0, 1)))
            .Build();
    }
    public static BlockModel GetGrass(Atlas.Sprite top, Atlas.Sprite bottom, Atlas.Sprite side) {
        return new BlockModel.Builder()
            //Left
            .AddVertex(0, new(new(0, 0, 0), LeftColor, side.GetTrueUV(new vec2(0, 1)), new(0, 0)))
            .AddVertex(0, new(new(0, 0, 1), LeftColor, side.GetTrueUV(new vec2(1, 1)), new(1, 0)))
            .AddVertex(0, new(new(0, 1, 1), LeftColor, side.GetTrueUV(new vec2(1, 0)), new(1, 1)))
            .AddVertex(0, new(new(0, 1, 0), LeftColor, side.GetTrueUV(new vec2(0, 0)), new(0, 1)))

            //Right
            .AddVertex(1, new(new(1, 0, 0), RightColor, side.GetTrueUV(new vec2(1, 1)), new(0, 0)))
            .AddVertex(1, new(new(1, 1, 0), RightColor, side.GetTrueUV(new vec2(1, 0)), new(1, 0)))
            .AddVertex(1, new(new(1, 1, 1), RightColor, side.GetTrueUV(new vec2(0, 0)), new(1, 1)))
            .AddVertex(1, new(new(1, 0, 1), RightColor, side.GetTrueUV(new vec2(0, 1)), new(0, 1)))

            //Bottom
            .AddVertex(2, new(new(0, 0, 0), BottomColor, bottom.GetTrueUV(new vec2(0, 1)), new(0, 0)))
            .AddVertex(2, new(new(1, 0, 0), BottomColor, bottom.GetTrueUV(new vec2(1, 1)), new(1, 0)))
            .AddVertex(2, new(new(1, 0, 1), BottomColor, bottom.GetTrueUV(new vec2(1, 0)), new(1, 1)))
            .AddVertex(2, new(new(0, 0, 1), BottomColor, bottom.GetTrueUV(new vec2(0, 0)), new(0, 1)))

            //Top
            .AddVertex(3, new(new(0, 1, 0), vec4.Ones, top.GetTrueUV(new vec2(0, 0)), new(0, 0)))
            .AddVertex(3, new(new(0, 1, 1), vec4.Ones, top.GetTrueUV(new vec2(1, 0)), new(1, 0)))
            .AddVertex(3, new(new(1, 1, 1), vec4.Ones, top.GetTrueUV(new vec2(1, 1)), new(1, 1)))
            .AddVertex(3, new(new(1, 1, 0), vec4.Ones, top.GetTrueUV(new vec2(0, 1)), new(0, 1)))

            //Backward
            .AddVertex(4, new(new(0, 0, 0), BackwardColor, side.GetTrueUV(new vec2(1, 1)), new(0, 0)))
            .AddVertex(4, new(new(0, 1, 0), BackwardColor, side.GetTrueUV(new vec2(1, 0)), new(1, 0)))
            .AddVertex(4, new(new(1, 1, 0), BackwardColor, side.GetTrueUV(new vec2(0, 0)), new(1, 1)))
            .AddVertex(4, new(new(1, 0, 0), BackwardColor, side.GetTrueUV(new vec2(0, 1)), new(0, 1)))

            //Forward
            .AddVertex(5, new(new(0, 0, 1), ForwardColor, side.GetTrueUV(new vec2(0, 1)), new(0, 0)))
            .AddVertex(5, new(new(1, 0, 1), ForwardColor, side.GetTrueUV(new vec2(1, 1)), new(1, 0)))
            .AddVertex(5, new(new(1, 1, 1), ForwardColor, side.GetTrueUV(new vec2(1, 0)), new(1, 1)))
            .AddVertex(5, new(new(0, 1, 1), ForwardColor, side.GetTrueUV(new vec2(0, 0)), new(0, 1)))
            .Build();
    }

    public static void Init(AssetReader reader, Atlas atlas) {
        foreach ((string name, var stream, _) in reader.LoadAll(Prefix, Suffix)) {
            using var sr = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(sr);
            
            string texture = Serializer.Deserialize<ModelJson>(jsonTextReader)?.Texture ?? "";
            int start = Prefix.Length + 1;
            int end = name.Length - Suffix.Length;
            string blockName = name[start..end];

            if (Blocks.GetBlock(blockName, out var block) && atlas.TryGetSprite(texture, out var sprite))
                RegisterModel(block, GetDefault(sprite));
        }
        if (
            atlas.TryGetSprite("main/grass_top", out var top) &&
            atlas.TryGetSprite("main/grass_side", out var side) &&
            atlas.TryGetSprite("main/dirt", out var bottom)
        )
            RegisterModel(Blocks.Grass, GetGrass(top, bottom, side));
    }

    private class ModelJson {
        public string Texture { get; set; }
    }
}
