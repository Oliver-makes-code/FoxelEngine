using System;
using System.Collections.Generic;
using System.IO;
using GlmSharp;
using Newtonsoft.Json;
using Voxel.Core.Assets;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.Texture;

public class AtlasLoader {
    private static readonly JsonSerializer Serializer = new();

    public static void LoadAtlas(AssetReader reader, Atlas target, RenderSystem renderSystem) {
        if (target.Id.Group != reader.Group)
            return;

        foreach (var (_, stream, _) in reader.LoadAll($"textures/atlases/{target.Id.Value.ToLower()}", ".json")) {
            using var sr = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(sr);

            var entries = Serializer.Deserialize<AtlasJsonEntry[]>(jsonTextReader);

            foreach (var entry in entries) {
                if (entry.Source == null)
                    throw new InvalidOperationException("Atlas entries must have a source file specified");

                if (!renderSystem.TextureManager.TryGetTextureAndSet($"textures/atlases/{target.Id.Value.ToLower()}/{entry.Source}", out var texture, out var set))
                    throw new InvalidOperationException($"Texture 'textures/atlases/{target.Id.Value.ToLower()}/{entry.Source}' not found");

                //If no sprite is specified, use the entire file as the sprite.
                entry.Sprites ??= [
                    new() {
                        X = 0,
                        Y = 0,
                        Width = (int)texture.Width,
                        Height = (int)texture.Height,
                        Name = string.Empty
                    }
                ];

                foreach (var sprite in entry.Sprites) {
                    if (sprite.X == null || sprite.Y == null)
                        throw new InvalidOperationException("X and Y position of sprite must be specified!");

                    var finalName = sprite.Name == string.Empty ? target.Id.Value.ToLower() : $"{target.Id.Value.ToLower()}/{sprite.Name}";
                    target.StitchTexture(finalName, texture, set, new ivec2(sprite.X ?? 0, sprite.Y ?? 0), new ivec2(sprite.Width ?? 16, sprite.Height ?? 16));
                }
            }
        }

        target.GenerateMipmaps();
        renderSystem.MainCommandList.SetFramebuffer(renderSystem.GraphicsDevice.SwapchainFramebuffer);
    }
    
    
    private class AtlasJsonEntry {
        public string? Source { get; set; }
        public AtlasJsonSprite[]? Sprites { get; set; }
    }

    private class AtlasJsonSprite {
        public string Name { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
