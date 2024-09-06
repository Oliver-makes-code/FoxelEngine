using GlmSharp;
using Foxel.Common.World;
using Foxel.Common.World.Storage;
using Greenhouse.Libs.Serialization;
using Foxel.Common.Util;
using Foxel.Common.Network.Packets.Utils;

namespace Foxel.Common.Network.Packets.S2C.Gameplay;

public class ChunkDataS2CPacket : S2CPacket {
    public static readonly Codec<ChunkDataS2CPacket> Codec = RecordCodec<ChunkDataS2CPacket>.Create(
        FoxelCodecs.IVec3.Field<ChunkDataS2CPacket>("position", it => it.position),
        ChunkStorage.Codec.Field<ChunkDataS2CPacket>("storage", it => it.storage),
        (position, storage) => {
            var pkt = PacketPool.GetPacket<ChunkDataS2CPacket>();
            pkt.position = position;
            pkt.storage = storage;
            return pkt;
        }
    );
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<ChunkDataS2CPacket>(Codec);

    public ivec3 position { get; private set; }
    private ChunkStorage storage;

    public void Init(Chunk chunk) {
        position = chunk.ChunkPosition;
        storage = chunk.storage.GenerateCopy();
    }

    public void Apply(Chunk chunk) {
        chunk.SetStorage(storage.WithChunk(chunk));
    }

    public override Codec<Packet> GetCodec() 
        => ProxyCodec;

    private enum Type : byte {
        Single,
        Simple,
    }
}
