using GlmSharp;
using Voxel.Common.Util;

namespace Voxel.Common.World.Entity;

public abstract class LivingEntity : TickedEntity {

    public override void Tick() {
        base.Tick();

        var preMoveVelocity = velocity;
        velocity = MoveAndSlide(velocity);
        position += velocity * Constants.SecondsPerTick;

        //If we're moving down && the new velocity after moving is greater than the velocity before we moved, then we hit a floor.
        isOnFloor = preMoveVelocity.y < 0 && velocity.y > preMoveVelocity.y;

        if (!isOnFloor) {
            var verticalVelocity = velocity.y;
            verticalVelocity = Math.Max(-32, verticalVelocity - Constants.GravityPerTick);
            velocity = velocity.WithY(verticalVelocity);
        } else {
            velocity -= dvec3.UnitY * 0.1f;
        }
    }

    public void Jump(float height = 1) {
        if (!isOnFloor)
            return;

        velocity = velocity.WithY(Math.Sqrt(2 * Constants.Gravity * height));
    }
}
