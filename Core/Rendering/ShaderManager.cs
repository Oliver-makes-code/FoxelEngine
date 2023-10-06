using System.Diagnostics.CodeAnalysis;
using System.Text;
using RenderSurface.Assets;
using Veldrid;
using Veldrid.SPIRV;

namespace RenderSurface.Rendering;

public class ShaderManager {

    private readonly RenderSystem RenderSystem;

    private readonly HashSet<string> UniqueShaders = new();
    
    private readonly Dictionary<string, string> ShaderSources = new();

    private readonly Dictionary<string, Shader[]> CompiledShaders = new();

    public ShaderManager(RenderSystem renderSystem, AssetReader assetReader) {
        RenderSystem = renderSystem;

        assetReader.LoadAll(s => s.EndsWith(".glsl"), LoadShaderSource);

        foreach (string uniqueShader in UniqueShaders)
            LoadActualShader(uniqueShader);
    }

    private void LoadShaderSource(string path, Stream sourceStream, int length) {

        Span<byte> tmp = stackalloc byte[length];
        if (sourceStream.Read(tmp) != length)
            return;

        var src = Encoding.UTF8.GetString(tmp);
        ShaderSources[path] = src;

        UniqueShaders.Add(path.Replace(".vert.glsl", string.Empty).Replace(".frag.glsl", string.Empty));
    }

    private void LoadActualShader(string uniqueShaderName) {
        if (!ShaderSources.TryGetValue($"{uniqueShaderName}.frag.glsl", out var fragSrc) || !ShaderSources.TryGetValue($"{uniqueShaderName}.vert.glsl", out var vertSrc))
            return;

        try {
            var shaders = RenderSystem.ResourceFactory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertSrc), "main"),
                new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragSrc), "main")
            );

            CompiledShaders[uniqueShaderName] = shaders;
        } catch (Exception e) {
            //TODO - Add fallback shader & log error

            Console.Out.WriteLine(e);
        }
    }
    public bool GetShaders(string name, [NotNullWhen(true)] out Shader[]? shaders)
        => CompiledShaders.TryGetValue(name, out shaders);
}
