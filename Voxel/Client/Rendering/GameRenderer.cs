using GlmSharp;
using Voxel.Client.Keybinding;
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

    public GameRenderer(VoxelClient client) : base(client) {
        //Jank but OK
        client.GameRenderer = this;

        MainCamera = new();
        CameraStateManager = new(client.RenderSystem);

        WorldRenderer = new(client);
    }

    public override void Render(double delta) {

        dvec3 inputDir = dvec3.Zero;

        if (Keybinds.StrafeLeft.isPressed)
            inputDir.x -= 1;
        if (Keybinds.StrafeRight.isPressed)
            inputDir.x += 1;
        if (Keybinds.Forward.isPressed)
            inputDir.z -= 1;
        if (Keybinds.Backward.isPressed)
            inputDir.z += 1;
        if (Keybinds.Crouch.isPressed)
            inputDir.y -= 1;
        if (Keybinds.Jump.isPressed)
            inputDir.y += 1;

        if (Keybinds.LookLeft.isPressed)
            MainCamera.rotation *= quat.FromAxisAngle((float)delta, new(0, 1, 0));
        if (Keybinds.LookRight.isPressed)
            MainCamera.rotation *= quat.FromAxisAngle((float)-delta, new(0, 1, 0));
        
        inputDir = MainCamera.rotation * (vec3)inputDir;
        MainCamera.position += inputDir * delta;

        CameraStateManager.SetToCamera(MainCamera);

        WorldRenderer.Render(delta);
    }

    public override void Dispose() {
        WorldRenderer.Dispose();
    }
}
