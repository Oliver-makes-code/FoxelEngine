using System.Diagnostics.CodeAnalysis;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Voxel.Core.Assets;
using Voxel.Core.Util;

namespace Voxel.Core.Rendering;

public class ShaderManager {
    public readonly PackManager.ReloadTask ReloadTask;

    private readonly RenderSystem RenderSystem;

    private readonly HashSet<ResourceKey> UniqueShaders = new();

    private readonly Dictionary<ResourceKey, string> ShaderSources = new();

    private readonly Dictionary<ResourceKey, Shader[]> CompiledShaders = new();

    public ShaderManager(RenderSystem renderSystem, PackManager packs) {
        RenderSystem = renderSystem;

        Reload(packs);

        ReloadTask = PackManager.RegisterResourceLoader(AssetType.Assets, Reload);
    }

    public bool GetShaders(ResourceKey name, [NotNullWhen(true)] out Shader[]? shaders)
        => CompiledShaders.TryGetValue(name, out shaders);

    private void Reload(PackManager manager) {
        UniqueShaders.Clear();
        ShaderSources.Clear();

        foreach (var key in manager.ListResources(AssetType.Assets, prefix:"shaders/", suffix:".glsl")) {
            var stream = manager.OpenStream(AssetType.Assets, key).First();

            var reader = new StreamReader(stream);

            string src = reader.ReadToEnd();
            ShaderSources[key] = src;
        }

        foreach ((ResourceKey key, string value) in ShaderSources) {
            if (!key.Value.EndsWith(".v.glsl"))
                continue;
            
            ShaderPreprocessor.Preprocess(value, k => ShaderSources[k.PrefixValue("shaders/")], out var vert, out var frag);
            
            var id = key.WithValue(key.Value.Replace(".v.glsl", string.Empty));

            try {
                var shaders = RenderSystem.ResourceFactory.CreateFromSpirv(
                    new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vert), "main"),
                    new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(frag), "main")
                );
                
                CompiledShaders[id] = shaders;
            } catch (SpirvCompilationException e) {
                string path = id.ToFilePath().Replace('/', '.');
                File.WriteAllText($"{path}.debug.vert.glsl", vert);
                File.WriteAllText($"{path}.debug.frag.glsl", frag);
                try {
                    throw new ShaderCompilationException(e, path);
                } catch (ShaderCompilationException err) {
                    Game.Logger.Error(err);
                }
            }
        }
    }

    public class ShaderCompilationException(SpirvCompilationException inner, string filePath) : Exception(
        $"Shader compilation failed! Broken shaders written to `{filePath}.debug.vert.glsl` and `{filePath}.debug.frag.glsl`",
        inner
    ) {}
}
