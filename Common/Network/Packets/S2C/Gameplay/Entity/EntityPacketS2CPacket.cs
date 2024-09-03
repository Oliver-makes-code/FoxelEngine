using Foxel.Common.Util.Serialization;

namespace Foxel.Common.Network.Packets.S2C.Gameplay.Entity;

public abstract class EntityPacketS2CPacket : S2CPacket {
    public Guid id;

    public virtual void Init(World.Entity.Entity entity) {
        id = entity.id;
    }

    public virtual void Apply(World.Entity.Entity entity) {}
}
