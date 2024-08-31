using GlmSharp;
using Foxel.Common.Util.Serialization;

namespace Foxel.Common.Network.Packets.S2C.Gameplay.Tile;

public class BlockChangedS2CPacket : S2CPacket {
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
