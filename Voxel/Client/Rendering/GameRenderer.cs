using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.World;

namespace Voxel.Client.Rendering;

public class GameRenderer : Renderer {

    /// <summary>
    /// Main camera, used to render main game window.
    /// Cannot be destroyed, it's essential for basic game rendering.
    /// </summary>
    public readonly Camera MainCamera;
    public readonly WorldRenderer WorldRenderer;
    public readonly CameraStateManager CameraStateManager;

    public GameRenderer(VoxelNewClient client) : base(client) {
        //Jank but OK
        client.gameRenderer = this;

        MainCamera = new();
        CameraStateManager = new(client.renderSystem);

        WorldRenderer = new(client);
    }

    public override void Render(double delta) {
        CameraStateManager.SetToCamera(MainCamera);

        WorldRenderer.Render(delta);
    }

    public override void Dispose() {
        WorldRenderer.Dispose();
    }
}
