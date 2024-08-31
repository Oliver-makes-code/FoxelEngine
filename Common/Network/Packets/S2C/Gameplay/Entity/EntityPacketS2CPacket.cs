using Foxel.Common.Util.Serialization;

namespace Foxel.Common.Network.Packets.S2C.Gameplay.Entity;

public class EntityPacketS2CPacket : S2CPacket {
    public Guid ID;

    public virtual void Init(World.Entity.Entity entity) {
        ID = entity.id;
    }

    public virtual void Apply(World.Entity.Entity entity) {
        
    }

    public override void Write(VDataWriter writer) {
        writer.Write(ID);
    }
    public override void Read(VDataReader reader) {
        ID = reader.ReadGuid();
    }
}
