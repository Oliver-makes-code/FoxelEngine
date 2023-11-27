using System.Diagnostics.CodeAnalysis;
using RenderSurface.Assets;
using Veldrid;
using Veldrid.ImageSharp;

namespace RenderSurface.Rendering;

public class TextureManager {

    private readonly RenderSystem RenderSystem;
    public readonly ResourceLayout TextureResourceLayout;

    private readonly Dictionary<string, Texture> LoadedTextures = new();
    private readonly Dictionary<string, ResourceSet> TextureSets = new();

    public TextureManager(RenderSystem renderSystem, AssetReader assetReader) {
        RenderSystem = renderSystem;

        TextureResourceLayout = RenderSystem.ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment | ShaderStages.Vertex),
            new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment | ShaderStages.Vertex)
        ));

        foreach ((string path, Stream textureStream, _) in assetReader.LoadAll(".png")) {
            var loadedTexture = new ImageSharpTexture(textureStream, true);

            var deviceTexture = loadedTexture.CreateDeviceTexture(RenderSystem.GraphicsDevice, RenderSystem.ResourceFactory);

            var textureSet = CreateTextureResourceSet(deviceTexture);

            LoadedTextures[path] = deviceTexture;
            TextureSets[path] = textureSet;
        }
    }

    public bool TryGetTexture(string path, [NotNullWhen(true)] out Texture? texture) => LoadedTextures.TryGetValue(path, out texture);

    public bool TryGetTextureResourceSet(string path, [NotNullWhen(true)] out ResourceSet? textureSet) => TextureSets.TryGetValue(path, out textureSet);

    public bool TryGetTextureAndSet(string path, [NotNullWhen(true)] out Texture? texture, [NotNullWhen(true)] out ResourceSet? textureSet) {
        textureSet = null;
        return LoadedTextures.TryGetValue(path, out texture) && TextureSets.TryGetValue(path, out textureSet);
    }

    public ResourceSet CreateTextureResourceSet(Texture texture) => RenderSystem.ResourceFactory.CreateResourceSet(new() {
        Layout = TextureResourceLayout,
        BoundResources = new BindableResource[] {
            RenderSystem.GraphicsDevice.Aniso4xSampler,
            texture
        }
    });
}
