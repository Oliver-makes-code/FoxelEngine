using GlmSharp;
using Foxel.Common.Util.Serialization;
using Greenhouse.Libs.Serialization;
using Foxel.Common.Network.Packets.S2C.Gameplay.Entity;
using Foxel.Common.Util;
using Foxel.Common.Network.Packets.Utils;

namespace Foxel.Common.Network.Packets.C2S.Gameplay;

public class PlayerUpdatedC2SPacket : C2SPacket {
    public static readonly Codec<PlayerUpdatedC2SPacket> Codec = RecordCodec<PlayerUpdatedC2SPacket>.Create(
        FoxelCodecs.DVec3.Field<PlayerUpdatedC2SPacket>("position", it => it.position),
        FoxelCodecs.DVec2.Field<PlayerUpdatedC2SPacket>("rotation", it => it.rotation),
        (postiion, rotation) => {
            var pkt = PacketPool.GetPacket<PlayerUpdatedC2SPacket>();
            pkt.position = postiion;
            pkt.rotation = rotation;
            return pkt;
        }
    );
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<PlayerUpdatedC2SPacket>(Codec);

    public dvec3 position;
    public dvec2 rotation;

    public override Codec<Packet> GetCodec()
        => ProxyCodec;
}
