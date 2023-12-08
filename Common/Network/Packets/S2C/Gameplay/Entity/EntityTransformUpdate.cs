using Common.Util.Serialization;
using GlmSharp;
using Voxel.Common.Network.Packets.S2C.Gameplay.Entity;
using Voxel.Common.World.Entity;

namespace Common.Network.Packets.S2C.Gameplay;

public class EntityTransformUpdate : EntityPacket {
    public dvec3 Position;
    public float Rotation;

    public override void Init(Entity entity) {
        Position = entity.position;
        Rotation = entity.rotation;
    }

    public override void Apply(Entity entity) {
        entity.position = Position;
        entity.rotation = Rotation;
    }

    public override void Write(VDataWriter writer) {
        writer.Write(Position);
        writer.Write(Rotation);
    }
    public override void Read(VDataReader reader) {
        Position = reader.ReadDVec3();
        Rotation = reader.ReadFloat();
    }
}
