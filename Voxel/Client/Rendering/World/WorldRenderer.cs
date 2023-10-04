namespace Voxel.Client.Rendering.World;

public class WorldRenderer : Renderer {

    public readonly ChunkRenderer ChunkRenderer;

    public WorldRenderer(VoxelNewClient client) : base(client) {
        ChunkRenderer = new(client);
    }

    public override void Render(double delta) {

        if (Client.World == null)
            return;

        ChunkRenderer.Render(delta);
    }

    public override void Dispose() {
        ChunkRenderer.Dispose();
    }
}
