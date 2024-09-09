using System.Runtime.InteropServices;
using Foxel.Core.Util;
using shaderc;

namespace Foxel.Core.Rendering;

public static class ShaderCompiler {
    public static void Compile(string source, string path, Func<ResourceKey, string?> dependencyProvider, out byte[] vertexCode, out byte[] fragmentCode) {
        var optionsVert = new FoxelIncludeOptions(dependencyProvider);
        optionsVert.AddMacroDefinition("VERTEX");
        optionsVert.AddMacroDefinition("vert", "main");
        optionsVert.AddMacroDefinition("vert_param(i, v)", "layout (location = i) in v;");
        optionsVert.AddMacroDefinition("frag_param(i, v)", "layout (location = i) out v;");
        optionsVert.AddMacroDefinition("out_param(i, v)", "");

        var optionsFrag = new FoxelIncludeOptions(dependencyProvider);
        optionsFrag.AddMacroDefinition("FRAGMENT");
        optionsFrag.AddMacroDefinition("frag", "main");
        optionsFrag.AddMacroDefinition("vert_param(i, v)", "");
        optionsFrag.AddMacroDefinition("frag_param(i, v)", "layout (location = i) in v;");
        optionsFrag.AddMacroDefinition("out_param(i, v)", "layout (location = i) out v;");

        using var vertCompiler = new Compiler(optionsVert);
        var vertResult = vertCompiler.Compile(source, path, ShaderKind.GlslVertexShader, "main");
        if (vertResult.Status != Status.Success)
            throw new Exception($"Vert shader compilation for {path} failed: {vertResult.Status}\n{vertResult.ErrorMessage}");

        using var fragCompiler = new Compiler(optionsFrag);
        var fragResult = fragCompiler.Compile(source, path, ShaderKind.GlslFragmentShader, "main");
        if (fragResult.Status != Status.Success)
            throw new Exception($"Frag shader compilation for {path} failed: {fragResult.Status}\n{fragResult.ErrorMessage}");
        
        vertexCode = new byte[vertResult.CodeLength];
        fragmentCode = new byte[fragResult.CodeLength];
        
        // SAFETY: We already checked the status.
        unsafe {
            Marshal.Copy(vertResult.CodePointer, vertexCode, 0, vertexCode.Length);
            Marshal.Copy(fragResult.CodePointer, fragmentCode, 0, fragmentCode.Length);
        }
    }

    private class FoxelIncludeOptions(Func<ResourceKey, string?> dependencyProvider) : Options {
        private readonly Func<ResourceKey, string?> DependencyProvider = dependencyProvider;
        protected override bool TryFindInclude(string source, string include, IncludeType incType, out string incFile, out string incContent) {
            ResourceKey path = new(include);
            string? src = DependencyProvider(path);
            if (src == null) {
                incFile = "";
                incContent = "";
                return false;
            }
            incFile = include;
            incContent = src;
            return false;
        }
    }
}
