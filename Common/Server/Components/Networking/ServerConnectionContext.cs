using Common.Network;
using Common.Network.Packets;
using Voxel.Common.Util;

namespace Common.Server.Components.Networking;

/// <summary>
/// This is what controls a connection to a player from the server.
///
/// Holds stuff like connection status and whatnot.
/// </summary>
public class ServerConnectionContext {
    public bool isDead => Connection.isDead;

    private readonly S2CConnection Connection;

    private readonly PacketHandler HandshakeHandler;
    private readonly PacketHandler GameplayHandler;

    private PacketHandler? currentHandler;

    public ServerConnectionContext(S2CConnection connection) {
        Connection = connection;

        HandshakeHandler = new PacketHandler();

        GameplayHandler = new PacketHandler();

        currentHandler = HandshakeHandler;
    }

    public void Tick() {
        if (currentHandler == null || Connection.isDead)
            return;

        Connection.Poll(currentHandler);
    }

    public void Close() => Connection.Close();
}
