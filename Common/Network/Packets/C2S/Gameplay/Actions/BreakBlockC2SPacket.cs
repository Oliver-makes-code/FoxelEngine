using Foxel.Common.Util.Serialization;

namespace Foxel.Common.Network.Packets.C2S.Gameplay.Actions;

public class BreakBlockC2SPacket : PlayerActionC2SPacket {

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
