using System;
using System.Runtime.InteropServices;
using Veldrid;
using Foxel.Client.Rendering.VertexTypes;
using Foxel.Client.Rendering.Texture;
using Foxel.Core.Assets;
using Foxel.Core.Rendering;
using Foxel.Client.Rendering.Utils;
using GlmSharp;
using Foxel.Client.World.Gui.Render;
using Foxel.Client.World.Gui;

namespace Foxel.Client.Rendering.Gui;

public class GuiRenderer : Renderer, IDisposable {
    public readonly GuiBuilder Builder = new();
    public readonly ReloadableDependency<Atlas> GuiAtlas;
    public readonly ResourceLayout ScreenDataResourceLayout;
    public readonly ResourceSet ScreenDataResourceSet;
    private readonly TypedDeviceBuffer<vec2> ScreenSizeBuffer;
    private readonly TypedDeviceBuffer<int> GuiScaleBuffer;
    private readonly TypedVertexBuffer<Position2dVertex> InstanceBuffer;
    private readonly TypedVertexBuffer<GuiQuadVertex> QuadBuffer;
    private GuiScreenRenderer? currentRenderer;

    public GuiRenderer(VoxelClient client) : base(client, RenderPhase.PreRender) {
        InstanceBuffer = new(ResourceFactory);

        var consumer = new VertexConsumer<Position2dVertex>()
            .Vertex(new() {
                position = new(-1, 1)
            })
            .Vertex(new() {
                position = new(1, 1)
            })
            .Vertex(new() {
                position = new(1, -1)
            })
            .Vertex(new() {
                position = new(-1, -1)
            });
        
        InstanceBuffer.Update(consumer, CommandList, ResourceFactory);

        QuadBuffer = new(ResourceFactory);

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

        GuiAtlas = GuiAtlasLoader.CreateDependency(new("gui"), Client);

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
            BlendState = new() {
                AttachmentStates = [
                    BlendAttachmentDescription.AlphaBlend,
                    BlendAttachmentDescription.OverrideBlend,
                ]
            },
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
                CullMode = FaceCullMode.Front,
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
            QuadBuffer.Update(consumer, CommandList, ResourceFactory);
        });

        if (QuadBuffer.size == 0)
            return;
        
        CommandList.SetVertexBuffer(0, InstanceBuffer.buffer);
        CommandList.SetVertexBuffer(1, QuadBuffer.buffer);
        CommandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);
        CommandList.DrawIndexed(6, QuadBuffer.size, 0, 0, 0);
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
