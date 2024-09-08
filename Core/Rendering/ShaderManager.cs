using System.Diagnostics.CodeAnalysis;
using Veldrid;
using Veldrid.SPIRV;
using Foxel.Core.Assets;
using Foxel.Core.Util;

namespace Foxel.Core.Rendering;

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

            string filePath = new ResourceKey(key.Group, key.Value["shaders/".Length..]).ToString();

            ShaderCompiler.Compile(value, filePath, k => ShaderSources.TryGetValue(k.PrefixValue("shaders/"), out var src) ? src : null, out var vert, out var frag);
            // ShaderPreprocessor.Preprocess(value, k => ShaderSources.TryGetValue(k.PrefixValue("shaders/"), out var src) ? src : null, out var vert, out var frag);
            
            var id = key.WithValue(key.Value.Replace(".v.glsl", string.Empty));

            try {
                var shaders = RenderSystem.ResourceFactory.CreateFromSpirv(
                    new ShaderDescription(ShaderStages.Vertex, vert, "main"),
                    new ShaderDescription(ShaderStages.Fragment, frag, "main")
                );
                
                CompiledShaders[id] = shaders;
            } catch (SpirvCompilationException e) {
                string path = id.ToFilePath().Replace('/', '.');
                File.WriteAllBytes($"{path}.debug.vert.spirv", vert);
                File.WriteAllBytes($"{path}.debug.frag.spirv", frag);
                try {
                    throw new ShaderCompilationException(e, path);
                } catch (ShaderCompilationException err) {
                    Game.Logger.Error(err);
                }
            }
        }
    }

    public class ShaderCompilationException(SpirvCompilationException inner, string filePath) : Exception(
        $"Shader compilation failed! Broken shaders written to `{filePath}.debug.vert.spirv` and `{filePath}.debug.frag.spirv`",
        inner
    ) {}
}
