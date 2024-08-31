namespace Foxel.Common.Server.Components;

public abstract class ServerComponent {
    public readonly VoxelServer Server;
    
    public ServerComponent(VoxelServer server) {
        Server = server;
    }

    public abstract void OnServerStart();
    public abstract void Tick();
    public abstract void OnServerStop();
}
