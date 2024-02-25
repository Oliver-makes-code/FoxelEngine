using System.Diagnostics.CodeAnalysis;
using Veldrid;
using Veldrid.ImageSharp;
using Voxel.Core.Assets;

namespace Voxel.Core.Rendering;

public class TextureManager {
    public delegate void TexturesLoadedEvent(PackManager packManager, TextureManager textureManager);

    public static event TexturesLoadedEvent? OnTexturesLoaded;

    private readonly RenderSystem RenderSystem;
    public readonly ResourceLayout TextureResourceLayout;

    private readonly Dictionary<string, Texture> LoadedTextures = [];
    private readonly Dictionary<string, ResourceSet> TextureSets = [];

    public TextureManager(RenderSystem renderSystem, AssetReader assetReader, PackManager packManager) {
        RenderSystem = renderSystem;

        TextureResourceLayout = RenderSystem.ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment | ShaderStages.Vertex),
            new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment | ShaderStages.Vertex)
        ));

        // Keep the old behavior until it works properly
        foreach ((string path, Stream textureStream, _) in assetReader.LoadAll(".png")) {
            var loadedTexture = new ImageSharpTexture(textureStream, true);

            var deviceTexture = loadedTexture.CreateDeviceTexture(RenderSystem.GraphicsDevice, RenderSystem.ResourceFactory);

            var textureSet = CreateTextureResourceSet(deviceTexture);

            LoadedTextures[path] = deviceTexture;
            TextureSets[path] = textureSet;
        }

        PackManager.RegisterResourceLoader(Reload);
    }

    private void Reload(PackManager packManager) {
        foreach (var key in packManager.ListResources(AssetType.Assets, ".png")) {
            var loadedTexture = new ImageSharpTexture(packManager.OpenStream(AssetType.Assets, key).First(), true);

            var deviceTexture = loadedTexture.CreateDeviceTexture(RenderSystem.GraphicsDevice, RenderSystem.ResourceFactory);

            var textureSet = CreateTextureResourceSet(deviceTexture);

            LoadedTextures[key.ToString()] = deviceTexture;
            TextureSets[key.ToString()] = textureSet;
        }
        OnTexturesLoaded?.Invoke(packManager, this);
    }

    public bool TryGetTexture(string path, [NotNullWhen(true)] out Texture? texture) => LoadedTextures.TryGetValue(path, out texture);

    public bool TryGetTextureResourceSet(string path, [NotNullWhen(true)] out ResourceSet? textureSet) => TextureSets.TryGetValue(path, out textureSet);

    public bool TryGetTextureAndSet(string path, [NotNullWhen(true)] out Texture? texture, [NotNullWhen(true)] out ResourceSet? textureSet) {
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
}
