using Common.Network;
using Common.Network.Packets;

namespace Common.Server.Components.Networking;

public class NetworkManager : ServerComponent {

    private readonly List<ServerConnectionContext> NewConnections = new();
    private readonly List<ServerConnectionContext> Connections = new();

    public NetworkManager(VoxelServer server) : base(server) {
    }

    public ServerConnectionContext AddConnection(S2CConnection newConnection) {
        var ctx = new ServerConnectionContext(newConnection);
        NewConnections.Add(ctx);
        return ctx;
    }

    public override void OnServerStart() {

    }

    public override void Tick() {
        Connections.AddRange(NewConnections);
        NewConnections.Clear();

        for (var i = Connections.Count - 1; i >= 0; i--) {
            var connection = Connections[i];

            connection.Tick();

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
