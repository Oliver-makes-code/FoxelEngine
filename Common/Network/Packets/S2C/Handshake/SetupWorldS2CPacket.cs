using Foxel.Common.Network.Packets.Utils;
using Foxel.Common.Util.Serialization;
using Foxel.Common.World.Content.Packets;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.Network.Packets.S2C.Handshake;

public class SetupWorldS2CPacket : S2CPacket {
    public static readonly Codec<SetupWorldS2CPacket> Codec = new UnitCodec<SetupWorldS2CPacket>(PacketPool.GetPacket<SetupWorldS2CPacket>);
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<SetupWorldS2CPacket>(Codec);

    public override Codec<Packet> GetCodec()
        => ProxyCodec;
}
