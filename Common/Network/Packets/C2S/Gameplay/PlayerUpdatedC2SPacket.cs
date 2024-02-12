using GlmSharp;
using Voxel.Common.Util.Serialization;

namespace Voxel.Common.Network.Packets.C2S.Gameplay;

public class PlayerUpdatedC2SPacket : C2SPacket {
    public dvec3 Position;
    public dvec2 Rotation;

    public override void Write(VDataWriter writer) {
        writer.Write(Position);
        writer.Write(Rotation);
    }
    public override void Read(VDataReader reader) {
        Position = reader.ReadDVec3();
        Rotation = reader.ReadDVec2();
    }
}
