namespace Common.Server.Components;

public class WorldManager : ServerComponent {
    public WorldManager(VoxelServer server) : base(server) {
    }
    public override void OnServerStart() => throw new NotImplementedException();
    public override void Tick() => throw new NotImplementedException();
    public override void OnServerStop() => throw new NotImplementedException();
}
