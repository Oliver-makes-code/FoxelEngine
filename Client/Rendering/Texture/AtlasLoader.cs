using System.Collections.Generic;
using System.IO;
using GlmSharp;
using Newtonsoft.Json;
using RenderSurface.Assets;
using RenderSurface.Rendering;

namespace Voxel.Client.Rendering.Texture;

public class AtlasLoader {

    private static readonly JsonSerializer Serializer = new();
    
    public static void LoadAtlas(AssetReader reader, Atlas target, RenderSystem renderSystem) {
        foreach (var (_, stream, _) in reader.LoadAll(Path.Combine("textures", "atlases", target.Name.ToLower()), ".json")) {
            using var sr = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(sr);

            var jsonObject = Serializer.Deserialize<AtlasJson>(jsonTextReader);

            if (jsonObject == null || jsonObject.TexturePath == null)
                return;

            if (!renderSystem.TextureManager.TryGetTextureAndSet(Path.Combine("textures", "atlases", target.Name.ToLower(), jsonObject.TexturePath), out var texture, out var set))
                return;

            if (jsonObject.Auto != null) {
                var spriteCount = (ivec2)vec2.Floor(new vec2(texture.Width, texture.Height) / jsonObject.Auto.Size);
                var spriteSize = new ivec2(jsonObject.Auto.Size, jsonObject.Auto.Size);

                for (int x = 0; x < spriteCount.x; x++)
                for (int y = 0; y < spriteCount.y; y++) {
                    var spritePos = new ivec2(x, y) * spriteSize;
                    target.StitchTexture($"{target.Name.ToLower()}:{x},{y}", texture, set, spritePos, spriteSize);
                }
            }

            if (jsonObject.Explicit != null && jsonObject.Explicit != null)
                foreach (var entry in jsonObject.Explicit)
                    target.StitchTexture($"{target.Name.ToLower()}/{entry.Name}", texture, set, new ivec2(entry.X, entry.Y), new ivec2(entry.Width, entry.Height));
        }

        target.GenerateMipmaps();
        renderSystem.MainCommandList.SetFramebuffer(renderSystem.GraphicsDevice.SwapchainFramebuffer);
    }

    private class AtlasJson {
        public string? TexturePath { get; set; }

        public AutoAtlas? Auto { get; set; }
        public ExplicitEntry[]? Explicit { get; set; }

        public class ExplicitEntry {
            public string? Name { get; set; }
            public int X { get; set; }
            public int Y { get; set; }

            public int Width { get; set; } = 16;
            public int Height { get; set; } = 16;
        }
    }

    private class AutoAtlas {
        public int Size { get; set; }
    }
}
