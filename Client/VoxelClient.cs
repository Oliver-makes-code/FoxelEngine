using System;
using GlmSharp;
using RenderSurface;
using Voxel.Client.Keybinding;
using Voxel.Client.Rendering;
using Voxel.Client.Rendering.World;
using Voxel.Client.World;
using Voxel.Common.Util;
using Voxel.Common.World;

namespace Voxel.Client;

public class VoxelClient : Game {
    public static VoxelClient Instance { get; private set; }

    public GameRenderer GameRenderer { get; set; }

    public ClientWorld? world { get; private set; }

    public double timeSinceLastTick = 0;

    public VoxelClient() {
        Instance = this;
    }

    public override void Init() {
        ClientConfig.Load();
        ClientConfig.Save();
        
        world = new();

        GameRenderer = new(this);
        
        GameRenderer.MainCamera.aspect = (float)NativeWindow.Width / NativeWindow.Height;
    }

    public override void OnFrame(double delta, double tickAccumulator) {
        timeSinceLastTick = tickAccumulator;
        GameRenderer.Render(delta);

        ImGuiNET.ImGui.ShowMetricsWindow();
    }
    
    public override void OnTick() {
        Keybinds.Poll();
        
        timeSinceLastTick = 0;
        
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

        var move = Keybinds.Move.axis;
        
        inputDir.x += move.x;
        inputDir.z += move.y;
        
        if (Keybinds.Refresh.isPressed)
            GameRenderer.WorldRenderer.ChunkRenderer.Reload();
        
        inputDir = inputDir.NormalizedSafe;

        var camera = GameRenderer.MainCamera;
        
        camera.oldPosition = camera.position;
        camera.oldRotationVec = camera.rotationVec;

        if (Keybinds.LookLeft.isPressed)
            camera.rotationVec.y += 0.125f * (float)Keybinds.LookLeft.strength;
        if (Keybinds.LookRight.isPressed)
            camera.rotationVec.y -= 0.125f * (float)Keybinds.LookRight.strength;
        if (Keybinds.LookUp.isPressed)
            camera.rotationVec.x += 0.125f * (float)Keybinds.LookUp.strength;
        if (Keybinds.LookDown.isPressed)
            camera.rotationVec.x -= 0.125f * (float)Keybinds.LookDown.strength;
        
        camera.rotationVec += -(vec2)Keybinds.Look.axis.swizzle.yx * 0.125f;
        
        if (camera.rotationVec.x < -MathF.PI/2)
            camera.rotationVec.x = -MathF.PI/2;
        if (camera.rotationVec.x > MathF.PI/2)
            camera.rotationVec.x = MathF.PI/2;
        
        inputDir = camera.rotationY * (vec3)inputDir;
        inputDir /= 4;
        
        camera.MoveAndSlide(world!, inputDir);
        
        // GameRenderer.WorldRenderer.ChunkRenderer.SetRenderPosition(camera.position);
    }

    public override void OnWindowResize() {
        base.OnWindowResize();

        GameRenderer.MainCamera.aspect = (float)NativeWindow.Width / NativeWindow.Height;
    }

    public override void Dispose() {
        GameRenderer.Dispose();
        base.Dispose();
    }
}
