using System.Text;
using RenderSurface.Assets;
using Veldrid;
using Veldrid.SPIRV;

namespace RenderSurface.Rendering;

public class ShaderManager {

    private readonly RenderSystem RenderSystem;

    private HashSet<string> _uniqueShaders = new();
    private Dictionary<string, string> _shaderSources = new();

    private Dictionary<string, Shader[]> _compiledShaders = new();

    public ShaderManager(RenderSystem renderSystem, AssetReader assetReader) {
        RenderSystem = renderSystem;

        assetReader.LoadAll(s => s.EndsWith(".glsl"), LoadShaderSource);

        foreach (string uniqueShader in _uniqueShaders)
            LoadActualShader(uniqueShader);
    }

    private void LoadShaderSource(string path, Stream sourceStream, int length) {

        Span<byte> tmp = stackalloc byte[length];
        if (sourceStream.Read(tmp) != length)
            return;

        var src = Encoding.UTF8.GetString(tmp);
        _shaderSources[path] = src;

        _uniqueShaders.Add(path.Replace(".vert.glsl", string.Empty).Replace(".frag.glsl", string.Empty));
    }

    private void LoadActualShader(string uniqueShaderName) {
        if (!_shaderSources.TryGetValue($"{uniqueShaderName}.frag.glsl", out var fragSrc) || !_shaderSources.TryGetValue($"{uniqueShaderName}.vert.glsl", out var vertSrc))
            return;

        try {
            var shaders = RenderSystem.ResourceFactory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragSrc), "main"),
                new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertSrc), "main")
            );

            _compiledShaders[uniqueShaderName] = shaders;
        } catch (Exception e) {
            //TODO - Add fallback shader & log error
        }
    }
}
