using GlmSharp;
using Voxel.Common.Util;

namespace Voxel.Common.World.Entity;

public abstract class LivingEntity : TickedEntity {
    private const int CoyoteTicks = 4;
    
    public double airTime;
    public bool jumped;
    
    public override void Tick() {
        base.Tick();

        var preMoveVelocity = velocity;
        velocity = MoveAndSlide(velocity);
        position += velocity * Constants.SecondsPerTick;

        //If we're moving down && the new velocity after moving is greater than the velocity before we moved, then we hit a floor.
        isOnFloor = preMoveVelocity.y < 0 && velocity.y > preMoveVelocity.y;

        if (!isOnFloor) {
            airTime += Constants.SecondsPerTick;
            double verticalVelocity = velocity.y;
            verticalVelocity = Math.Max(-32, verticalVelocity - Constants.GravityPerTick);
            velocity = velocity.WithY(verticalVelocity);
        } else {
            airTime = 0;
            jumped = false;
            velocity -= dvec3.UnitY * 0.1f;
        }
    }

    public void Jump(float height = 1) {
        if (airTime > CoyoteTicks * Constants.SecondsPerTick || jumped)
            return;

        jumped = true;
        velocity = velocity.WithY(Math.Sqrt(2 * Constants.Gravity * height));
    }
}