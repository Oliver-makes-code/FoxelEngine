using Voxel.Client.Rendering.World;

namespace Voxel.Client.Rendering;

public class GameRenderer : Renderer {

    public readonly WorldRenderer WorldRenderer;

    public GameRenderer(VoxelNewClient client) : base(client) {
        WorldRenderer = new(client);
    }

    public override void Render(double delta) {
        WorldRenderer.Render(delta);
    }

    public override void Dispose() {
        WorldRenderer.Dispose();
    }
}
