using System;
using System.IO;
using System.Threading.Tasks;
using GlmSharp;
using Newtonsoft.Json;
using Voxel.Core;
using Voxel.Core.Assets;
using Voxel.Core.Rendering;
using Voxel.Core.Util;

namespace Voxel.Client.Rendering.Texture;

public class AtlasLoader {
    private static readonly JsonSerializer Serializer = new();

    public static ReloadableDependency<Atlas> CreateDependency(ResourceKey id)
        => new((packs, renderSystem, buffer) => {
            Task.Run(async () => await renderSystem.TextureManager.ReloadTask).Wait();
            
            lock (renderSystem.ShaderManager.ReloadTask) {
                var atlas = new Atlas(id, renderSystem);

                LoadAtlas(packs, atlas, renderSystem);

                return atlas;
            }
        });

    private static void LoadAtlas(PackManager packs, Atlas target, RenderSystem renderSystem) {
        var metaPath = target.Id.PrefixValue($"atlases/").SuffixValue(".json");
        
        foreach (var s in packs.OpenStream(AssetType.Assets, metaPath)) {
            using var stream = s;
            using var sr = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(sr);

            var json = Serializer.Deserialize<AtlasJson>(jsonTextReader) ?? new();

            if (json.bulkIncludePath != null && json.bulkIncludePath != string.Empty) {
                foreach (var tex in renderSystem.TextureManager.SearchTextures($"{json.bulkIncludePath}/")) {
                    if (!renderSystem.TextureManager.TryGetTextureAndSet(tex, out var texture, out var set))
                        throw new InvalidOperationException($"Texture '{tex}' not found");
                    var finalName = tex.WithValue(tex.Value.Replace("textures/", "").Replace(".png", ""));

                    var sprite = target.StitchTexture(finalName, texture, set, new ivec2(0, 0), new ivec2((int)texture.Width, (int)texture.Height));
                }
            }

            var entries = json.files ?? [];

            foreach (var entry in entries) {
                if (entry.source == null)
                    continue;
                
                var imageId = new ResourceKey(entry.source);
                var imagePath = imageId.PrefixValue("textures/").SuffixValue(".png");

                if (!renderSystem.TextureManager.TryGetTextureAndSet(imagePath, out var texture, out var set))
                    throw new InvalidOperationException($"Texture '{imagePath}' not found");
                
                //If no sprite is specified, use the entire file as the sprite.
                entry.sprites ??= [
                    new() {
                        x = 0,
                        y = 0,
                        width = (int)texture.Width,
                        height = (int)texture.Height,
                        name = string.Empty
                    }
                ];

                foreach (var sprite in entry.sprites) {
                    sprite.x ??= 0;
                    sprite.y ??= 0;

                    var finalName = sprite.name == string.Empty || sprite.name == null ? imageId : imageId.SuffixValue($"/{sprite.name}");
                    
                    target.StitchTexture(finalName, texture, set, new ivec2(sprite.x ?? 0, sprite.y ?? 0), new ivec2(sprite.width ?? 16, sprite.height ?? 16));
                }
            }
        }

        target.GenerateMipmaps();
        renderSystem.MainCommandList.SetFramebuffer(renderSystem.GraphicsDevice.SwapchainFramebuffer);
    }
    
    private class AtlasJson {
        public string? bulkIncludePath { get; set; }
        public AtlasFileEntry[]? files { get; set; }
    }

    private class AtlasFileEntry {
        public string? source { get; set; }
        public AtlasJsonSprite[]? sprites { get; set; }
    }

    private class AtlasJsonSprite {
        public string? name { get; set; }
        public int? x { get; set; }
        public int? y { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
    }
}
