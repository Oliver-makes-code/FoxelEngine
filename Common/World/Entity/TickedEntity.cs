using Foxel.Common.World.Tick;

namespace Foxel.Common.World.Entity;

public abstract class TickedEntity : Entity, Tickable {

    public virtual void Tick() {
        lastPosition = position;
        lastRotation = rotation;
    }
}
