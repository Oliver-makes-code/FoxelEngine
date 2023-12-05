using System;
using Veldrid;
using Voxel.Client.Rendering.VertexTypes;

namespace Voxel.Client.Rendering.GUI; 

public class GUIRenderer : Renderer, IDisposable {
    public readonly Pipeline GUIPipeline;

    public GUIRenderer(VoxelClient client) : base(client) {
        if (!client.RenderSystem.ShaderManager.GetShaders("shaders/gui", out var shaders))
            throw new("Shaders not present.");
        
        GUIPipeline = ResourceFactory.CreateGraphicsPipeline(new() {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = new() {
                DepthComparison = ComparisonKind.Never,
                DepthTestEnabled = false,
                DepthWriteEnabled = false,
            },
            Outputs = RenderSystem.GraphicsDevice.SwapchainFramebuffer.OutputDescription,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            RasterizerState = new() {
                CullMode = FaceCullMode.None,
                DepthClipEnabled = false,
                FillMode = PolygonFillMode.Solid,
                FrontFace = FrontFace.CounterClockwise,
                ScissorTestEnabled = false
            },
            ResourceLayouts = new[] {
                RenderSystem.TextureManager.TextureResourceLayout
            },
            ShaderSet = new() {
                VertexLayouts = new[] {
                    GUIVertex.Layout
                },
                Shaders = shaders
            }
        });
    }
    
    public override void Render(double delta) {
        CommandList.SetPipeline(GUIPipeline);

        //CommandList.SetGraphicsResourceSet(0, TerrainAtlas.AtlasResourceSet);

        CommandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32); 
    }
    public override void Dispose() {}
}
