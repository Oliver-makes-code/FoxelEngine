using System;
using RenderSurface;
using Voxel.Client.Keybinding;
using Voxel.Client.Rendering;
using Voxel.Client.World;
using Voxel.Common.World;

namespace Voxel.Client;

public class VoxelClient : Game {
    public static VoxelClient Instance { get; private set; }

    public GameRenderer GameRenderer { get; set; }

    public ClientWorld? world { get; private set; }

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

    public override void OnFrame(double delta) {
        GameRenderer.Render(delta);

        ImGuiNET.ImGui.ShowMetricsWindow();
    }
    
    public override void OnTick() {
        Keybinds.Poll();
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
