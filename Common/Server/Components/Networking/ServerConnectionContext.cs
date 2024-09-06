using GlmSharp;
using Foxel.Common.Collision;
using Foxel.Common.Network;
using Foxel.Common.Network.Packets;
using Foxel.Common.Network.Packets.C2S;
using Foxel.Common.Network.Packets.C2S.Gameplay;
using Foxel.Common.Network.Packets.C2S.Gameplay.Actions;
using Foxel.Common.Network.Packets.C2S.Handshake;
using Foxel.Common.Network.Packets.S2C;
using Foxel.Common.Network.Packets.S2C.Gameplay;
using Foxel.Common.Network.Packets.S2C.Handshake;
using Foxel.Common.Network.Packets.Utils;
using Foxel.Common.World;
using Foxel.Common.World.Content.Entities.Player;
using Foxel.Common.World.Content;

namespace Foxel.Common.Server.Components.Networking;

/// <summary>
/// This is what controls a connection to a player from the server.
///
/// Holds stuff like connection status and whatnot.
/// </summary>
public class ServerConnectionContext {
    public bool isDead => Connection.isDead;
    public Guid playerID { get; private set; }

    public readonly S2CConnection Connection;

    private readonly PacketHandler<C2SPacket> HandshakeHandler;
    private readonly PacketHandler<C2SPacket> GameplayHandler;

    public PlayerEntity? entity { get; private set; }
    public LoadedChunkSection? loadedChunks { get; private set; }

    public event Action GameplayStart = () => {};

    public ServerConnectionContext(S2CConnection connection) {
        Connection = connection;

        HandshakeHandler = new();
        HandshakeHandler.RegisterHandler<HandshakeDoneC2SPacket>(OnHandshakeDone);

        GameplayHandler = new();
        GameplayHandler.RegisterHandler<PlayerUpdatedC2SPacket>(OnPlayerUpdated);

        GameplayHandler.RegisterHandler<PlayerUseActionC2SPacket>(OnPlayerUse);
        GameplayHandler.RegisterHandler<BreakBlockC2SPacket>(OnPlayerBrokeBlock);

        Connection.packetHandler = HandshakeHandler;
    }

    public void Close()
        => Connection.Close();

    /// <summary>
    /// Sends a packet to the client only if the given position is visible to them.
    /// </summary>
    public void SendPositionedPacket(dvec3 position, S2CPacket packet, bool returnPacket = true) {
        if (loadedChunks?.ContainsPosition(position) ?? false)
            SendPacket(packet, returnPacket);
    }

    public void SendPacket(S2CPacket packet, bool returnPacket = true) {
        Connection.DeliverPacket(packet);

        if (returnPacket)
            PacketPool.Return(packet);
    }

    public void SetPlayerEntity(PlayerEntity e) => entity = e;

    public void SetupViewArea(VoxelWorld world, ivec3 position, int range) {
        loadedChunks = new(world, position, range, range);

        foreach (var chunk in loadedChunks.Chunks()) {
            var pkt = PacketPool.GetPacket<ChunkDataS2CPacket>();
            pkt.Init(chunk);

            SendPacket(pkt);
        }

        loadedChunks.OnChunkAddedToView += c => {
            var pkt = PacketPool.GetPacket<ChunkDataS2CPacket>();
            pkt.Init(c);

            SendPacket(pkt);
        };

        loadedChunks.OnChunkRemovedFromView += c => {
            var pkt = PacketPool.GetPacket<ChunkUnloadS2CPacket>();
            pkt.Init(c);

            SendPacket(pkt);
        };
    }

    private void OnHandshakeDone(HandshakeDoneC2SPacket pkt) {
        Connection.packetHandler = GameplayHandler;

        //Notify client that server has finished handshake.
        var response = PacketPool.GetPacket<HandshakeDoneS2CPacket>();
        response.playerId = playerID = Guid.NewGuid();
        Connection.DeliverPacket(response);

        VoxelServer.Logger.Info("Client Says Handshake Done");
        GameplayStart();
    }

    private void OnPlayerUpdated(PlayerUpdatedC2SPacket pkt) {
        if (entity == null)
            return;

        entity.position = pkt.position;
        entity.rotation = pkt.rotation;

        loadedChunks?.Move(entity.chunkPosition);

        //Console.WriteLine(entity.position + "|" + entity.chunkPosition);
    }

    private void OnPlayerUse(PlayerUseActionC2SPacket pkt) {
        if (entity == null)
            return;

        var pos = pkt.position + entity.eyeOffset;
        var rot = quat.Identity
            .Rotated((float)pkt.rotation.y, new(0, 1, 0))
            .Rotated((float)pkt.rotation.x, new(1, 0, 0));
        var projected = rot * new vec3(0, 0, -5);

        if (entity.world?.Raycast(new RaySegment(new Ray(pos, projected), 5), out var hit) == true)
            entity.Inventory[pkt.slot].UseOnBlock(entity.world, hit);
    }

    private void OnPlayerBrokeBlock(BreakBlockC2SPacket pkt) {
        if (entity == null)
            return;

        var block = ContentStores.Blocks.GetValue(pkt.blockId);

        var pos = pkt.position + entity.eyeOffset;
        var rot = quat.Identity
            .Rotated((float)pkt.rotation.y, new(0, 1, 0))
            .Rotated((float)pkt.rotation.x, new(1, 0, 0));
        var projected = rot * new vec3(0, 0, -5);

        if (entity.world!.Raycast(new RaySegment(new Ray(pos, projected), 5), out var hit))
            entity.world!.SetBlockState(hit.blockPos, block.DefaultState);
    }
}
