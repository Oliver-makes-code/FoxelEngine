using System;
using GlmSharp;
using Voxel.Client.Keybinding;
using Voxel.Client.Rendering.World;
using Voxel.Common.Collision;

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
        
        if (Keybinds.Refresh.isPressed)
            WorldRenderer.ChunkRenderer.Reload();
        
        inputDir = inputDir.NormalizedSafe;

        if (Keybinds.LookLeft.isPressed)
            MainCamera.rotationVec.y += (float)delta;
        if (Keybinds.LookRight.isPressed)
            MainCamera.rotationVec.y -= (float)delta;
        if (Keybinds.LookUp.isPressed)
            MainCamera.rotationVec.x += (float)delta;
        if (Keybinds.LookDown.isPressed)
            MainCamera.rotationVec.x -= (float)delta;
        if (MainCamera.rotationVec.x < -MathF.PI/2)
            MainCamera.rotationVec.x = -MathF.PI/2;
        if (MainCamera.rotationVec.x > MathF.PI/2)
            MainCamera.rotationVec.x = MathF.PI/2;
        
        inputDir = MainCamera.rotationY * (vec3)inputDir;

        MainCamera.position += new AABB(
            MainCamera.position - new dvec3(0.3, 1.6, 0.3),
            MainCamera.position + new dvec3(0.3, 0.2, 0.3)
        ).MoveAndSlide(VoxelClient.Instance.world!, inputDir / 4);

        CameraStateManager.SetToCamera(MainCamera);

        WorldRenderer.Render(delta);
    }

    public override void Dispose() {
        WorldRenderer.Dispose();
    }
}
