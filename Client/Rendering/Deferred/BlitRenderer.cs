using Veldrid;
using Foxel.Client.Rendering.VertexTypes;
using Foxel.Core.Assets;

namespace Foxel.Client.Rendering.Deferred;

public class BlitRenderer : Renderer {
    public readonly DeferredRenderer DeferredRenderer;

    public BlitRenderer(VoxelClient client, DeferredRenderer parent) : base(client) {
        DeferredRenderer = parent;

        DeferredRenderer.ApplyResourceSets(this);
        WithResourceSet(DeferredRenderer.SetIndex(0), () => DeferredRenderer.Ssao2.outputTextureSet);
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
                ..DeferredRenderer.Layouts(framebuffer),
                RenderSystem.TextureManager.TextureResourceLayout,
            ]
        }));
    }

    public override void Render(double delta) {
        var frameBuffer = Client.gameRenderer!.frameBuffer;

        frameBuffer!.Resolve(RenderSystem);

        Blit(RenderSystem.GraphicsDevice.MainSwapchain.Framebuffer);
    }

    public void Blit(Framebuffer destination) {
        RenderSystem.SetFramebuffer(destination);

        // Draw the texture
        DeferredRenderer.VertexBuffer.Bind(0);
        RenderSystem.Draw(3);
    }
}
