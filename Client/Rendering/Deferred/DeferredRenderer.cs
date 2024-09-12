using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Foxel.Client.Rendering.Utils;
using Foxel.Client.Rendering.VertexTypes;
using Foxel.Core.Assets;
using Foxel.Core.Rendering.Buffer;
using Foxel.Core.Util;
using GlmSharp;
using Veldrid;

namespace Foxel.Client.Rendering.Deferred;

public class DeferredRenderer : Renderer {
    public readonly VertexBuffer<Position2dVertex> VertexBuffer;

    public readonly SsaoDeferredStage1 Ssao1;
    public readonly SsaoDeferredStage2 Ssao2;
    public readonly BlitRenderer Blit;

    private readonly TypedGraphicsBuffer<vec2> ScreenSizeBuffer;
    private readonly ResourceLayout ScreenSizeResourceLayout;
    private readonly ResourceSet ScreenSizeResourceSet;

    public DeferredRenderer(VoxelClient client) : base(client) {
        VertexBuffer = new(RenderSystem);
        VertexBuffer.UpdateImmediate([
            new Position2dVertex(new vec2(0, -1)),
            new Position2dVertex(new vec2(0, 1)),
            new Position2dVertex(new vec2(2, 1)),
        ]);


        ScreenSizeResourceLayout = ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("ScreenSize", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        ScreenSizeBuffer = new(RenderSystem, GraphicsBufferUsage.UniformBuffer | GraphicsBufferUsage.Dynamic);
        ScreenSizeBuffer.WithCapacity(2);

        ScreenSizeResourceSet = ResourceFactory.CreateResourceSet(new(
            ScreenSizeResourceLayout,
            ScreenSizeBuffer.baseBuffer
        ));

        Ssao1 = new(Client, this);
        DependsOn(Ssao1);
        Ssao2 = new(Client, this);
        DependsOn(Ssao2);
        Blit = new(Client, this);
        DependsOn(Blit);
    }

    public static uint SetIndex(uint idx)
        => idx + 3;

    public ResourceLayout[] Layouts(MainFramebuffer buffer)
        => [
            buffer.ResolvedTextureLayout,
            ScreenSizeResourceLayout,
            Client.gameRenderer!.CameraStateManager.CameraResourceLayout,
        ];

    public void ApplyResourceSets(Renderer renderer) {
        renderer.WithResourceSet(0, () => Client.gameRenderer!.frameBuffer!.ResolvedTextureSet);

        renderer.WithResourceSet(1, () => {
            var screenSize = (vec2)Client.screenSize;
            ScreenSizeBuffer.UpdateDeferred(0, [screenSize, 1 / screenSize]);
            return ScreenSizeResourceSet;
        });

        renderer.WithResourceSet(2, () => Client.gameRenderer!.CameraStateManager.CameraResourceSet);
    }
}

public abstract class DeferredStage : Renderer {
    public readonly DeferredRenderer DeferredRenderer;

    public readonly float Scale;
    public readonly PixelFormat Format;

    public Veldrid.Texture outputTexture;
    public ResourceSet outputTextureSet;
    public Framebuffer outputBuffer;

    protected DeferredStage(VoxelClient client, DeferredRenderer parent, float scale, PixelFormat format) : base(client) {
        DeferredRenderer = parent;

        Scale = scale;
        Format = format;
        
        DeferredRenderer.ApplyResourceSets(this);
    }

    public abstract ResourceKey ShaderKey();
    public abstract ResourceLayout[] Layouts();

    public override Pipeline CreatePipeline(PackManager packs, MainFramebuffer framebuffer) {
        if (!RenderSystem.ShaderManager.GetShaders(ShaderKey(), out var shaders))
            throw new($"Deferred rendering ({ShaderKey()}) shaders not found");
        
        uint width = (uint)(Client.nativeWindow!.Width * Scale);
        uint height = (uint)(Client.nativeWindow!.Height * Scale);

        var baseDescription = new TextureDescription {
            Width = width,
            Height = height,
            Depth = 1,
            ArrayLayers = 1,
            MipLevels = 1,
            Type = TextureType.Texture2D,
            SampleCount = TextureSampleCount.Count1,
            Format = Format,
            Usage = TextureUsage.RenderTarget | TextureUsage.Sampled
        };

        outputTexture = RenderSystem.ResourceFactory.CreateTexture(baseDescription);
        outputTextureSet = RenderSystem.TextureManager.CreateFilteredTextureResourceSet(outputTexture);

        outputBuffer = RenderSystem.ResourceFactory.CreateFramebuffer(new FramebufferDescription {
            ColorTargets = [
                new FramebufferAttachmentDescription {
                    Target = outputTexture
                }
            ]
        });

        return framebuffer.AddDependency(ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription {
            Outputs = outputBuffer.OutputDescription,
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new() {
                DepthWriteEnabled = false,
                DepthTestEnabled = false,
                StencilTestEnabled = false,
            },
            PrimitiveTopology = PrimitiveTopology.TriangleStrip,
            RasterizerState = new() {
                CullMode = FaceCullMode.None,
                DepthClipEnabled = false,
                ScissorTestEnabled = false,
                FillMode = PolygonFillMode.Solid
            },
            ShaderSet = new() {
                VertexLayouts = [
                    Position2dVertex.Layout
                ],
                Shaders = shaders
            },
            ResourceLayouts = [
                ..DeferredRenderer.Layouts(framebuffer),
                ..Layouts()
            ]
        }));
    }

    public override void Render(double delta) {
        RenderSystem.SetFramebuffer(outputBuffer);
        DeferredRenderer.VertexBuffer.Bind(0);
        RenderSystem.Draw(3);
    }
}
