using System;
using RenderSurface;
using Voxel.Client.Rendering;
using Voxel.Client.World;

namespace Voxel.Client;

public class VoxelNewClient : Game {

    public GameRenderer? gameRenderer { get; set; }

    public ClientWorld? world { get; private set; }

    public override void Init() {
        world = new();

        gameRenderer = new(this);
    }

    public override void OnFrame(double delta) {
        gameRenderer.Render(delta);
    }


    public override void OnTick() {

    }

    public override void Dispose() {
        gameRenderer.Dispose();
        base.Dispose();
    }
}
