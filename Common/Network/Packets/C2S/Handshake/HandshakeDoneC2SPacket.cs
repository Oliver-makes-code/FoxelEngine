using Foxel.Common.Network.Packets.Utils;
using Foxel.Common.Util.Serialization;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.Network.Packets.C2S.Handshake;

public class HandshakeDoneC2SPacket : C2SPacket {
    public static readonly Codec<HandshakeDoneC2SPacket> Codec = new UnitCodec<HandshakeDoneC2SPacket>(PacketPool.GetPacket<HandshakeDoneC2SPacket>);
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<HandshakeDoneC2SPacket>(Codec);

    public override Codec<Packet> GetCodec()
        => ProxyCodec;
}
