using Voxel.Common.Server.World;

namespace Voxel.Common.Server.Components;

public class WorldManager : ServerComponent {

    private readonly Dictionary<string, ServerWorld> Worlds = new();

    public readonly ServerWorld DefaultWorld;

    public WorldManager(VoxelServer server) : base(server) {

        Worlds["Overworld"] = DefaultWorld = new ServerWorld();
    }

    public override void OnServerStart() {

    }
    public override void Tick() {

    }
    public override void OnServerStop() {

    }
}
