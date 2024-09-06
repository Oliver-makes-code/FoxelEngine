using Foxel.Common.Network.Packets.Utils;
using Foxel.Common.Util;
using Foxel.Common.World.Content.Blocks.State;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.Network.Packets.C2S.Gameplay.Actions;

public class BreakBlockC2SPacket : PlayerActionC2SPacket {
    public static readonly Codec<BreakBlockC2SPacket> Codec = RecordCodec<BreakBlockC2SPacket>.Create(
        FoxelCodecs.DVec3.Field<BreakBlockC2SPacket>("position", it => it.position),
        FoxelCodecs.DVec2.Field<BreakBlockC2SPacket>("rotation", it => it.rotation),
        BlockState.NetCodec.Field<BreakBlockC2SPacket>("state", it => it.state),
        (position, rotation, state) => {
            var pkt = PacketPool.GetPacket<BreakBlockC2SPacket>();
            pkt.position = position;
            pkt.rotation = rotation;
            pkt.state = state;
            return pkt;
        }
    );
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<BreakBlockC2SPacket>(Codec);

    public BlockState state;

    public override Codec<Packet> GetCodec()
        => ProxyCodec;
}
