using System;
using System.Runtime.InteropServices;
using Veldrid;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Client.Rendering.Texture;
using Voxel.Core.Assets;
using Voxel.Core.Rendering;
using Voxel.Client.Rendering.Utils;
using GlmSharp;
using Voxel.Client.World.Gui.Render;
using Voxel.Client.World.Gui;

namespace Voxel.Client.Rendering.Gui;

public class GuiRenderer : Renderer, IDisposable {
    public readonly GuiBuilder Builder = new();
    public readonly ReloadableDependency<Atlas> GuiAtlas;
    public readonly ResourceLayout ScreenDataResourceLayout;
    public readonly ResourceSet ScreenDataResourceSet;
    private readonly TypedDeviceBuffer<vec2> ScreenSizeBuffer;
    private readonly TypedDeviceBuffer<int> GuiScaleBuffer;
    private readonly DeviceBuffer InstanceBuffer;
    private DeviceBuffer? quadBuffer;
    private uint quadCount = 0;
    private GuiScreenRenderer? currentRenderer;

    public GuiRenderer(VoxelClient client) : base(client, RenderPhase.PreRender) {
        InstanceBuffer = ResourceFactory.CreateBuffer(new() {
            SizeInBytes = (uint)Marshal.SizeOf<PositionVertex>() * 4,
            Usage = BufferUsage.VertexBuffer | BufferUsage.Dynamic
        });

        CommandList.UpdateBuffer(InstanceBuffer, 0, [
            new Position2dVertex {
                position = new(-1, -1)
            },
            new Position2dVertex {
                position = new(1, -1)
            },
            new Position2dVertex {
                position = new(1, 1)
            },
            new Position2dVertex {
                position = new(-1, 1)
            }
        ]);

        ScreenDataResourceLayout = ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("ScreenSize", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
            new ResourceLayoutElementDescription("GuiScale", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        ScreenSizeBuffer = new(
            new() {
                Usage = BufferUsage.UniformBuffer | BufferUsage.Dynamic
            },
            RenderSystem
        );

        GuiScaleBuffer = new(
            new() {
                Usage = BufferUsage.UniformBuffer | BufferUsage.Dynamic
            },
            RenderSystem
        );

        ScreenDataResourceSet = ResourceFactory.CreateResourceSet(new() {
            Layout = ScreenDataResourceLayout,
            BoundResources = [
                ScreenSizeBuffer.BackingBuffer,
                GuiScaleBuffer.BackingBuffer
            ]
        });

        GuiAtlas = AtlasLoader.CreateDependency(new("gui"));
        DependsOn(GuiAtlas);

        WithResourceSet(0, () => {
            var screenSize = (vec2)Client.screenSize;
            CommandList.UpdateBuffer(ScreenSizeBuffer, 0, [new vec4(screenSize, 1/screenSize.x, 1/screenSize.y)]);
            CommandList.UpdateBuffer(GuiScaleBuffer, 0, [ClientConfig.General.guiScale]);
            return ScreenDataResourceSet;
        });

        WithResourceSet(1, () => GuiAtlas.value!.atlasResourceSet);
    }

    public override void Reload(PackManager packs, RenderSystem renderSystem, MainFramebuffer buffer) {
        base.Reload(packs, renderSystem, buffer);

        Builder.MarkForRebuild();
    }

    public override Pipeline? CreatePipeline(PackManager packs, MainFramebuffer buffer) {
        if (!Client.renderSystem!.ShaderManager.GetShaders(new("shaders/gui"), out var shaders))
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
                ScreenDataResourceLayout,
                RenderSystem.TextureManager.TextureResourceLayout
            ],
            ShaderSet = new() {
                VertexLayouts = [
                    Position2dVertex.Layout,
                    GuiQuadVertex.Layout
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

    public override void Render(double delta) {
        TryBuildScreen();

        Builder.BuildAll(GuiAtlas.value!, consumer => {
            quadBuffer?.Dispose();
            if (consumer.Count > 0) {
                quadCount = (uint)consumer.Count;
                quadBuffer = ResourceFactory.CreateBuffer(new() {
                    SizeInBytes = (uint)Marshal.SizeOf<GuiQuadVertex>() * (uint)consumer.Count,
                    Usage = BufferUsage.VertexBuffer | BufferUsage.Dynamic
                });
                CommandList.UpdateBuffer(quadBuffer, 0, consumer.AsSpan());
            } else {
                quadCount = 0;
                quadBuffer = null;
            }
        });

        if (quadBuffer == null || quadCount == 0)
            return;
        
        CommandList.SetVertexBuffer(0, InstanceBuffer);
        CommandList.SetVertexBuffer(1, quadBuffer);
        CommandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);
        CommandList.DrawIndexed(6, quadCount, 0, 0, 0);
    }

    public override void Dispose() {
        ScreenSizeBuffer.Dispose();
        ScreenDataResourceLayout.Dispose();
        ScreenDataResourceSet.Dispose();
        base.Dispose();
    }

    private void TryBuildScreen() {
        if (Client.screen == null)
            return;
        
        if (Client.screen != currentRenderer?.GetScreen()) {
            currentRenderer = GuiScreenRendererRegistry.GetRenderer(Client.screen);
            Builder.Clear();
            currentRenderer.Build(Builder);
        } else if (Client.screen.dirty) {
            Builder.Clear();
            currentRenderer.Build(Builder);
        }
    }
}
