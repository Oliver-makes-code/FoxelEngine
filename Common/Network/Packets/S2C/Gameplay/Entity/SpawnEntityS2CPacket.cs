using GlmSharp;
using Voxel.Common.Util.Serialization;

namespace Voxel.Common.Network.Packets.S2C.Gameplay.Entity;

public class SpawnEntityS2CPacket : EntityPacketS2CPacket {

    public dvec3 position;
    public dvec2 rotation;

    public override void Init(World.Entity.Entity entity) {
        base.Init(entity);

        position = entity.position;
        rotation = entity.rotation;
    }

    public override void Write(VDataWriter writer) {
        base.Write(writer);

        writer.Write(position);
        writer.Write(rotation);
    }

    public override void Read(VDataReader reader) {
        base.Read(reader);

        position = reader.ReadDVec3();
        rotation = reader.ReadDVec2();
    }
}
