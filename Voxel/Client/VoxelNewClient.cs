using System;
using RenderSurface;
using Voxel.Client.Rendering;
using Voxel.Client.World;

namespace Voxel.Client;

public class VoxelNewClient : Game {

    public GameRenderer GameRenderer { get; set; }

    public ClientWorld? World { get; private set; }

    public override void Init() {
        World = new ClientWorld();

        GameRenderer = new(this);
    }

    public override void OnFrame(double delta) {
        GameRenderer.Render(delta);
    }


    public override void OnTick() {

    }

    public override void Dispose() {
        GameRenderer.Dispose();
        base.Dispose();
    }
}
