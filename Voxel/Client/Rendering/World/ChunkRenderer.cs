using System;
using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Common.Util;

namespace Voxel.Client.Rendering.World;

public class ChunkRenderer : Renderer {

    private ChunkRenderSlot[]? renderSlots;
    private int renderDistance = 0;
    private int realRenderDistance = 0;

    private ivec3 renderPosition = ivec3.Zero;

    public readonly Pipeline ChunkPipeline;
    public readonly ResourceLayout ChunkResourceLayout;

    private ChunkRenderSlot? this[int x, int y, int z] {
        get {
            if (renderSlots == null) return null;

            var index = z + y * realRenderDistance + x * realRenderDistance * realRenderDistance;

            return renderSlots[index];
        }
        set {
            if (renderSlots == null) return;

            var index = z + y * realRenderDistance + x * realRenderDistance * realRenderDistance;

            renderSlots[index] = value;
        }
    }

    private ChunkRenderSlot? this[ivec3 pos] {
        get => this[pos.x, pos.y, pos.z];
        set => this[pos.x, pos.y, pos.z] = value;
    }

    public ChunkRenderer(VoxelNewClient client) : base(client) {
        SetRenderDistance(5);

        //Chunk resources are just the model matrix (for now)
        ChunkResourceLayout = ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ModelMatrix", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        client.RenderSystem.ShaderManager.GetShaders("shaders/simple", out var shaders);

        ChunkPipeline = ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new DepthStencilStateDescription {
                DepthComparison = ComparisonKind.LessEqual,
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
            },
            Outputs = RenderSystem.GraphicsDevice.SwapchainFramebuffer.OutputDescription,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            RasterizerState = new RasterizerStateDescription {
                CullMode = FaceCullMode.Back,
                DepthClipEnabled = true,
                FillMode = PolygonFillMode.Solid,
                FrontFace = FrontFace.Clockwise,
                ScissorTestEnabled = false
            },
            ResourceLayouts = new[] {
                Client.GameRenderer.CameraStateManager.CameraResourceLayout,
                //RenderSystem.TextureManager.TextureResourceLayout, TODO - Textures!
                ChunkResourceLayout
            },
            ShaderSet = new() {
                VertexLayouts = new[] {
                    BasicVertex.Packed.Layout
                },
                Shaders = shaders ?? Array.Empty<Shader>()
            }
        });
    }

    public override void Render(double delta) {
        if (renderSlots == null)
            return;

        CommandList.SetPipeline(ChunkPipeline);

        //CommandList.SetGraphicsResourceSet(1, Client.GameRenderer.CameraStateManager.CameraResourceSet); //TODO - Textures!

        CommandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);
        foreach (var slot in renderSlots)
            slot.Render(delta);
    }

    public void SetRenderDistance(int distance) {
        if (renderSlots != null)
            foreach (var slot in renderSlots)
                slot.Dispose(); //Todo - Cache and re-use instead of dispose

        renderDistance = distance;
        realRenderDistance = ((renderDistance * 2) + 1);
        var totalChunks = realRenderDistance * realRenderDistance * realRenderDistance;
        renderSlots = new ChunkRenderSlot[totalChunks];

        for (int x = 0; x < realRenderDistance; x++)
        for (int y = 0; y < realRenderDistance; y++)
        for (int z = 0; z < realRenderDistance; z++) {
            var slot = new ChunkRenderSlot(Client, new ivec3(x, y, z) - distance);
            slot.Move(renderPosition);

            this[x, y, z] = slot;
        }

        //Sort by distance so that closer chunks are rebuilt first.
        Array.Sort(renderSlots, (a, b) => a.RelativePosition.LengthSqr.CompareTo(b.RelativePosition.LengthSqr));
    }

    public void SetRenderPosition(dvec3 worldPosition) {
        var newPos = worldPosition.WorldToChunkPosition();

        if (newPos == renderPosition || renderSlots == null)
            return;
        renderPosition = newPos;

        foreach (var slot in renderSlots)
            slot.Move(renderPosition);
    }

    public override void Dispose() {
        if (renderSlots == null)
            return;

        foreach (var slot in renderSlots)
            slot.Dispose();
        renderSlots = null;
    }
}
