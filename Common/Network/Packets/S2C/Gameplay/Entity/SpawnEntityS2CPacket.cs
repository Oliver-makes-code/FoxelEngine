using GlmSharp;
using Foxel.Common.Util.Serialization;
using Greenhouse.Libs.Serialization;
using Foxel.Common.Util;
using Foxel.Common.Network.Packets.Utils;

namespace Foxel.Common.Network.Packets.S2C.Gameplay.Entity;

public class SpawnEntityS2CPacket : EntityPacketS2CPacket {
    public static readonly Codec<SpawnEntityS2CPacket> Codec = RecordCodec<SpawnEntityS2CPacket>.Create(
        Codecs.Guid.Field<SpawnEntityS2CPacket>("id", it => it.id),
        FoxelCodecs.DVec3.Field<SpawnEntityS2CPacket>("position", it => it.position),
        FoxelCodecs.DVec2.Field<SpawnEntityS2CPacket>("rotation", it => it.rotation),
        (id, postiion, rotation) => {
            var pkt = PacketPool.GetPacket<SpawnEntityS2CPacket>();
            pkt.id = id;
            pkt.position = postiion;
            pkt.rotation = rotation;
            return pkt;
        }
    );
    public static readonly Codec<Packet> ProxyCodec = new PacketProxyCodec<SpawnEntityS2CPacket>(Codec);

    public dvec3 position;
    public dvec2 rotation;

    public override void Init(World.Content.Entities.Entity entity) {
        base.Init(entity);

        position = entity.position;
        rotation = entity.rotation;
    }

    public override Codec<Packet> GetCodec()
        => ProxyCodec;
}
