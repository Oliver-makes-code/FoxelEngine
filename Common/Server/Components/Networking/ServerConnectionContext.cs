using GlmSharp;
using Voxel.Common.Collision;
using Voxel.Common.Content;
using Voxel.Common.Network;
using Voxel.Common.Network.Packets;
using Voxel.Common.Network.Packets.C2S;
using Voxel.Common.Network.Packets.C2S.Gameplay;
using Voxel.Common.Network.Packets.C2S.Gameplay.Actions;
using Voxel.Common.Network.Packets.C2S.Handshake;
using Voxel.Common.Network.Packets.S2C;
using Voxel.Common.Network.Packets.S2C.Gameplay;
using Voxel.Common.Network.Packets.S2C.Handshake;
using Voxel.Common.Network.Packets.Utils;
using Voxel.Common.Util;
using Voxel.Common.World;
using Voxel.Common.World.Entity;
using Voxel.Common.World.Entity.Player;

namespace Voxel.Common.Server.Components.Networking;

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

        HandshakeHandler = new PacketHandler<C2SPacket>();
        HandshakeHandler.RegisterHandler<HandshakeDoneC2SPacket>(OnHandshakeDone);

        GameplayHandler = new PacketHandler<C2SPacket>();
        GameplayHandler.RegisterHandler<PlayerUpdatedC2SPacket>(OnPlayerUpdated);

        GameplayHandler.RegisterHandler<PlaceBlockC2SPacket>(OnPlayerPlacedBlock);
        GameplayHandler.RegisterHandler<BreakBlockC2SPacket>(OnPlayerBrokeBlock);

        Connection.packetHandler = HandshakeHandler;
    }

    private void OnHandshakeDone(HandshakeDoneC2SPacket pkt) {
        Connection.packetHandler = GameplayHandler;

        //Notify client that server has finished handshake.
        var response = PacketPool.GetPacket<HandshakeDoneS2CPacket>();
        response.PlayerID = playerID = Guid.NewGuid();
        Connection.DeliverPacket(response);

        Console.WriteLine("Server:Client Says Handshake Done");
        GameplayStart();
    }

    private void OnPlayerUpdated(PlayerUpdatedC2SPacket pkt) {
        if (entity == null)
            return;

        entity.position = pkt.Position;
        entity.rotation = pkt.Rotation;

        loadedChunks?.Move(entity.chunkPosition);

        //Console.WriteLine(entity.position + "|" + entity.chunkPosition);
    }

    private void OnPlayerPlacedBlock(PlaceBlockC2SPacket pkt) {
        if (entity == null)
            return;

        if (!ContentDatabase.Instance.Registries.Blocks.RawToEntry(pkt.BlockRawID, out var block))
            return;

        var pos = pkt.Position + entity.eyeOffset;
        var rot = quat.Identity
            .Rotated((float)pkt.Rotation.y, new(0, 1, 0))
            .Rotated((float)pkt.Rotation.x, new(1, 0, 0));
        var projected = rot * new vec3(0, 0, -5);

        if (entity.world.Raycast(new RaySegment(new Ray(pos, projected), 5), out var hit, out var worldPos))
            entity.world.SetBlock(worldPos + hit.normal.WorldToBlockPosition(), block);
    }

    private void OnPlayerBrokeBlock(BreakBlockC2SPacket pkt) {
        if (entity == null)
            return;

        if (!ContentDatabase.Instance.Registries.Blocks.RawToEntry(pkt.BlockRawID, out var block))
            return;

        var pos = pkt.Position + entity.eyeOffset;
        var rot = quat.Identity
            .Rotated((float)pkt.Rotation.y, new(0, 1, 0))
            .Rotated((float)pkt.Rotation.x, new(1, 0, 0));
        var projected = rot * new vec3(0, 0, -5);

        if (entity.world.Raycast(new RaySegment(new Ray(pos, projected), 5), out var hit, out var worldPos))
            entity.world.SetBlock(worldPos, block);
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
        loadedChunks = new LoadedChunkSection(world, position, range, range);

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
}
