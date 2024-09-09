using System.Runtime.InteropServices;
using GlmSharp;
using Veldrid;
using Foxel.Client.Rendering.Utils;
using Foxel.Client.Rendering.VertexTypes;
using Foxel.Core.Assets;
using System;

namespace Foxel.Client.Rendering;

/// <summary>
/// Solely responsible for blitting one texture into another.
/// </summary>
public class BlitRenderer : Renderer {
    private readonly DeviceBuffer VertexBuffer;

    private readonly TypedDeviceBuffer<BlitParam> BlitParams;
    private readonly TypedDeviceBuffer<vec2> ScreenSizeBuffer;
    private readonly ResourceLayout BlitParamsLayout;
    private readonly ResourceLayout SsaoParamsLayout;
    public readonly ResourceLayout ScreenSizeResourceLayout;
    private readonly ResourceSet BlitParamsSet;
    private readonly ResourceSet SsaoParamsSet;
    public readonly ResourceSet ScreenSizeResourceSet;

    public BlitRenderer(VoxelClient client) : base(client) {
        VertexBuffer = RenderSystem.ResourceFactory.CreateBuffer(new BufferDescription {
            SizeInBytes = (uint)Marshal.SizeOf<Position2dVertex>() * 3, Usage = BufferUsage.VertexBuffer
        });
        RenderSystem.GraphicsDevice.UpdateBuffer(VertexBuffer, 0, new[] {
            new Position2dVertex(new vec2(0, -1)),
            new Position2dVertex(new vec2(0, 1)),
            new Position2dVertex(new vec2(2, 1)),
        });

        BlitParams = new(new() {
            Usage = BufferUsage.UniformBuffer | BufferUsage.Dynamic
        }, RenderSystem);

        BlitParamsLayout = ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("BlitParams", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        BlitParamsSet = ResourceFactory.CreateResourceSet(new(
            BlitParamsLayout,
            BlitParams.BackingBuffer
        ));

        ScreenSizeResourceLayout = ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("ScreenSize", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        ScreenSizeBuffer = new(
            new() {
                Usage = BufferUsage.UniformBuffer | BufferUsage.Dynamic
            },
            RenderSystem
        );

        ScreenSizeResourceSet = ResourceFactory.CreateResourceSet(new(
            ScreenSizeResourceLayout,
            ScreenSizeBuffer.BackingBuffer
        ));

        WithResourceSet(4, () => {
            var screenSize = (vec2)Client.screenSize;
            CommandList.UpdateBuffer(ScreenSizeBuffer, 0, [new vec4(screenSize, 1/screenSize.x, 1/screenSize.y)]);
            return ScreenSizeResourceSet;
        });

        WithResourceSet(5, () => BlitParamsSet);
    }

    public override Pipeline CreatePipeline(PackManager packs, MainFramebuffer framebuffer) {
        if (!RenderSystem.ShaderManager.GetShaders(new("shaders/deferred/blit"), out var shaders))
            throw new("Blit shaders not found");

        return framebuffer.AddDependency(ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription {
            Outputs = framebuffer.WindowFramebuffer.OutputDescription,
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
                RenderSystem.TextureManager.TextureResourceLayout,
                RenderSystem.TextureManager.TextureResourceLayout,
                RenderSystem.TextureManager.TextureResourceLayout,
                RenderSystem.TextureManager.TextureResourceLayout,
                ScreenSizeResourceLayout,
                BlitParamsLayout,
            ]
        }));
    }

    public override void Render(double delta) {
        var frameBuffer = Client.gameRenderer!.frameBuffer;

        frameBuffer!.Resolve(RenderSystem);

        Blit(frameBuffer.ResolvedMainColorSet, frameBuffer.ResolvedNormalSet, frameBuffer.ResolvedScreenPosSet, frameBuffer.ResolvedDepthSet, RenderSystem.GraphicsDevice.MainSwapchain.Framebuffer, true);
    }

    public override void Dispose() {
        VertexBuffer.Dispose();
    }

    public void Blit(ResourceSet color, ResourceSet normal, ResourceSet screenPos, ResourceSet depth, Framebuffer destination, bool flip = false) {
        BlitParams.SetValue(new BlitParam {
            flipped = flip
        }, CommandList);

        CommandList.SetFramebuffer(destination);

        // Set resource sets...
        CommandList.SetGraphicsResourceSet(0, color);
        CommandList.SetGraphicsResourceSet(1, normal);
        CommandList.SetGraphicsResourceSet(2, screenPos);
        CommandList.SetGraphicsResourceSet(3, depth);

        // Draw the texture
        CommandList.SetVertexBuffer(0, VertexBuffer);
        CommandList.Draw(3);
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct BlitParam {
        public bool flipped;
    }
}
