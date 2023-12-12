using GlmSharp;
using Voxel.Common.Util;

namespace Voxel.Common.World.Entity;

public abstract class LivingEntity : Entity {

    public override void Tick() {
        base.Tick();
        
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
