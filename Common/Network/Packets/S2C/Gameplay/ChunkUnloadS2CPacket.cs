using GlmSharp;
using Foxel.Common.World;
using Foxel.Core;
using Greenhouse.Libs.Serialization;
using Foxel.Common.Util;
using Foxel.Common.Network.Packets.Utils;

namespace Foxel.Common.Network.Packets.S2C.Gameplay;

public class ChunkUnloadS2CPacket : S2CPacket {
    public static readonly Codec<ChunkUnloadS2CPacket> Codec = RecordCodec<ChunkUnloadS2CPacket>.Create(
        FoxelCodecs.IVec3.Field<ChunkUnloadS2CPacket>("position", it => it.position),
        (position) => {
            var pkt = PacketPool.GetPacket<ChunkUnloadS2CPacket>();
            pkt.position = position;
            return pkt;
        }
    );
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<ChunkUnloadS2CPacket>(Codec);

    public ivec3 position { get; private set; }

    public void Init(Chunk target) {
        position = target.ChunkPosition;
    }

    public void Apply(Chunk target) {
        target.World.UnloadChunk(target.ChunkPosition);

        Game.Logger.Info($"Unload Chunk {target.ChunkPosition}");
    }

    public override Codec<Packet> GetCodec() 
        => ProxyCodec;
}
