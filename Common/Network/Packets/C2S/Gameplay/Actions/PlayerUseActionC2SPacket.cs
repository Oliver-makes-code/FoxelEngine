using Foxel.Common.Network.Packets.Utils;
using Foxel.Common.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.Network.Packets.C2S.Gameplay.Actions;

public class PlayerUseActionC2SPacket : PlayerActionC2SPacket {
    public static readonly Codec<PlayerUseActionC2SPacket> Codec = RecordCodec<PlayerUseActionC2SPacket>.Create(
        FoxelCodecs.DVec3.Field<PlayerUseActionC2SPacket>("position", it => it.position),
        FoxelCodecs.DVec2.Field<PlayerUseActionC2SPacket>("rotation", it => it.rotation),
        Codecs.Int.Field<PlayerUseActionC2SPacket>("slot", it => it.slot),
        (position, rotation, slot) => {
            var pkt = PacketPool.GetPacket<PlayerUseActionC2SPacket>();
            pkt.position = position;
            pkt.rotation = rotation;
            pkt.slot = slot;
            return pkt;
        }
    );
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<PlayerUseActionC2SPacket>(Codec);

    public int slot;

    public override Codec<Packet> GetCodec()
        => ProxyCodec;
}
