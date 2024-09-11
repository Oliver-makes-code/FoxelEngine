using GlmSharp;
using Foxel.Common.Util;

namespace Foxel.Common.World.Content.Entities;

public abstract class LivingEntity : TickedEntity {
    private const double CoyoteAirTime = 0.1;
    
    public double airTime;
    public bool jumped;
    
    public override void Tick() {
        base.Tick();

        var preMoveVelocity = velocity;
        velocity = MoveAndSlide(velocity, out var translateBy);
        position += translateBy;

        //If we're moving down && the new velocity after moving is greater than the velocity before we moved, then we hit a floor.
        isOnFloor = CalculateIsOnFloor();

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
        if (airTime > CoyoteAirTime || jumped)
            return;

        jumped = true;
        velocity = velocity.WithY(Math.Sqrt(2 * Constants.Gravity * height));
        isOnFloor = false;
    }
}
