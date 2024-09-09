using System.Runtime.InteropServices;
using GlmSharp;
using Veldrid;
using Foxel.Client.Rendering.Utils;
using Foxel.Client.Rendering.VertexTypes;
using Foxel.Core.Assets;
using System;

namespace Foxel.Client.Rendering.Deferred;

public class NewBlitRenderer : Renderer {
    public readonly DeferredRenderer DeferredRenderer;

    public NewBlitRenderer(VoxelClient client, DeferredRenderer parent) : base(client) {
        DeferredRenderer = parent;

        DeferredRenderer.ApplyResourceSets(this);
        WithResourceSet(5, () => DeferredRenderer.Test.OutputTextureSet);
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
                ..DeferredRenderer.Layouts(),
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
        CommandList.SetFramebuffer(destination);

        // Draw the texture
        CommandList.SetVertexBuffer(0, DeferredRenderer.VertexBuffer);
        CommandList.Draw(3);
    }
}
