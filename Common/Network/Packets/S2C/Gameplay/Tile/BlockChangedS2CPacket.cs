using GlmSharp;
using Greenhouse.Libs.Serialization;
using Foxel.Common.Util;
using Foxel.Common.Network.Packets.Utils;
using Foxel.Common.World.Content.Blocks.State;

namespace Foxel.Common.Network.Packets.S2C.Gameplay.Tile;

public class BlockChangedS2CPacket : S2CPacket {
    public static readonly Codec<BlockChangedS2CPacket> Codec = RecordCodec<BlockChangedS2CPacket>.Create(
        FoxelCodecs.DVec3.Field<BlockChangedS2CPacket>("world_position", it => it.worldPos),
        Single.Codec.Array().Field<BlockChangedS2CPacket>("updates", it => it.updates),
        (worldPos, updates) => {
            var pkt = PacketPool.GetPacket<BlockChangedS2CPacket>();
            pkt.worldPos = worldPos;
            pkt.updates = updates;
            return pkt;
        }
    );
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<BlockChangedS2CPacket>(Codec);

    public struct Single {
        public static readonly Codec<Single> Codec = RecordCodec<Single>.Create(
            FoxelCodecs.IVec3.Field<Single>("position", it => it.position),
            BlockState.NetCodec.Field<Single>("block_id", it => it.state),
            (pos, state) => new() {
                position = pos,
                state = state
            }
        );

        public ivec3 position;
        public BlockState state;
    }

    public dvec3 worldPos;
    public Single[] updates = [];

    public override Codec<Packet> GetCodec()
        => ProxyCodec;
}
