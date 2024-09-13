using Foxel.Client.Rendering.VertexTypes.Entities;
using Foxel.Core.Assets;
using Foxel.Core.Rendering.Resources.Buffer;
using Veldrid;

namespace Foxel.Client.Rendering.World.Entities;

public sealed class EntityRenderer : Renderer {
    public readonly VertexBuffer<EntityModelVertex> ModelBuffer;

    public EntityRenderer(VoxelClient client, RenderPhase phase = RenderPhase.PostRender) : base(client, phase) {
        ModelBuffer = new(RenderSystem);

        ModelBuffer.UpdateImmediate([
            new() {
                position = new(-1, 0, 1),
                normal = new(0, 1, 0),
                uv = new(0, 0),
                uvMin = new(0, 0),
                uvMax = new(1, 1),
            },
            new() {
                position = new(1, 0, 1),
                normal = new(0, 1, 0),
                uv = new(0, 0),
                uvMin = new(0, 0),
                uvMax = new(1, 1),
            },
            new() {
                position = new(1, 0, -1),
                normal = new(0, 1, 0),
                uv = new(0, 0),
                uvMin = new(0, 0),
                uvMax = new(1, 1),
            },
            new() {
                position = new(-1, 0, -1),
                normal = new(0, 1, 0),
                uv = new(1, 1),
                uvMin = new(0, 0),
                uvMax = new(1, 1),
            }
        ]);
    }
}