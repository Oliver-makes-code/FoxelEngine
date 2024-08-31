using GlmSharp;
using Foxel.Client.Rendering.Models;
using Foxel.Client.Rendering.Texture;
using Foxel.Core.Util;

namespace Foxel.Client.Rendering.Gui;

public static class GuiAtlasLoader {
    public static ReloadableDependency<Atlas> CreateDependency(ResourceKey id, VoxelClient client)
        => new(async (packs, renderSystem, buffer) => {
            await renderSystem.TextureManager.ReloadTask;
            var modelTextureizer = client.modelTextureizer!;
            await modelTextureizer.ReloadTask;
            await BlockModelManager.ReloadTask;
            
            lock (renderSystem.ShaderManager.ReloadTask) {
                var atlas = new Atlas(id, renderSystem);

                AtlasLoader.LoadAtlas(packs, atlas, renderSystem);

                foreach (var (id, model) in BlockModelManager.GetModels()) {
                    modelTextureizer.Textureize(model, quat.Identity.Rotated(float.Pi/6, vec3.UnitX).Rotated(float.Pi/4, vec3.UnitY));
                    atlas.StitchTexture(id.PrefixValue("model/"), modelTextureizer.ColorTexture, modelTextureizer.TextureSet, ivec2.Zero, ModelTextureizer.Size);
                }

                return atlas;
            }
        }, client);
}
