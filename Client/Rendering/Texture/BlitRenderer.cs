using System.Runtime.InteropServices;
using GlmSharp;
using Veldrid;
using Foxel.Client.Rendering.Utils;
using Foxel.Client.Rendering.VertexTypes;
using Foxel.Core.Assets;

namespace Foxel.Client.Rendering;

/// <summary>
/// Solely responsible for blitting one texture into another.
/// </summary>
public class BlitRenderer : Renderer {
    private readonly DeviceBuffer VertexBuffer;

    private readonly TypedDeviceBuffer<BlitParams> Params;
    private readonly ResourceLayout ParamsLayout;
    private readonly ResourceSet ParamsSet;

    public BlitRenderer(VoxelClient client) : base(client) {

        VertexBuffer = RenderSystem.ResourceFactory.CreateBuffer(new BufferDescription {
            SizeInBytes = (uint)Marshal.SizeOf<Position2dVertex>() * 3, Usage = BufferUsage.VertexBuffer
        });
        RenderSystem.GraphicsDevice.UpdateBuffer(VertexBuffer, 0, new[] {
            new Position2dVertex(new vec2(0, -1)),
            new Position2dVertex(new vec2(0, 1)),
            new Position2dVertex(new vec2(2, 1)),
        });

        Params = new(new() {
            Usage = BufferUsage.UniformBuffer | BufferUsage.Dynamic
        }, RenderSystem);

        ParamsLayout = ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("Params", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment))
        );
        ParamsSet = ResourceFactory.CreateResourceSet(new(
            ParamsLayout,
            Params.BackingBuffer
        ));
        WithResourceSet(3, () => ParamsSet);
    }

    public override Pipeline CreatePipeline(PackManager packs, MainFramebuffer framebuffer) {
        if (!RenderSystem.ShaderManager.GetShaders(new("shaders/blit"), out var shaders))
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
                ParamsLayout,
            ]
        }));
    }

    public override void Render(double delta) {
        var frameBuffer = Client.gameRenderer!.frameBuffer;

        frameBuffer!.Resolve(RenderSystem);

        Blit(frameBuffer.ResolvedMainColorSet, frameBuffer.ResolvedNormalSet, frameBuffer.ResolvedDepthSet, RenderSystem.GraphicsDevice.MainSwapchain.Framebuffer, true);
    }

    public override void Dispose() {
        VertexBuffer.Dispose();
    }

    public void Blit(ResourceSet color, ResourceSet normal, ResourceSet depth, Framebuffer destination, bool flip = false) {
        Params.SetValue(new BlitParams {
            flipped = flip
        }, CommandList);

        CommandList.SetFramebuffer(destination);

        // Set resource sets...
        CommandList.SetGraphicsResourceSet(0, color);
        CommandList.SetGraphicsResourceSet(1, normal);
        CommandList.SetGraphicsResourceSet(2, depth);

        // Draw the texture
        CommandList.SetVertexBuffer(0, VertexBuffer);
        CommandList.Draw(3);
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct BlitParams {
        public bool flipped;
    }
}
