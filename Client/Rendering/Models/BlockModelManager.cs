using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GlmSharp;
using Voxel.Client.Rendering.Texture;
using Voxel.Common.Tile;

namespace Voxel.Client.Rendering.Models;

public static class BlockModelManager {

    private static readonly Dictionary<Block, BlockModel> Models = new();

    public static void RegisterModel(Block block, BlockModel model) => Models[block] = model;
    public static bool TryGetModel(Block block, [NotNullWhen(true)] out BlockModel? model) => Models.TryGetValue(block, out model);

    public static BlockModel GetDefault(Atlas.Sprite sprite) {
        return new BlockModel.Builder()
            //Left
            .AddVertex(0, new(new(0, 0, 0), vec4.Ones * 0.8f, sprite.GetTrueUV(new vec2(0, 1))))
            .AddVertex(0, new(new(0, 0, 1), vec4.Ones * 0.8f, sprite.GetTrueUV(new vec2(1, 1))))
            .AddVertex(0, new(new(0, 1, 1), vec4.Ones * 0.8f, sprite.GetTrueUV(new vec2(1, 0))))
            .AddVertex(0, new(new(0, 1, 0), vec4.Ones * 0.8f, sprite.GetTrueUV(new vec2(0, 0))))

            //Right
            .AddVertex(1, new(new(1, 0, 0), vec4.Ones * 0.77f, sprite.GetTrueUV(new vec2(1, 1))))
            .AddVertex(1, new(new(1, 1, 0), vec4.Ones * 0.77f, sprite.GetTrueUV(new vec2(1, 0))))
            .AddVertex(1, new(new(1, 1, 1), vec4.Ones * 0.77f, sprite.GetTrueUV(new vec2(0, 0))))
            .AddVertex(1, new(new(1, 0, 1), vec4.Ones * 0.77f, sprite.GetTrueUV(new vec2(0, 1))))

            //Bottom
            .AddVertex(2, new(new(0, 0, 0), vec4.Ones * 0.6f, sprite.GetTrueUV(new vec2(0, 1))))
            .AddVertex(2, new(new(1, 0, 0), vec4.Ones * 0.6f, sprite.GetTrueUV(new vec2(1, 1))))
            .AddVertex(2, new(new(1, 0, 1), vec4.Ones * 0.6f, sprite.GetTrueUV(new vec2(1, 0))))
            .AddVertex(2, new(new(0, 0, 1), vec4.Ones * 0.6f, sprite.GetTrueUV(new vec2(0, 0))))

            //Top
            .AddVertex(3, new(new(0, 1, 0), vec4.Ones, sprite.GetTrueUV(new vec2(0, 0))))
            .AddVertex(3, new(new(0, 1, 1), vec4.Ones, sprite.GetTrueUV(new vec2(1, 0))))
            .AddVertex(3, new(new(1, 1, 1), vec4.Ones, sprite.GetTrueUV(new vec2(1, 1))))
            .AddVertex(3, new(new(1, 1, 0), vec4.Ones, sprite.GetTrueUV(new vec2(0, 1))))

            //Backward
            .AddVertex(4, new(new(0, 0, 0), vec4.Ones * 0.7f, sprite.GetTrueUV(new vec2(1, 1))))
            .AddVertex(4, new(new(0, 1, 0), vec4.Ones * 0.7f, sprite.GetTrueUV(new vec2(1, 0))))
            .AddVertex(4, new(new(1, 1, 0), vec4.Ones * 0.7f, sprite.GetTrueUV(new vec2(0, 0))))
            .AddVertex(4, new(new(1, 0, 0), vec4.Ones * 0.7f, sprite.GetTrueUV(new vec2(0, 1))))

            //Forward
            .AddVertex(5, new(new(0, 0, 1), vec4.Ones * 0.67f, sprite.GetTrueUV(new vec2(0, 1))))
            .AddVertex(5, new(new(1, 0, 1), vec4.Ones * 0.67f, sprite.GetTrueUV(new vec2(1, 1))))
            .AddVertex(5, new(new(1, 1, 1), vec4.Ones * 0.67f, sprite.GetTrueUV(new vec2(1, 0))))
            .AddVertex(5, new(new(0, 1, 1), vec4.Ones * 0.67f, sprite.GetTrueUV(new vec2(0, 0))))
            .Build();
    }

    public static void Init(Atlas atlas) {
        Atlas.Sprite? sprite;
        if (atlas.TryGetSprite("main/stone", out sprite))
            RegisterModel(Blocks.Stone, GetDefault(sprite));
        if (atlas.TryGetSprite("main/dirt", out sprite))
            RegisterModel(Blocks.Dirt, GetDefault(sprite));
        if (atlas.TryGetSprite("main/grass", out sprite))
            RegisterModel(Blocks.Grass, GetDefault(sprite));
    }
}
