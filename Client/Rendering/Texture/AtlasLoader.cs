using System;
using System.IO;
using GlmSharp;
using Newtonsoft.Json;
using Foxel.Core.Assets;
using Foxel.Core.Rendering;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization.Reader;
using Greenhouse.Libs.Serialization;

namespace Foxel.Client.Rendering.Texture;

public class AtlasLoader {
    public static ReloadableDependency<Atlas> CreateDependency(ResourceKey id, VoxelClient client)
        => new(async (packs, renderSystem, buffer) => {
            await renderSystem.TextureManager.ReloadTask;
            
            lock (renderSystem.ShaderManager.ReloadTask) {
                var atlas = new Atlas(id, renderSystem);

                LoadAtlas(packs, atlas, renderSystem);

                return atlas;
            }
        }, client);

    public static void LoadAtlas(PackManager packs, Atlas target, RenderSystem renderSystem) {
        var metaPath = target.Id.PrefixValue($"atlases/").SuffixValue(".json");
        
        foreach (var s in packs.OpenStream(AssetType.Assets, metaPath)) {
            using var stream = s;
            using var sr = new StreamReader(stream);
            using var jr = new JsonTextReader(sr);
            var reader = new JsonDataReader(jr);

            var json = NewAtlasJson.Codec.ReadGeneric(reader);

            if (json.BulkIncludePath != null && json.BulkIncludePath != string.Empty) {
                foreach (var tex in renderSystem.TextureManager.SearchTextures($"{json.BulkIncludePath}/")) {
                    if (!renderSystem.TextureManager.TryGetTextureAndSet(tex, out var texture, out var set))
                        throw new InvalidOperationException($"Texture '{tex}' not found");
                    var finalName = tex.WithValue(tex.Value.Replace("textures/", "").Replace(".png", ""));

                    var sprite = target.StitchTexture(finalName, texture, set, new ivec2(0, 0), new ivec2((int)texture.Width, (int)texture.Height));
                }
            }

            if (json.Files == null)
                return;

            var entries = json.Files;

            foreach (var entry in entries) {
                var imageId = entry.Source;
                var imagePath = imageId.PrefixValue("textures/").SuffixValue(".png");

                if (!renderSystem.TextureManager.TryGetTextureAndSet(imagePath, out var texture, out var set))
                    throw new InvalidOperationException($"Texture '{imagePath}' not found");
                
                //If no sprite is specified, use the entire file as the sprite.
                var sprites = entry.Sprites ?? [
                    new(null, 0, 0, (int)texture.Width, (int)texture.Height)
                ];

                foreach (var sprite in sprites) {
                    var finalName = sprite.Name == null ? imageId : imageId.SuffixValue($"/{sprite.Name}");
                    
                    target.StitchTexture(finalName, texture, set, new ivec2(sprite.X, sprite.Y), new ivec2(sprite.Width ?? 16, sprite.Height ?? 16));
                }
            }
        }

        target.GenerateMipmaps();
        renderSystem.MainCommandList.SetFramebuffer(renderSystem.GraphicsDevice.SwapchainFramebuffer);
    }

    private record NewAtlasJson(
        string? BulkIncludePath,
        NewAtlasFileEntry[]? Files
    ) {
        public static readonly Codec<NewAtlasJson> Codec = RecordCodec<NewAtlasJson>.Create(
            Codecs.String.NullableField<string, NewAtlasJson>("bulk_include_path", it => it.BulkIncludePath),
            NewAtlasFileEntry.Codec.Array().NullableField<NewAtlasFileEntry[], NewAtlasJson>("files", it => it.Files),
            (bulk, files) => new(bulk, files)
        );
    }

    private record NewAtlasFileEntry(
        ResourceKey Source,
        NewAtlasSprite[]? Sprites
    ) {
        public static readonly Codec<NewAtlasFileEntry> Codec = RecordCodec<NewAtlasFileEntry>.Create(
            ResourceKey.Codec.Field<NewAtlasFileEntry>("source", it => it.Source),
            NewAtlasSprite.Codec.Array().NullableField<NewAtlasSprite[], NewAtlasFileEntry>("sprites", it => it.Sprites),
            (source, sprites) => new(source, sprites)
        );
    }

    private record NewAtlasSprite(
        string? Name,
        int X,
        int Y,
        int? Width,
        int? Height
    ) {
        public static readonly Codec<NewAtlasSprite> Codec = RecordCodec<NewAtlasSprite>.Create(
            Codecs.String.NullableField<string, NewAtlasSprite>("name", it => it.Name),
            Codecs.Int.Field<NewAtlasSprite>("x", it => it.X),
            Codecs.Int.Field<NewAtlasSprite>("y", it => it.Y),
            Codecs.Int.NullableField<int, NewAtlasSprite>("width", it => it.Width),
            Codecs.Int.NullableField<int, NewAtlasSprite>("height", it => it.Height),
            (name, x, y, width, height) => new(name, x, y, width, height)
        );
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
