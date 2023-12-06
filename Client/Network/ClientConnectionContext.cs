using Common.Network.Packets;
using Common.Network.Packets.S2C;
using Common.Network.Packets.S2C.Gameplay;

namespace Voxel.Client.Network;

/// <summary>
/// This controls a connection from the client to a server.
///
/// Holds stuff like connection status and whatnot.
/// </summary>
public class ClientConnectionContext {
    public bool isDead => Connection.isDead;

    private readonly C2SConnection Connection;

    private readonly PacketHandler HandshakeHandler;
    private readonly PacketHandler GameplayHandler;

    private PacketHandler? currentHandler;

    public ClientConnectionContext(VoxelClient client, C2SConnection connection) {
        Connection = connection;

        HandshakeHandler = new PacketHandler();
        HandshakeHandler.RegisterHandler<SetupWorld>(HandleSetupWorld);
        HandshakeHandler.RegisterHandler<HandshakeDone>(HandleHandshakeDone);

        GameplayHandler = new PacketHandler();
        GameplayHandler.RegisterHandler<ChunkData>(HandleChunkData);

        currentHandler = HandshakeHandler;
    }

    public void Tick() {
        if (currentHandler == null || Connection.isDead)
            return;

        Connection.Poll(currentHandler);
    }

    private void HandleSetupWorld(SetupWorld packet) {

    }

    private void HandleHandshakeDone(HandshakeDone packet) {
        currentHandler = GameplayHandler;
        
        
    }

    private void HandleChunkData(ChunkData packet) {

    }

    public void Close() => Connection.Close();
}
