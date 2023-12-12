using System.Diagnostics.CodeAnalysis;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Voxel.Core.Assets;

namespace Voxel.Core.Rendering;

public class ShaderManager {

    private readonly RenderSystem RenderSystem;

    private readonly HashSet<string> UniqueShaders = new();

    private readonly Dictionary<string, string> ShaderSources = new();

    private readonly Dictionary<string, Shader[]> CompiledShaders = new();

    public ShaderManager(RenderSystem renderSystem, AssetReader assetReader) {
        RenderSystem = renderSystem;

        foreach ((string path, Stream sourceStream, int length) in assetReader.LoadAll("", ".glsl")) {
            Span<byte> tmp = stackalloc byte[length];
            if (sourceStream.Read(tmp) != length)
                return;

            string src = Encoding.UTF8.GetString(tmp);
            ShaderSources[path] = src;
        }

        foreach ((string key, string value) in ShaderSources) {
            if (!key.EndsWith(".v.glsl"))
                continue;

            ShaderPreprocessor.Preprocess(value, k => ShaderSources[Path.Combine("shaders", k)], out var vert, out var frag);

            try {
                var shaders = RenderSystem.ResourceFactory.CreateFromSpirv(
                    new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vert), "main"),
                    new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(frag), "main")
                );

                CompiledShaders[key.Replace(".v.glsl", string.Empty)] = shaders;
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }
        
        
    }

    public bool GetShaders(string name, [NotNullWhen(true)] out Shader[]? shaders)
        => CompiledShaders.TryGetValue(name, out shaders);
}
