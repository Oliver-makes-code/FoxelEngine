using GlmSharp;
using Voxel.Common.Util.Serialization;

namespace Voxel.Common.Network.Packets.S2C.Gameplay.Entity;

public class EntityTransformUpdate : EntityPacket {
    public dvec3 Position;
    public dvec2 Rotation;

    public override void Init(World.Entity.Entity entity) {
        Position = entity.position;
        Rotation = entity.rotation;
    }

    public override void Apply(World.Entity.Entity entity) {
        entity.position = Position;
        entity.rotation = Rotation;
    }

    public override void Write(VDataWriter writer) {
        writer.Write(Position);
        writer.Write(Rotation);
    }
    public override void Read(VDataReader reader) {
        Position = reader.ReadDVec3();
        Rotation = reader.ReadDVec2();
    }
}
