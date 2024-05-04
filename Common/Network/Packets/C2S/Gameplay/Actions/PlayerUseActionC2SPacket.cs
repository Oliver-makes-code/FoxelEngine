using Voxel.Common.Util.Serialization;

namespace Voxel.Common.Network.Packets.C2S.Gameplay.Actions;

public class PlayerUseActionC2SPacket : PlayerActionC2SPacket {
    public int slot;

    public override void Write(VDataWriter writer) {
        base.Write(writer);
        writer.Write(slot);
    }

    public override void Read(VDataReader reader) {
        base.Read(reader);
        slot = reader.ReadInt();
    }
}
