using Voxel.Common.World.Tick;

namespace Voxel.Common.World.Entity;

public abstract class TickedEntity : Entity, Tickable {

    public virtual void Tick() {
        lastPosition = position;
        lastRotation = rotation;
    }
}
