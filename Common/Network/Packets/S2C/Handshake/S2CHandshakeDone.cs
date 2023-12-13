using Voxel.Common.Util.Serialization;

namespace Voxel.Common.Network.Packets.S2C.Handshake;

public class S2CHandshakeDone : S2CPacket {
    public Guid PlayerID;

    public override void Write(VDataWriter writer) {
        writer.Write(PlayerID);
    }
    public override void Read(VDataReader reader) {
        PlayerID = reader.ReadGuid();
    }
}
