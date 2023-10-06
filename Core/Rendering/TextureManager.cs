using System.Diagnostics.CodeAnalysis;
using RenderSurface.Assets;
using Veldrid;
using Veldrid.ImageSharp;

namespace RenderSurface.Rendering;

public class TextureManager {

    private readonly RenderSystem RenderSystem;
    public readonly ResourceLayout TextureResourceLayout;

    private readonly Dictionary<string, Texture> loadedTextures = new();
    private readonly Dictionary<string, ResourceSet> textureSets = new();

    public TextureManager(RenderSystem renderSystem, AssetReader assetReader) {
        RenderSystem = renderSystem;

        TextureResourceLayout = RenderSystem.ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment | ShaderStages.Vertex),
            new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment | ShaderStages.Vertex)
        ));

        assetReader.LoadAll(s => s.EndsWith(".png"), LoadTexture);

    }


    private void LoadTexture(string path, Stream textureStream, int length) {
        var loadedTexture = new ImageSharpTexture(textureStream, true);

        var deviceTexture = loadedTexture.CreateDeviceTexture(RenderSystem.GraphicsDevice, RenderSystem.ResourceFactory);

        var textureSet = RenderSystem.ResourceFactory.CreateResourceSet(new ResourceSetDescription {
            Layout = TextureResourceLayout,
            BoundResources = new BindableResource[] {
                RenderSystem.GraphicsDevice.PointSampler,
                deviceTexture
            }
        });

        loadedTextures[path] = deviceTexture;
        textureSets[path] = textureSet;
    }


    public bool TryGetTextureResourceSet(string path, [NotNullWhen(true)] out ResourceSet? textureSet) => textureSets.TryGetValue(path, out textureSet);
}
