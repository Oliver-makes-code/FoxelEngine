using System;
using Veldrid;
using Voxel.Common.World;
using Voxel.Core.Assets;

namespace Voxel.Client.Rendering.World;

public class WorldRenderer : Renderer {

    public readonly ChunkRenderer ChunkRenderer;
    public VoxelWorld? targetWorld;


    public WorldRenderer(VoxelClient client) : base(client, RenderPhase.PreRender) {
        ChunkRenderer = new(client);
        DependsOn(ChunkRenderer);
    }

    public override void Render(double delta) {
        if (Client.playerEntity?.world != targetWorld) {
            targetWorld = Client.playerEntity?.world;

            if (Client.playerEntity?.world != null)
                ChunkRenderer.SetWorld(Client.playerEntity.world);
        }

        if (targetWorld == null)
            return;
    }
}
