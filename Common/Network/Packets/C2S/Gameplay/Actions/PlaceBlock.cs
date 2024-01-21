using Voxel.Common.Util.Serialization;

namespace Voxel.Common.Network.Packets.C2S.Gameplay.Actions;

public class PlaceBlock : PlayerActionPacket {

    public uint BlockRawID;

    public override void Write(VDataWriter writer) {
        base.Write(writer);
        writer.Write(BlockRawID);
    }

    public override void Read(VDataReader reader) {
        base.Read(reader);
        BlockRawID = reader.ReadUInt();
    }

}
