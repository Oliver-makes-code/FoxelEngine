using Voxel.Common.Network;
using Voxel.Common.Network.Packets;
using Voxel.Common.Network.Packets.C2S;
using Voxel.Common.Network.Packets.C2S.Gameplay;
using Voxel.Common.Network.Packets.C2S.Handshake;
using Voxel.Common.Network.Packets.S2C;
using Voxel.Common.Network.Packets.S2C.Gameplay;
using Voxel.Common.Network.Packets.S2C.Handshake;
using Voxel.Common.Network.Packets.Utils;
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

    public readonly S2CConnection Connection;

    private readonly PacketHandler<C2SPacket> HandshakeHandler;
    private readonly PacketHandler<C2SPacket> GameplayHandler;

    public PlayerEntity? entity { get; private set; }
    public LoadedChunkSection? loadedChunks { get; private set; }

    public event Action GameplayStart = () => {};

    public ServerConnectionContext(S2CConnection connection) {
        Connection = connection;

        HandshakeHandler = new PacketHandler<C2SPacket>();
        HandshakeHandler.RegisterHandler<C2SHandshakeDone>(OnHandshakeDone);

        GameplayHandler = new PacketHandler<C2SPacket>();
        GameplayHandler.RegisterHandler<PlayerUpdated>(OnPlayerUpdated);

        Connection.packetHandler = HandshakeHandler;
    }

    private void OnHandshakeDone(C2SHandshakeDone pkt) {
        Connection.packetHandler = GameplayHandler;

        //Notify client that server has finished handshake.
        Connection.DeliverPacket(new S2CHandshakeDone());

        Console.WriteLine("Server:Client Says Handshake Done");
        GameplayStart();
    }

    private void OnPlayerUpdated(PlayerUpdated pkt) {
        if (entity == null)
            return;

        entity.position = pkt.Position;
        entity.rotation = pkt.Rotation;

        loadedChunks?.Move(entity.chunkPosition);

        //Console.WriteLine(entity.position + "|" + entity.chunkPosition);
    }

    public void Close()
        => Connection.Close();


    public void SendPacket(S2CPacket packet) {
        Connection.DeliverPacket(packet);
        PacketPool.Return(packet);
    }

    public void SetPlayerEntity(PlayerEntity e)
        => entity = e;

    public void SetupViewArea(int range) {
        if (entity == null)
            return;

        loadedChunks = new LoadedChunkSection(entity.world, entity.chunkPosition, range, range);

        foreach (var chunk in loadedChunks.Chunks()) {
            var pkt = PacketPool.GetPacket<ChunkData>();
            pkt.Init(chunk);

            SendPacket(pkt);
        }

        loadedChunks.OnChunkAddedToView += c => {
            var pkt = PacketPool.GetPacket<ChunkData>();
            pkt.Init(c);

            SendPacket(pkt);
        };

        loadedChunks.OnChunkRemovedFromView += c => {
            var pkt = PacketPool.GetPacket<ChunkUnload>();
            pkt.Init(c);
            
            SendPacket(pkt);
        };
    }
}
