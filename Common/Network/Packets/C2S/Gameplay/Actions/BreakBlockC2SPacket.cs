using Foxel.Common.Network.Packets.Utils;
using Foxel.Common.Util;
using Foxel.Common.Util.Serialization;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.Network.Packets.C2S.Gameplay.Actions;

public class BreakBlockC2SPacket : PlayerActionC2SPacket {
    public static readonly Codec<BreakBlockC2SPacket> Codec = RecordCodec<BreakBlockC2SPacket>.Create(
        FoxelCodecs.DVec3.Field<BreakBlockC2SPacket>("position", it => it.position),
        FoxelCodecs.DVec2.Field<BreakBlockC2SPacket>("rotation", it => it.rotation),
        Codecs.Int.Field<BreakBlockC2SPacket>("blockId", it => it.blockId),
        (position, rotation, id) => {
            var pkt = PacketPool.GetPacket<BreakBlockC2SPacket>();
            pkt.position = position;
            pkt.rotation = rotation;
            pkt.blockId = id;
            return pkt;
        }
    );
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<BreakBlockC2SPacket>(Codec);

    public int blockId;

    public override Codec<Packet> GetCodec()
        => ProxyCodec;
}
