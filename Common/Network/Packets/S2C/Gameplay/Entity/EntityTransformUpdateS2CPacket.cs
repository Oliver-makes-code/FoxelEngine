using GlmSharp;
using Foxel.Common.Util.Serialization;
using Greenhouse.Libs.Serialization;
using Foxel.Common.Util;
using Foxel.Common.Network.Packets.Utils;

namespace Foxel.Common.Network.Packets.S2C.Gameplay.Entity;

public class EntityTransformUpdateS2CPacket : EntityPacketS2CPacket {
    public static readonly Codec<EntityTransformUpdateS2CPacket> Codec = RecordCodec<EntityTransformUpdateS2CPacket>.Create(
        Codecs.Guid.Field<EntityTransformUpdateS2CPacket>("id", it => it.id),
        FoxelCodecs.DVec3.Field<EntityTransformUpdateS2CPacket>("position", it => it.position),
        FoxelCodecs.DVec2.Field<EntityTransformUpdateS2CPacket>("rotation", it => it.rotation),
        (id, postiion, rotation) => {
            var pkt = PacketPool.GetPacket<EntityTransformUpdateS2CPacket>();
            pkt.id = id;
            pkt.position = postiion;
            pkt.rotation = rotation;
            return pkt;
        }
    );
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<EntityTransformUpdateS2CPacket>(Codec);

    public dvec3 position;
    public dvec2 rotation;

    public override void Init(World.Entity.Entity entity) {
        position = entity.position;
        rotation = entity.rotation;
    }

    public override void Apply(World.Entity.Entity entity) {
        entity.position = position;
        entity.rotation = rotation;
    }

    public override Codec<Packet> GetCodec()
        => ProxyCodec;
}
