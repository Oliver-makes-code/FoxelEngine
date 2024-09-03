using Foxel.Common.World.Tick;

namespace Foxel.Common.World.Content.Entities;

public abstract class TickedEntity : Entity, Tickable {

    public virtual void Tick() {
        lastPosition = position;
        lastRotation = rotation;
    }
}
