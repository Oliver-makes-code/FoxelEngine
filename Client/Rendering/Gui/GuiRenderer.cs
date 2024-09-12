using System;
using Veldrid;
using Foxel.Client.Rendering.VertexTypes;
using Foxel.Client.Rendering.Texture;
using Foxel.Core.Assets;
using Foxel.Core.Rendering;
using GlmSharp;
using Foxel.Client.World.Gui.Render;
using Foxel.Client.World.Gui;
using Foxel.Core.Rendering.Buffer;

namespace Foxel.Client.Rendering.Gui;

public class GuiRenderer : Renderer, IDisposable {
    public readonly GuiBuilder Builder = new();
    public readonly ReloadableDependency<Atlas> GuiAtlas;
    public readonly ResourceLayout ScreenDataResourceLayout;
    public readonly ResourceSet ScreenDataResourceSet;
    private readonly TypedGraphicsBuffer<vec2> ScreenSizeBuffer;
    private readonly TypedGraphicsBuffer<int> GuiScaleBuffer;
    private readonly VertexBuffer<Position2dVertex> InstanceBuffer;
    private readonly VertexBuffer<GuiQuadVertex> QuadBuffer;
    private GuiScreenRenderer? currentRenderer;

    public GuiRenderer(VoxelClient client) : base(client) {
        InstanceBuffer = new(RenderSystem);

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
        InstanceBuffer.UpdateImmediate(consumer);

        QuadBuffer = new(RenderSystem);

        ScreenDataResourceLayout = ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("ScreenSize", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
            new ResourceLayoutElementDescription("GuiScale", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        ScreenSizeBuffer = new(RenderSystem, GraphicsBufferUsage.UniformBuffer | GraphicsBufferUsage.Dynamic);
        ScreenSizeBuffer.WithCapacity(2);

        GuiScaleBuffer = new(RenderSystem, GraphicsBufferUsage.UniformBuffer | GraphicsBufferUsage.Dynamic);
        GuiScaleBuffer.WithCapacity(1);

        ScreenDataResourceSet = ResourceFactory.CreateResourceSet(new() {
            Layout = ScreenDataResourceLayout,
            BoundResources = [
                ScreenSizeBuffer.baseBuffer,
                GuiScaleBuffer.baseBuffer
            ]
        });

        GuiAtlas = GuiAtlasLoader.CreateDependency(new("gui"), Client);

        WithResourceSet(0, () => {
            var screenSize = (vec2)Client.screenSize;
            ScreenSizeBuffer.UpdateDeferred(0, [screenSize, 1/screenSize]);
            GuiScaleBuffer.UpdateDeferred(0, [ClientConfig.General.guiScale]);
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
                ]
            },
            DepthStencilState = new() {
                DepthComparison = ComparisonKind.Never,
                DepthTestEnabled = false,
                DepthWriteEnabled = false,
            },
            Outputs = buffer.WindowFramebuffer.OutputDescription,
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
                CullMode = FaceCullMode.Back,
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
            QuadBuffer.UpdateImmediate(consumer);
        });

        if (QuadBuffer.size == 0)
            return;

        RenderSystem.SetFramebuffer(Client.gameRenderer!.frameBuffer!.WindowFramebuffer);
        
        InstanceBuffer.Bind(0);
        QuadBuffer.Bind(1);
        RenderSystem.CommonIndexBuffer.Bind();
        RenderSystem.DrawIndexed(6, QuadBuffer.size);
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
