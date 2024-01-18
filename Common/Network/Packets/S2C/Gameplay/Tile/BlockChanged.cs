using GlmSharp;
using Voxel.Common.Util.Serialization;

namespace Voxel.Common.Network.Packets.S2C.Gameplay.Tile;

public class BlockChanged : S2CPacket {
    public ivec3 Position;
    public uint BlockID;

    public override void Write(VDataWriter writer) {
        writer.Write(Position);
        writer.Write(BlockID);
    }
    public override void Read(VDataReader reader) {
        Position = reader.ReadIVec3();
        BlockID = reader.ReadUInt();
    }
}
