using Foxel.Common.Server.World;

namespace Foxel.Common.Server.Components;

public class WorldManager : ServerComponent {

    public readonly ServerWorld DefaultWorld;


    private readonly Dictionary<string, ServerWorld> Worlds = [];
    public WorldManager(VoxelServer server) : base(server) {

        Worlds["Overworld"] = DefaultWorld = new(server);
    }

    public override void OnServerStart() {}
    
    public override void Tick() {
        foreach (var world in Worlds.Values)
            world.Tick();
    }

    public override void OnServerStop() {}
}
