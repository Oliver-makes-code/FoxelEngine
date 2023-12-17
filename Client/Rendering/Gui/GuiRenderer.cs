using System;
using System.Runtime.InteropServices;
using Veldrid;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Client.Gui;

namespace Voxel.Client.Rendering.Gui;

public class GuiRenderer : Renderer, IDisposable {
    public Pipeline GuiPipeline;
    
    private readonly DeviceBuffer GuiVertices;
    private readonly ResourceSet GuiTestTexture;

    public GuiRenderer(VoxelClient client) : base(client) {
        GuiCanvas.Init(this);
        
        GuiVertices = RenderSystem.ResourceFactory.CreateBuffer(new() {
            SizeInBytes = (uint)Marshal.SizeOf<BasicVertex.Packed>() * 1024, // limits GuiRect's to 256
            Usage = BufferUsage.VertexBuffer | BufferUsage.Dynamic
        });
        GuiTestTexture = RenderSystem.TextureManager.CreateTextureResourceSet(RenderSystem.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            128u, 128u,
            4u, 1u,
            PixelFormat.R32_G32_B32_A32_Float,
            TextureUsage.Sampled | TextureUsage.RenderTarget | TextureUsage.GenerateMipmaps
        )));
    }

    public override void CreatePipeline(MainFramebuffer framebuffer) {
        if (!Client.RenderSystem.ShaderManager.GetShaders("shaders/gui", out var shaders))
            throw new("Shaders not present.");

        GuiPipeline = framebuffer.AddDependency(ResourceFactory.CreateGraphicsPipeline(new() {
            BlendState = BlendStateDescription.SingleDisabled, // TODO: Set this back to BlendStateDescription.SingleAlphaBlend
            DepthStencilState = new() {
                DepthComparison = ComparisonKind.Never, DepthTestEnabled = false, DepthWriteEnabled = false,
            },
            Outputs = framebuffer.Framebuffer.OutputDescription,
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
                    GuiVertex.Layout
                },
                Shaders = shaders
            }
        }));
        
       
    }
    public override void Render(double delta) {
        CommandList.SetPipeline(GuiPipeline);
        CommandList.SetGraphicsResourceSet(0, Client.GameRenderer.WorldRenderer.ChunkRenderer.TerrainAtlas.AtlasResourceSet);

        var verts = Voxel.Client.Gui.GuiCanvas.GetVertices();
        CommandList.UpdateBuffer(GuiVertices, 0, verts);

        CommandList.SetVertexBuffer(0, GuiVertices);
        CommandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);
        
        CommandList.DrawIndexed((uint)(verts.Length / 4 * 6));
    }
    public override void Dispose() {}
}
