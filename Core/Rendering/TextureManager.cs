using System.Diagnostics.CodeAnalysis;
using Veldrid;
using Veldrid.ImageSharp;
using Foxel.Core.Assets;
using Foxel.Core.Util;

namespace Foxel.Core.Rendering;

public class TextureManager {
    public readonly PackManager.ReloadTask ReloadTask;
    
    public readonly ResourceLayout TextureResourceLayout;

    public readonly RenderSystem RenderSystem;

    private readonly Dictionary<ResourceKey, Texture> LoadedTextures = [];
    private readonly Dictionary<ResourceKey, ResourceSet> TextureSets = [];
    private readonly List<ResourceKey> TextureKeys = [];

    public TextureManager(RenderSystem renderSystem, PackManager packs) {
        RenderSystem = renderSystem;

        TextureResourceLayout = RenderSystem.ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment | ShaderStages.Vertex),
            new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment | ShaderStages.Vertex)
        ));

        ReloadTask = PackManager.RegisterResourceLoader(AssetType.Assets, Reload);
    }

    public IEnumerable<ResourceKey> SearchTextures(string prefix = "", string suffix = "") {
        string pre = $"textures/{prefix}";
        string post = $"{suffix}.png";

        foreach (var texture in TextureKeys)
            if (texture.Value.StartsWith(pre) && texture.Value.EndsWith(post))
                yield return texture;
    }

    public bool TryGetTexture(ResourceKey path, [NotNullWhen(true)] out Texture? texture)
        => LoadedTextures.TryGetValue(path, out texture);

    public bool TryGetTextureResourceSet(ResourceKey path, [NotNullWhen(true)] out ResourceSet? textureSet)
        => TextureSets.TryGetValue(path, out textureSet);

    public bool TryGetTextureAndSet(ResourceKey path, [NotNullWhen(true)] out Texture? texture, [NotNullWhen(true)] out ResourceSet? textureSet) {
        textureSet = null;
        return LoadedTextures.TryGetValue(path, out texture) && TextureSets.TryGetValue(path, out textureSet);
    }

    public ResourceSet CreateTextureResourceSet(Texture texture) => RenderSystem.ResourceFactory.CreateResourceSet(new() {
        Layout = TextureResourceLayout,
        BoundResources = [
            RenderSystem.GraphicsDevice.PointSampler,
            texture
        ]
    });

    public ResourceSet CreateFilteredTextureResourceSet(Texture texture) => RenderSystem.ResourceFactory.CreateResourceSet(new() {
        Layout = TextureResourceLayout,
        BoundResources = [
            RenderSystem.GraphicsDevice.LinearSampler,
            texture
        ]
    });

    private void Reload(PackManager packs) {
        LoadedTextures.Clear();
        TextureSets.Clear();
        TextureKeys.Clear();

        foreach (var key in packs.ListResources(AssetType.Assets, prefix: "textures/", suffix: ".png")) {
            var loadedTexture = new ImageSharpTexture(packs.OpenStream(AssetType.Assets, key).First(), true);

            var deviceTexture = loadedTexture.CreateDeviceTexture(RenderSystem.GraphicsDevice, RenderSystem.ResourceFactory);

            var textureSet = CreateTextureResourceSet(deviceTexture);

            LoadedTextures[key] = deviceTexture;
            TextureSets[key] = textureSet;
            TextureKeys.Add(key);
        }
    }
}
