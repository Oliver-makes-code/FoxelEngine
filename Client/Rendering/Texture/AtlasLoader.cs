using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GlmSharp;
using Newtonsoft.Json;
using Voxel.Core.Assets;
using Voxel.Core.Rendering;
using Voxel.Core.Util;

namespace Voxel.Client.Rendering.Texture;

public class AtlasLoader {
    private static readonly JsonSerializer Serializer = new();

    public static ReloadableDependency<Atlas> CreateDependency(ResourceKey id)
        => new((packs, renderSystem, buffer) => {
            Task.Run(async () => await renderSystem.TextureManager.ReloadTask).Wait();
            
            var atlas = new Atlas(id, renderSystem);

            LoadAtlas(packs, atlas, renderSystem);

            return atlas;
        });

    private static void LoadAtlas(PackManager packs, Atlas target, RenderSystem renderSystem) {
        var metaPath = target.Id.PrefixValue($"atlases/").SuffixValue(".json");
        
        foreach (var s in packs.OpenStream(AssetType.Assets, metaPath)) {
            using var stream = s;
            using var sr = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(sr);

            var entries = Serializer.Deserialize<AtlasJsonEntry[]>(jsonTextReader) ?? [];

            foreach (var entry in entries) {
                if (entry.Source == null)
                    continue;
                
                var imageId = new ResourceKey(entry.Source);
                var imagePath = imageId.PrefixValue("textures/").SuffixValue(".png");

                if (!renderSystem.TextureManager.TryGetTextureAndSet(imagePath.ToString(), out var texture, out var set))
                    throw new InvalidOperationException($"Texture '{imagePath}' not found");
                
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
                    sprite.X ??= 0;
                    sprite.Y ??= 0;

                    var finalName = sprite.Name == string.Empty || sprite.Name == null ? imageId : new ResourceKey(sprite.Name);
                    
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
        public string? Name { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
