using Greenhouse.Libs.Serialization;

namespace Foxel.Common.Network.Packets;

public abstract class Packet {
    public Packet() {}

    public abstract Codec<Packet> GetCodec();

    public virtual void OnReturnToPool() {}
}

public record PacketProxyCodec<TPacket>(Codec<TPacket> Codec) : Codec<Packet> where TPacket : Packet {
    public override Packet ReadGeneric(DataReader reader)
        => Codec.ReadGeneric(reader);
    public override void WriteGeneric(DataWriter writer, Packet value)
        => Codec.WriteGeneric(writer, (TPacket) value);
}
