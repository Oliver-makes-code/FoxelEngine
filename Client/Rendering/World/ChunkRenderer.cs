using System;
using System.Collections.Generic;
using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.Models;
using Voxel.Client.Rendering.Texture;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Common.Util;
using Voxel.Core.Util.Profiling;
using Voxel.Common.World;
using Voxel.Core.Util;
using Voxel.Core.Assets;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.World;

public class ChunkRenderer : Renderer {
    private static readonly Profiler.ProfilerKey RenderKey = Profiler.GetProfilerKey("Render Chunks");

    private static readonly ivec3[] Directions = [
        new(1, 0, 0), new(-1, 0, 0),
        new(0, 1, 0), new(0, -1, 0),
        new(0, 0, 1), new(0, 0, -1)
    ];
    public readonly ResourceLayout ChunkResourceLayout;

    public readonly ReloadableDependency<Atlas> TerrainAtlas;

    private readonly Queue<ivec3> ChunkQueue = [];

    private BitVector? visitedChunks;
    private ChunkRenderSlot[]? renderSlots;
    private List<ChunkRenderSlot> createdRenderSlots = new();
    private int renderDistance = 0;
    private int realRenderDistance = 0;

    private ivec3 renderPosition = ivec3.Zero;

    public ChunkRenderer(VoxelClient client) : base(client, RenderPhase.PostRender) {
        TerrainAtlas = AtlasLoader.CreateDependency(new("terrain"));
        DependsOn(TerrainAtlas);
        // Make sure it gets initialized
        var _ = BlockModelManager.ReloadTask;

        //Chunk resources are just the model matrix (for now)
        ChunkResourceLayout = ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ModelMatrix", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        WithResourceSet(0, () => Client.gameRenderer!.CameraStateManager.CameraResourceSet);
        WithResourceSet(2, () => TerrainAtlas.value!.atlasResourceSet);
    }

    public override Pipeline CreatePipeline(PackManager packs, MainFramebuffer framebuffer) {
        if (!Client.RenderSystem.ShaderManager.GetShaders("shaders/terrain", out var shaders))
            throw new("Shaders not present.");

        return framebuffer.AddDependency(ResourceFactory.CreateGraphicsPipeline(new() {
            BlendState = new() {
                AttachmentStates = [
                    BlendAttachmentDescription.OverrideBlend
                ]
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
            ResourceLayouts = [
                Client.gameRenderer!.CameraStateManager.CameraResourceLayout,
                ChunkResourceLayout,
                RenderSystem.TextureManager.TextureResourceLayout,
            ],
            ShaderSet = new() {
                VertexLayouts = [
                    TerrainVertex.Packed.Layout
                ],
                Shaders = shaders
            }
        }));
    }

    public void SetWorld(VoxelWorld world) {
        SetRenderDistance(ClientConfig.General.renderDistance);
    }

    public override void Reload(PackManager packs, RenderSystem renderSystem, MainFramebuffer buffer) {
        base.Reload(packs, renderSystem, buffer);

        if (renderSlots == null)
            return;

        foreach (var slot in renderSlots)
            slot.Reload();
    }

    public override void Render(double delta) {
        if (renderSlots == null)
            return;

        SetRenderPosition(Client.gameRenderer!.MainCamera.position);

        CommandList.SetIndexBuffer(RenderSystem.CommonIndexBuffer, IndexFormat.UInt32);

        using (RenderKey.Push()) {
            visitedChunks ??= new(renderSlots.Length);
            ChunkQueue.Clear();
            visitedChunks.Clear();
            ChunkQueue.Add(ivec3.Zero);
            var rootPos = Client.gameRenderer.MainCamera.position.WorldToChunkPosition();
            visitedChunks.Set(GetLoopedArrayIndex(rootPos));

            var frustum = Client.gameRenderer.MainCamera.Frustum;

            int count = 0;

            while (ChunkQueue.Count > 0) {
                var curr = ChunkQueue.Remove();
                int index = GetLoopedArrayIndex(curr + rootPos);
                var chunk = renderSlots[index];
                if (chunk == null)
                    continue;
                count++;
                chunk.Render(delta);

                foreach (var dir in Directions) {
                    var pos = dir + curr;
                    var slotPos = pos + renderDistance;
                    var realPos = pos + rootPos;
                    int idx = GetLoopedArrayIndex(realPos);
                    if (
                        visitedChunks.Get(idx) ||
                        (slotPos < 0).Any ||
                        (slotPos >= realRenderDistance).Any ||
                        !frustum.TestBox(new(
                            realPos.ChunkToWorldPosition(),
                            (realPos + 1).ChunkToWorldPosition()
                        ))
                    )
                        continue;
                    ChunkQueue.Add(pos);
                    visitedChunks.Set(idx);
                }
            }

            Profiler.SetCurrentMeta($"{count} / {renderSlots.Length} ({(int)(count / (float) renderSlots.Length * 100)}%)");
        
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
        int totalChunks = realRenderDistance * realRenderDistance * realRenderDistance;
        renderSlots = new ChunkRenderSlot[totalChunks];
        visitedChunks = new(totalChunks);

        renderPosition = ivec3.MinValue;
        SetRenderPosition(Client.gameRenderer!.MainCamera.position);
    }

    public void SetRenderPosition(dvec3 worldPosition) {
        var newPos = worldPosition.WorldToChunkPosition();

        if (newPos == renderPosition || renderSlots == null)
            return;

        renderPosition = newPos;

        foreach (var pos in Iteration.Cubic(realRenderDistance)) {
            var localPos = pos - renderDistance;
            var absolutePos = (pos - renderDistance) + renderPosition;
            int index = GetLoopedArrayIndex(absolutePos);
            var slot = renderSlots[index];

            if (slot == null) {
                renderSlots[index] = slot = new(Client);
                createdRenderSlots.Add(slot);
            }

            slot.Move(absolutePos, Client.world!);
        }

        //Sort by distance so that closer chunks are rebuilt first.
        createdRenderSlots.Sort((a, b) => (a.RealPosition - renderPosition).LengthSqr.CompareTo((b.RealPosition - renderPosition).LengthSqr));
    }

    public override void Dispose() {
        base.Dispose();
        if (renderSlots == null)
            return;

        foreach (var slot in renderSlots)
            slot.Dispose();
        renderSlots = null;
    }

    private int GetLoopedArrayIndex(ivec3 pos) {
        pos = new(MathHelper.Repeat(pos.x, realRenderDistance), MathHelper.Repeat(pos.y, realRenderDistance), MathHelper.Repeat(pos.z, realRenderDistance));
        return pos.z + pos.y * realRenderDistance + pos.x * realRenderDistance * realRenderDistance;
    }
}
