using System;
using Foxel.Client.Input;
using Foxel.Common.Util;
using Foxel.Core.Rendering.Resources.Buffer;
using Foxel.Core.Util;
using GlmSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;

namespace Foxel.Client.Rendering.Deferred;

public class SsaoDeferredStage1 : DeferredStage {
    const int SampleCount = 32;

    private readonly Veldrid.Texture RandomOffsetTexture;
    private readonly ResourceSet RandomOffsetTextureSet;

    private readonly GraphicsBuffer<vec4> SampleBuffer;
    private readonly ResourceLayout SampleResourceLayout;
    private readonly ResourceSet SampleResourceSet;
    private readonly GraphicsBuffer<bool> ConfigBuffer;
    private readonly ResourceLayout ConfigResourceLayout;
    private readonly ResourceSet ConfigResourceSet;

    public SsaoDeferredStage1(VoxelClient client, DeferredRenderer parent) : base(client, parent, 0.5f, PixelFormat.R16_G16_B16_A16_Float) {
        var rand = Random.Shared;

        var texture = new Image<Rgba32>(16, 16);
        
        foreach (var pos in Iteration.Square(16)) {
            texture[pos.x, pos.y] = new(
                rand.NextSingle() * 2 - 1,
                rand.NextSingle() * 2 - 1,
                0
            );
        }

        var isTexture = new ImageSharpTexture(texture);
        RandomOffsetTexture = isTexture.CreateDeviceTexture(Client.graphicsDevice, Client.gameRenderer!.ResourceFactory);
        RandomOffsetTextureSet = Client.gameRenderer.RenderSystem.TextureManager.CreateFilteredTextureResourceSet(RandomOffsetTexture);

        vec4[] samples = new vec4[SampleCount];
        for (int i = 0; i < SampleCount; i++) {
            var sample = new vec3(
                rand.NextSingle() * 2 - 1,
                rand.NextSingle() * 2 - 1,
                rand.NextSingle()
            ).Normalized * rand.NextSingle();

            const float Scale = 1 / 64f;
            sample *= float.Lerp(0.1f, 1, Scale * Scale);
            samples[i] = new(sample, 1);
        }

        SampleResourceLayout = ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("ScreenSize", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        SampleBuffer = new(RenderSystem, GraphicsBufferType.UniformBuffer, SampleCount);
        SampleBuffer.UpdateDeferred(0, samples);

        SampleResourceSet = ResourceFactory.CreateResourceSet(new(
            SampleResourceLayout,
            SampleBuffer.BaseBuffer
        ));

        ConfigResourceLayout = ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("Config", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        ConfigBuffer = new(RenderSystem, GraphicsBufferType.UniformBuffer, 1);

        ConfigResourceSet = ResourceFactory.CreateResourceSet(new(
            ConfigResourceLayout,
            ConfigBuffer.BaseBuffer
        ));
        WithResourceSet(DeferredRenderer.SetIndex(0), () => RandomOffsetTextureSet);
        WithResourceSet(DeferredRenderer.SetIndex(1), () => SampleResourceSet);
        WithResourceSet(DeferredRenderer.SetIndex(2), () => {
            ConfigBuffer.UpdateDeferred(0, [!ActionGroups.Ssao.GetValue()]);
            return ConfigResourceSet;
        });
    }

    public override ResourceLayout[] Layouts()
        => [
            RenderSystem.TextureManager.TextureResourceLayout,
            SampleResourceLayout,
            ConfigResourceLayout
        ];

    public override ResourceKey ShaderKey()
        => new("shaders/deferred/ssao");
}

public class SsaoDeferredStage2 : DeferredStage {
    public SsaoDeferredStage2(VoxelClient client, DeferredRenderer parent) : base(client, parent, 0.5f, PixelFormat.R16_G16_B16_A16_Float) {
        WithResourceSet(DeferredRenderer.SetIndex(0), () => DeferredRenderer.Ssao1.outputTextureSet!);
    }

    public override ResourceLayout[] Layouts()
        => [
            RenderSystem.TextureManager.TextureResourceLayout,
        ];

    public override ResourceKey ShaderKey()
        => new("shaders/deferred/ssao_blur");
}
