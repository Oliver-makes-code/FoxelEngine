using System;
using Voxel.Common.World;

namespace Voxel.Client.Rendering.World;

public class WorldRenderer : Renderer {

    public readonly ChunkRenderer ChunkRenderer;
    public VoxelWorld? targetWorld;


    public WorldRenderer(VoxelClient client) : base(client) {
        ChunkRenderer = new(client);
    }

    public override void CreatePipeline(MainFramebuffer framebuffer) {
        ChunkRenderer.CreatePipeline(framebuffer);
    }

    public override void Render(double delta) {

        if (Client.PlayerEntity?.world != targetWorld) {
            targetWorld = Client.PlayerEntity?.world;

            if (Client.PlayerEntity?.world != null)
                ChunkRenderer.SetWorld(Client.PlayerEntity.world);
        }

        if (targetWorld == null)
            return;

        ChunkRenderer.Render(delta);
    }

    public override void Dispose() {
        ChunkRenderer.Dispose();
    }
}
