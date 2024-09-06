using Foxel.Common.Network.Packets.Utils;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.Network.Packets.S2C.Handshake;

public class HandshakeDoneS2CPacket : S2CPacket {
    public static readonly Codec<HandshakeDoneS2CPacket> Codec = RecordCodec<HandshakeDoneS2CPacket>.Create(
        Codecs.Guid.Field<HandshakeDoneS2CPacket>("player_id", it => it.playerId),
        (playerId) => {
            var pkt = PacketPool.GetPacket<HandshakeDoneS2CPacket>();
            pkt.playerId = playerId;
            return pkt;
        }
    );
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<HandshakeDoneS2CPacket>(Codec);

    public Guid playerId;

    public override Codec<Packet> GetCodec()
        => ProxyCodec;
}
