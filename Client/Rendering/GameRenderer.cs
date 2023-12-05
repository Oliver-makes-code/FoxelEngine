using System;
using GlmSharp;
using Voxel.Client.Keybinding;
using Voxel.Client.Rendering.GUI;
using Voxel.Client.Rendering.World;
using Voxel.Common.Collision;
using Voxel.Common.World;

namespace Voxel.Client.Rendering;

public class GameRenderer : Renderer {

    /// <summary>
    /// Main camera, used to render main game window.
    /// Cannot be destroyed, it's essential for basic game rendering.
    /// </summary>
    public readonly Camera MainCamera;
    public readonly WorldRenderer WorldRenderer;
    public readonly GuiRenderer GuiRenderer;
    public readonly CameraStateManager CameraStateManager;

    public GameRenderer(VoxelClient client) : base(client) {
        //Jank but OK
        client.GameRenderer = this;

        MainCamera = new();
        CameraStateManager = new(client.RenderSystem);

        WorldRenderer = new(client);
        GuiRenderer = new(client);
    }

    public override void Render(double delta) {
        CameraStateManager.SetToCamera(MainCamera, Client.timeSinceLastTick);

        WorldRenderer.Render(delta);
        GuiRenderer.Render(delta);
    }

    public override void Dispose() {
        WorldRenderer.Dispose();
        GuiRenderer.Dispose();
    }
}
