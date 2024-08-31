using System;
using Foxel.Client.Rendering.Models;
using Foxel.Client.World;
using Foxel.Client.World.Entity;
using Foxel.Common.Content;
using Foxel.Common.Network.Packets;
using Foxel.Common.Network.Packets.C2S;
using Foxel.Common.Network.Packets.S2C;
using Foxel.Common.Network.Packets.S2C.Gameplay;
using Foxel.Common.Network.Packets.S2C.Gameplay.Entity;
using Foxel.Common.Network.Packets.S2C.Gameplay.Tile;
using Foxel.Common.Network.Packets.S2C.Handshake;
using Foxel.Common.Network.Packets.Utils;
using Foxel.Core;

namespace Foxel.Client.Network;

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

        HandshakeHandler = new();
        HandshakeHandler.RegisterHandler<HandshakeDoneS2CPacket>(HandleHandshakeDone);

        GameplayHandler = new();
        GameplayHandler.RegisterHandler<SetupWorldS2CPacket>(HandleSetupWorld);
        GameplayHandler.RegisterHandler<ChunkDataS2CPacket>(HandleChunkData);
        GameplayHandler.RegisterHandler<ChunkUnloadS2CPacket>(HandleChunkUnload);

        GameplayHandler.RegisterHandler<SpawnEntityS2CPacket>(HandleSpawnEntity);
        GameplayHandler.RegisterHandler<BlockChangedS2CPacket>(HandleBlockChanged);

        Connection.packetHandler = HandshakeHandler;
    }

    public void Tick() {
        if (Connection.isDead)
            return;

        Connection.Tick();
    }

    public void SendPacket(C2SPacket packet) {
        Connection.DeliverPacket(packet);
        PacketPool.Return(packet);
    }

    public void Close()
        => Connection.Close();

    private void HandleSetupWorld(SetupWorldS2CPacket packet)
        => Client.SetupWorld();

    private void HandleHandshakeDone(HandshakeDoneS2CPacket packet) {
        Connection.packetHandler = GameplayHandler;
        playerID = packet.PlayerID;
        Game.Logger.Info("Server Says Handshake Done");
    }

    private void HandleChunkData(ChunkDataS2CPacket packet) {
        if (Client.world == null)
            return;

        var chunk = Client.world.GetOrCreateChunk(packet.position);
        packet.Apply(chunk);
    }

    private void HandleChunkUnload(ChunkUnloadS2CPacket packet) {
        if (Client.world == null)
            return;

        if (Client.world.TryGetChunkRaw(packet.position, out var chunk))
            packet.Apply(chunk);
    }

    private void HandleSpawnEntity(SpawnEntityS2CPacket packet) {
        if (Client.world == null)
            return;

        if (packet.ID == playerID) {
            var entity = new ControlledClientPlayerEntity();
            entity.id = packet.ID;
            Client.playerEntity = entity;
            Client.world.AddEntity(entity, packet.position, packet.rotation);
        }
    }

    private void HandleBlockChanged(BlockChangedS2CPacket packet) {
        if (Client.world == null)
            return;

        if (!Client.world.TryGetChunk(packet.worldPos, out var chunk))
            return;

        foreach (var update in packet.updates) {
            if (!ContentDatabase.Instance.Registries.Blocks.RawToEntry(update.blockId, out var block))
                throw new Exception($"Block with ID {update.blockId} not found");

            chunk.SetBlock(update.position, block);
        }
    }
}
