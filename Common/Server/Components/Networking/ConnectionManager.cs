using Voxel.Common.Network;

namespace Voxel.Common.Server.Components.Networking;

public class ConnectionManager : ServerComponent {

    private readonly List<ServerConnectionContext> NewConnections = new();
    private readonly List<ServerConnectionContext> Connections = new();

    public event Action<ServerConnectionContext> OnConnectionMade = _ => {};

    public ConnectionManager(VoxelServer server) : base(server) {
    }

    public ServerConnectionContext AddConnection(S2CConnection newConnection) {
        var ctx = new ServerConnectionContext(newConnection);
        NewConnections.Add(ctx);
        OnConnectionMade(ctx);
        return ctx;
    }

    public override void OnServerStart() {

    }

    public override void Tick() {
        foreach (var connection in NewConnections) {
            Connections.Add(connection);
        }

        NewConnections.Clear();

        for (var i = Connections.Count - 1; i >= 0; i--) {
            var connection = Connections[i];

            if (connection.isDead)
                Connections.RemoveAt(i);
        }
    }

    public override void OnServerStop() {
        //Close all connections if the server stops.
        foreach (var connection in Connections)
            connection.Close();

        NewConnections.Clear();
        Connections.Clear();
    }
}
