using Foxel.Common.Network.Packets.Utils;
using Foxel.Common.Util;
using Foxel.Common.Util.Serialization;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.Network.Packets.C2S.Gameplay.Actions;

public class PlaceBlockC2SPacket : PlayerActionC2SPacket {
    public static readonly Codec<PlaceBlockC2SPacket> Codec = RecordCodec<PlaceBlockC2SPacket>.Create(
        FoxelCodecs.DVec3.Field<PlaceBlockC2SPacket>("position", it => it.position),
        FoxelCodecs.DVec2.Field<PlaceBlockC2SPacket>("rotation", it => it.rotation),
        Codecs.UInt.Field<PlaceBlockC2SPacket>("blockId", it => it.blockId),
        (position, rotation, id) => {
            var pkt = PacketPool.GetPacket<PlaceBlockC2SPacket>();
            pkt.position = position;
            pkt.rotation = rotation;
            pkt.blockId = id;
            return pkt;
        }
    );
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<PlaceBlockC2SPacket>(Codec);

    public uint blockId;

    public override Codec<Packet> GetCodec()
        => ProxyCodec;
}
