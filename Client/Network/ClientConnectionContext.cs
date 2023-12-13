using System;
using Voxel.Client.Rendering.Models;
using Voxel.Client.World;
using Voxel.Client.World.Entity;
using Voxel.Common.Network.Packets;
using Voxel.Common.Network.Packets.C2S;
using Voxel.Common.Network.Packets.S2C;
using Voxel.Common.Network.Packets.S2C.Gameplay;
using Voxel.Common.Network.Packets.S2C.Gameplay.Entity;
using Voxel.Common.Network.Packets.S2C.Handshake;
using Voxel.Common.Network.Packets.Utils;

namespace Voxel.Client.Network;

/// <summary>
/// This controls a connection from the client to a server.
///
/// Holds stuff like connection status and whatnot.
/// </summary>
public class ClientConnectionContext {
    public bool isDead => Connection.isDead;
    public Guid playerID { get; private set; }

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

        GameplayHandler.RegisterHandler<SpawnEntity>(HandleSpawnEntity);

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
        BlockModelManager.BakeRawBlockModels();
        Connection.packetHandler = GameplayHandler;
        playerID = packet.PlayerID;
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

    private void HandleSpawnEntity(SpawnEntity packet) {
        if (Client.world == null)
            return;

        if (packet.ID == playerID) {
            var entity = new ControlledClientPlayerEntity();
            entity.ID = packet.ID;
            Client.PlayerEntity = entity;
            Client.world.AddEntity(entity, packet.position, packet.rotation);
        }
    }

    public void SendPacket(C2SPacket packet) {
        Connection.DeliverPacket(packet);
        PacketPool.Return(packet);
    }

    public void Close() => Connection.Close();
}
