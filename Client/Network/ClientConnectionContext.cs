using System;
using Common.Network.Packets;
using Common.Network.Packets.C2S;
using Common.Network.Packets.C2S.Handshake;
using Common.Network.Packets.S2C;
using Common.Network.Packets.S2C.Gameplay;
using Common.Network.Packets.S2C.Handshake;
using Common.Network.Packets.Utils;
using Voxel.Client.World;
using Voxel.Common.Network.Packets;
using Voxel.Common.Network.Packets.S2C.Gameplay;

namespace Voxel.Client.Network;

/// <summary>
/// This controls a connection from the client to a server.
///
/// Holds stuff like connection status and whatnot.
/// </summary>
public class ClientConnectionContext {
    public bool isDead => Connection.isDead;

    public readonly VoxelClient Client;
    private readonly C2SConnection Connection;

    private readonly PacketHandler<S2CPacket> HandshakeHandler;
    private readonly PacketHandler<S2CPacket> GameplayHandler;

    public ClientConnectionContext(VoxelClient client, C2SConnection connection) {
        Client = client;
        Connection = connection;

        HandshakeHandler = new PacketHandler<S2CPacket>();
        HandshakeHandler.RegisterHandler<S2CHandshakeDone>(HandleHandshakeDone);

        GameplayHandler = new PacketHandler<S2CPacket>();
        GameplayHandler.RegisterHandler<SetupWorld>(HandleSetupWorld);
        GameplayHandler.RegisterHandler<ChunkData>(HandleChunkData);
        GameplayHandler.RegisterHandler<ChunkUnload>(HandleChunkUnload);

        Connection.packetHandler = HandshakeHandler;
    }

    public void Tick() {
        if (Connection.isDead)
            return;

        Connection.Tick();
    }

    private void HandleSetupWorld(SetupWorld packet) {
        Client.SetupWorld();
    }

    private void HandleHandshakeDone(S2CHandshakeDone packet) {
        Connection.packetHandler = GameplayHandler;
        Console.WriteLine("Client:Server Says Handshake Done");
    }

    private void HandleChunkData(ChunkData packet) {
        if (Client.world == null)
            return;

        var chunk = Client.world.GetOrCreateChunk(packet.position);
        packet.Apply(chunk);
    }

    private void HandleChunkUnload(ChunkUnload packet) {
        if (Client.world == null)
            return;

        if (Client.world.TryGetChunkRaw(packet.position, out var chunk))
            packet.Apply(chunk);
    }

    public void SendPacket(C2SPacket packet) {
        Connection.DeliverPacket(packet);
        PacketPool.Return(packet);
    }

    public void Close() => Connection.Close();
}
