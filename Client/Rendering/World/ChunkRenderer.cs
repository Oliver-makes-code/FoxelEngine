using System;
using System.Collections.Generic;
using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.Models;
using Voxel.Client.Rendering.Texture;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Common.Util;
using Voxel.Common.World;

namespace Voxel.Client.Rendering.World;

public class ChunkRenderer : Renderer {
    public readonly Pipeline ChunkPipeline;
    public readonly ResourceLayout ChunkResourceLayout;

    public readonly Atlas TerrainAtlas;

    public LoadedChunkSection chunks;

    private ChunkRenderSlot[]? renderSlots;
    private List<ChunkRenderSlot> createdRenderSlots = new();
    private int renderDistance = 0;
    private int realRenderDistance = 0;

    private ivec3 renderPosition = ivec3.Zero;

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

    public ChunkRenderer(VoxelClient client) : base(client) {
        TerrainAtlas = new("main", client.RenderSystem);
        AtlasLoader.LoadAtlas(RenderSystem.Game.AssetReader, TerrainAtlas, RenderSystem);
        BlockModelManager.Init(RenderSystem.Game.AssetReader, TerrainAtlas);

        //Chunk resources are just the model matrix (for now)
        ChunkResourceLayout = ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ModelMatrix", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        if (!client.RenderSystem.ShaderManager.GetShaders("shaders/simple", out var shaders))
            throw new("Shaders not present.");

        ChunkPipeline = ResourceFactory.CreateGraphicsPipeline(new() {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new() {
                DepthComparison = ComparisonKind.LessEqual, DepthTestEnabled = true, DepthWriteEnabled = true,
            },
            Outputs = RenderSystem.GraphicsDevice.SwapchainFramebuffer.OutputDescription,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            RasterizerState = new() {
                CullMode = FaceCullMode.Back,
                DepthClipEnabled = true,
                FillMode = PolygonFillMode.Solid,
                FrontFace = FrontFace.CounterClockwise,
                ScissorTestEnabled = false
            },
            ResourceLayouts = new[] {
                Client.GameRenderer.CameraStateManager.CameraResourceLayout,
                RenderSystem.TextureManager.TextureResourceLayout,
                ChunkResourceLayout
            },
            ShaderSet = new() {
                VertexLayouts = new[] {
                    BasicVertex.Packed.Layout
                },
                Shaders = shaders
            }
        });
    }

    public void SetWorld(VoxelWorld world) {
        chunks = new(world, renderPosition, ClientConfig.General.renderDistance, ClientConfig.General.renderDistance);
        SetRenderDistance(ClientConfig.General.renderDistance);
    }

    public void Reload() {
        if (renderSlots == null)
            return;

        foreach (var slot in renderSlots)
            slot.lastVersion = null;
    }

    public override void Render(double delta) {
        if (renderSlots == null)
            return;

        CommandList.SetPipeline(ChunkPipeline);

        RenderSystem.MainCommandList.SetGraphicsResourceSet(0, Client.GameRenderer.CameraStateManager.CameraResourceSet);
        CommandList.SetGraphicsResourceSet(1, TerrainAtlas.AtlasResourceSet);

        CommandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);
        foreach (var slot in createdRenderSlots)
            slot.Render(delta);

        //Console.Out.WriteLine();
    }

    public void SetRenderDistance(int distance) {
        chunks.Resize(distance, distance);
        if (renderSlots != null) {
            foreach (var slot in renderSlots)
                slot.Dispose(); //Todo - Cache and re-use instead of dispose
            Array.Fill(renderSlots, null);
        }

        renderDistance = distance;
        realRenderDistance = renderDistance * 2 + 1;
        var totalChunks = realRenderDistance * realRenderDistance * realRenderDistance;
        renderSlots = new ChunkRenderSlot[totalChunks];

        renderPosition = ivec3.MinValue;
        SetRenderPosition(Client.GameRenderer.MainCamera.position);
    }

    public void SetRenderPosition(dvec3 worldPosition) {
        var newPos = worldPosition.WorldToChunkPosition();

        if (newPos == renderPosition || renderSlots == null)
            return;

        renderPosition = newPos;

        chunks.Move(newPos);

        for (int x = 0; x < realRenderDistance; x++)
        for (int y = 0; y < realRenderDistance; y++)
        for (int z = 0; z < realRenderDistance; z++) {
            var absolutePos = (new ivec3(x, y, z) - renderDistance) + renderPosition;
            var index = GetLoopedArrayIndex(absolutePos);
            var slot = renderSlots[index];

            if (slot == null) {
                renderSlots[index] = slot = new ChunkRenderSlot(Client);
                createdRenderSlots.Add(slot);
            }

            slot.Move(absolutePos, chunks);
        }

        //Sort by distance so that closer chunks are rebuilt first.
        createdRenderSlots.Sort((a, b) => (a.RealPosition - renderPosition).LengthSqr.CompareTo((b.RealPosition - renderPosition).LengthSqr));
    }


    private int GetLoopedArrayIndex(ivec3 pos) {
        pos = new(MathHelper.Repeat(pos.x, realRenderDistance), MathHelper.Repeat(pos.y, realRenderDistance), MathHelper.Repeat(pos.z, realRenderDistance));
        return pos.z + pos.y * realRenderDistance + pos.x * realRenderDistance * realRenderDistance;
    }

    public override void Dispose() {
        if (renderSlots == null)
            return;

        foreach (var slot in renderSlots)
            slot.Dispose();
        renderSlots = null;
    }
}
