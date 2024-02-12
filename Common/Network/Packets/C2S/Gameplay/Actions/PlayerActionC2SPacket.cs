using GlmSharp;
using Voxel.Common.Util.Serialization;
using Voxel.Common.World.Entity;

namespace Voxel.Common.Network.Packets.C2S.Gameplay.Actions;

public abstract class PlayerActionC2SPacket : C2SPacket {

    public dvec3 Position;
    public dvec2 Rotation;

    public virtual void Init(Entity entity) {
        Position = entity.position;
        Rotation = entity.rotation;
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
