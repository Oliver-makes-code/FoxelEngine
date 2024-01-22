using System;
using System.Collections.Generic;
using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.Models;
using Voxel.Client.Rendering.Texture;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Common.Collision;
using Voxel.Common.Util;
using Voxel.Common.Util.Profiling;
using Voxel.Common.World;
using Voxel.Core.Util;

namespace Voxel.Client.Rendering.World;

public class ChunkRenderer : Renderer {

    private static readonly Profiler.ProfilerKey RenderKey = Profiler.GetProfilerKey("Render Chunks");

    public Pipeline ChunkPipeline;
    public readonly ResourceLayout ChunkResourceLayout;

    public readonly Atlas TerrainAtlas;

    private ChunkRenderSlot[]? renderSlots;
    private List<ChunkRenderSlot> createdRenderSlots = new();
    private int renderDistance = 0;
    private int realRenderDistance = 0;

    private ivec3 renderPosition = ivec3.Zero;

    public ChunkRenderer(VoxelClient client) : base(client) {
        TerrainAtlas = new("main", client.RenderSystem);
        AtlasLoader.LoadAtlas(RenderSystem.Game.AssetReader, TerrainAtlas, RenderSystem);
        BlockModelManager.Init(RenderSystem.Game.AssetReader, TerrainAtlas);

        //Chunk resources are just the model matrix (for now)
        ChunkResourceLayout = ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ModelMatrix", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));
    }

    public override void CreatePipeline(MainFramebuffer framebuffer) {
        if (!Client.RenderSystem.ShaderManager.GetShaders("shaders/terrain", out var shaders))
            throw new("Shaders not present.");

        ChunkPipeline = framebuffer.AddDependency(ResourceFactory.CreateGraphicsPipeline(new() {
            BlendState = new BlendStateDescription {
                AttachmentStates = new[] {
                    BlendAttachmentDescription.OverrideBlend,
                    BlendAttachmentDescription.OverrideBlend
                }
            },
            DepthStencilState = new() {
                DepthComparison = ComparisonKind.LessEqual, DepthTestEnabled = true, DepthWriteEnabled = true,
            },
            Outputs = framebuffer.Framebuffer.OutputDescription,
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
                ChunkResourceLayout,
                RenderSystem.TextureManager.TextureResourceLayout,
            },
            ShaderSet = new() {
                VertexLayouts = new[] {
                    BasicVertex.Packed.Layout
                },
                Shaders = shaders
            }
        }));
    }

    public void SetWorld(VoxelWorld world) {
        SetRenderDistance(ClientConfig.General.renderDistance);
    }

    public void Reload() {
        if (renderSlots == null)
            return;

        foreach (var slot in renderSlots)
            slot.Reload();
    }

    public override void Render(double delta) {
        if (renderSlots == null)
            return;

        SetRenderPosition(Client.GameRenderer.MainCamera.position);

        CommandList.SetPipeline(ChunkPipeline);

        CommandList.SetGraphicsResourceSet(0, Client.GameRenderer.CameraStateManager.CameraResourceSet);
        CommandList.SetGraphicsResourceSet(2, TerrainAtlas.AtlasResourceSet);

        CommandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);

        using (RenderKey.Push()) {
            var queue = new Queue<ivec3>();
            var visited = new HashSet<ivec3>();
            queue.Add(ivec3.Zero);
            visited.Add(ivec3.Zero);
            var rootPos = Client.GameRenderer.MainCamera.position.WorldToChunkPosition();

            ivec3[] directions = [
                new(1, 0, 0), new(-1, 0, 0),
                new(0, 1, 0), new(0, -1, 0),
                new(0, 0, 1), new(0, 0, -1)
            ];

            var frustum = Client.GameRenderer.MainCamera.Frustum;

            while (queue.Count > 0) {
                var curr = queue.Remove();
                var index = GetLoopedArrayIndex(curr + rootPos);
                var chunk = renderSlots[index];
                if (chunk == null)
                    continue;
                chunk.Render(delta);

                foreach (var dir in directions) {
                    var pos = dir + curr;
                    var slotPos = pos + renderDistance;
                    var realPos = pos + rootPos;
                    if (
                        visited.Contains(pos) ||
                        (slotPos < 0).Any ||
                        (slotPos >= realRenderDistance).Any ||
                        !frustum.TestAABB(new(
                            realPos.ChunkToWorldPosition(),
                            (realPos + 1).ChunkToWorldPosition()
                        ))
                    )
                        continue;
                    queue.Add(pos);
                    visited.Add(pos);
                }
            }

            Profiler.SetCurrentMeta($"{visited.Count} / {renderSlots.Length} ({(int)(visited.Count / (float) renderSlots.Length * 100)}%)");
        
            // foreach (var slot in createdRenderSlots)
            //     slot.Render(delta);
        }

        //Console.Out.WriteLine();
    }

    public void SetRenderDistance(int distance) {
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

        foreach (var pos in Iteration.Cubic(realRenderDistance)) {
            var localPos = pos - renderDistance;
            var absolutePos = (pos - renderDistance) + renderPosition;
            var index = GetLoopedArrayIndex(absolutePos);
            var slot = renderSlots[index];

            if (slot == null) {
                renderSlots[index] = slot = new ChunkRenderSlot(Client);
                createdRenderSlots.Add(slot);
            }

            slot.Move(absolutePos, Client.world!);
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
