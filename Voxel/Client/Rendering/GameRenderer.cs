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
        client.GameRenderer = this;

        MainCamera = new();
        CameraStateManager = new(client.RenderSystem);

        WorldRenderer = new(client);
    }

    public override void Render(double delta) {

        dvec3 inputDir = dvec3.Zero;

        if (Client.InputManager.IsKeyPressed(Key.A))
            inputDir.x -= 1;
        if (Client.InputManager.IsKeyPressed(Key.D))
            inputDir.x += 1;
        if (Client.InputManager.IsKeyPressed(Key.W))
            inputDir.z -= 1;
        if (Client.InputManager.IsKeyPressed(Key.S))
            inputDir.z += 1;
        if (Client.InputManager.IsKeyPressed(Key.E))
            inputDir.y -= 1;
        if (Client.InputManager.IsKeyPressed(Key.Q))
            inputDir.y += 1;

        if (Client.InputManager.IsKeyPressed(Key.Z))
            MainCamera.rotation *= quat.FromAxisAngle((float)delta, new vec3(0, 1, 0));
        if (Client.InputManager.IsKeyPressed(Key.X))
            MainCamera.rotation *= quat.FromAxisAngle((float)-delta, new vec3(0, 1, 0));

        inputDir = MainCamera.rotation * (vec3)inputDir;
        MainCamera.position += inputDir * delta;

        CameraStateManager.SetToCamera(MainCamera);

        WorldRenderer.Render(delta);
    }

    public override void Dispose() {
        WorldRenderer.Dispose();
    }
}
