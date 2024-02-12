using Voxel.Common.Util.Serialization;

namespace Voxel.Common.Network.Packets.S2C.Handshake;

public class HandshakeDoneS2CPacket : S2CPacket {
    public Guid PlayerID;

    public override void Write(VDataWriter writer) {
        writer.Write(PlayerID);
    }
    public override void Read(VDataReader reader) {
        PlayerID = reader.ReadGuid();
    }
}
