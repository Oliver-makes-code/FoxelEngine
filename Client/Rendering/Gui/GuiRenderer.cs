using System;
using System.Runtime.InteropServices;
using Veldrid;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Client.Gui;
using Voxel.Client.Rendering.Texture;
using Voxel.Client.Gui.Canvas;
using Voxel.Core.Assets;
using Voxel.Core.Rendering;
using NLog.Layouts;
using Voxel.Client.Rendering.Utils;
using GlmSharp;

namespace Voxel.Client.Rendering.Gui;

public class GuiRenderer : Renderer, IDisposable {
    public readonly ReloadableDependency<Atlas> GuiAtlas;
    private readonly DeviceBuffer GuiVertices;
    
    public GuiRenderer(VoxelClient client) : base(client, RenderPhase.PreRender) {
        GuiVertices = RenderSystem.ResourceFactory.CreateBuffer(new() {
            SizeInBytes = (uint)Marshal.SizeOf<GuiVertex>() * 1024, // limits GuiRect's to 256
            Usage = BufferUsage.VertexBuffer | BufferUsage.Dynamic
        });

        GuiAtlas = AtlasLoader.CreateDependency(new("gui"));
        DependsOn(GuiAtlas);
        // AtlasLoader.LoadAtlas(RenderSystem.Game.AssetReader, GuiAtlas, RenderSystem);
        
        // initialize the GuiCanvas after loading the atlas, dummy
        // (it's me, i'm the dummy)
        GuiCanvas.Init(this);

        WithResourceSet(0, () => GuiAtlas.value!.atlasResourceSet);
    }

    public override void Reload(PackManager packs, RenderSystem renderSystem, MainFramebuffer buffer) {
        base.Reload(packs, renderSystem, buffer);
        GuiCanvas.Rebuild();
    }

    public override Pipeline CreatePipeline(PackManager packs, MainFramebuffer framebuffer) {
        if (!Client.renderSystem.ShaderManager.GetShaders(new("shaders/gui"), out var shaders))
            throw new("Shaders not present.");

        return framebuffer.AddDependency(ResourceFactory.CreateGraphicsPipeline(new() {
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
        CommandList.UpdateBuffer(GuiVertices, 0, GuiCanvas.QuadCache);

        CommandList.SetVertexBuffer(0, GuiVertices);
        CommandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);
        
        CommandList.DrawIndexed(GuiCanvas.QuadCount * 6);
    }

    public override void Dispose() {}
}

public class NewGuiRenderer : Renderer, IDisposable {
    public readonly ResourceLayout ScreenSizeResourceLayout;
    public readonly ResourceSet ScreenSizeResourceSet;
    private readonly TypedDeviceBuffer<vec2> ScreenSizeBuffer;
    private readonly DeviceBuffer GuiVertices;

    public NewGuiRenderer(VoxelClient client) : base(client, RenderPhase.PreRender) {
        GuiVertices = RenderSystem.ResourceFactory.CreateBuffer(new() {
            SizeInBytes = (uint)Marshal.SizeOf<PositionVertex>() * 4,
            Usage = BufferUsage.VertexBuffer | BufferUsage.Dynamic
        });

        ScreenSizeResourceLayout = RenderSystem.ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("ScreenSize", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        ScreenSizeBuffer = new(
            new() {
                Usage = BufferUsage.UniformBuffer | BufferUsage.Dynamic
            },
            RenderSystem
        );

        ScreenSizeResourceSet = RenderSystem.ResourceFactory.CreateResourceSet(new() {
            Layout = ScreenSizeResourceLayout,
            BoundResources = [
                ScreenSizeBuffer.BackingBuffer
            ]
        });

        CommandList.UpdateBuffer(GuiVertices, 0, [
            new PositionVertex {
                position = new(0, 0, 0)
            },
            new PositionVertex {
                position = new(64, 0, 0)
            },
            new PositionVertex {
                position = new(64, 64, 0)
            },
            new PositionVertex {
                position = new(0, 64, 0)
            }
        ]);

        WithResourceSet(0, () => {
            CommandList.UpdateBuffer(ScreenSizeBuffer.BackingBuffer, 0, [(vec2)Client.screenSize]);
            return ScreenSizeResourceSet;
        });
    }

    public override void Render(double delta) {
        CommandList.SetVertexBuffer(0, GuiVertices);
        CommandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);
        CommandList.DrawIndexed(6);
    }

    public override Pipeline? CreatePipeline(PackManager packs, MainFramebuffer buffer) {
        if (!Client.renderSystem!.ShaderManager.GetShaders(new("shaders/new_gui"), out var shaders))
            throw new("Shaders not present.");
        return buffer.AddDependency(ResourceFactory.CreateGraphicsPipeline(new() {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = new() {
                DepthComparison = ComparisonKind.Never,
                DepthTestEnabled = false,
                DepthWriteEnabled = false,
            },
            Outputs = buffer.Framebuffer.OutputDescription,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [
                ScreenSizeResourceLayout
            ],
            ShaderSet = new() {
                VertexLayouts = [
                    PositionVertex.Layout
                ],
                Shaders = shaders
            },
            RasterizerState = new() {
                CullMode = FaceCullMode.None,
                DepthClipEnabled = false,
                FillMode = PolygonFillMode.Solid,
                FrontFace = FrontFace.CounterClockwise,
                ScissorTestEnabled = false
            },
        }));
    }

    public override void Dispose() {
        ScreenSizeBuffer.Dispose();
        ScreenSizeResourceLayout.Dispose();
        ScreenSizeResourceSet.Dispose();
        base.Dispose();
    }
}
