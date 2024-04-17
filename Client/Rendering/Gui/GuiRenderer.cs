using System;
using System.Runtime.InteropServices;
using Veldrid;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Client.Gui;
using Voxel.Client.Rendering.Texture;
using Voxel.Client.Gui.Canvas;

namespace Voxel.Client.Rendering.Gui;

public class GuiRenderer : Renderer, IDisposable {
    public readonly Atlas GuiAtlas;
    private readonly DeviceBuffer GuiVertices;

    public Pipeline? GuiPipeline;
    
    public GuiRenderer(VoxelClient client) : base(client) {
        GuiVertices = RenderSystem.ResourceFactory.CreateBuffer(new() {
            SizeInBytes = (uint)Marshal.SizeOf<GuiVertex>() * 1024, // limits GuiRect's to 256
            Usage = BufferUsage.VertexBuffer | BufferUsage.Dynamic
        });

        GuiAtlas = new(new("gui"), RenderSystem);
        AtlasLoader.LoadAtlas(RenderSystem.Game.AssetReader, GuiAtlas, RenderSystem);
        
        // initialize the GuiCanvas after loading the atlas, dummy
        // (it's me, i'm the dummy)
        GuiCanvas.Init(this);
    }

    public override void CreatePipeline(MainFramebuffer framebuffer) {
        if (!Client.RenderSystem.ShaderManager.GetShaders("shaders/gui", out var shaders))
            throw new("Shaders not present.");

        GuiPipeline = framebuffer.AddDependency(ResourceFactory.CreateGraphicsPipeline(new() {
            BlendState = BlendStateDescription.SingleAlphaBlend, // TODONE: Set this back to BlendStateDescription.SingleAlphaBlend
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
        CommandList.SetGraphicsResourceSet(0, GuiAtlas.atlasResourceSet);
        
        CommandList.UpdateBuffer(GuiVertices, 0, GuiCanvas.QuadCache);

        CommandList.SetVertexBuffer(0, GuiVertices);
        CommandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);
        
        CommandList.DrawIndexed(GuiCanvas.QuadCount * 6);
    }

    public override void Dispose() {}
}
