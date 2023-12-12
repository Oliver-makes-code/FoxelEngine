using GlmSharp;
using Voxel.Client.Keybinding;
using Voxel.Common.Network.Packets.C2S.Gameplay;
using Voxel.Common.Network.Packets.Utils;
using Voxel.Common.Util;
using Voxel.Core.Util;

namespace Voxel.Client.World.Entity;

public class ControlledClientPlayerEntity : ClientPlayerEntity {

    private dvec3 vel;

    public ControlledClientPlayerEntity() {

    }

    public override void Tick() {
        base.Tick();

        var movement = Keybinds.Move.axis;
        var looking = -Keybinds.Look.axis;

        movement += new dvec2(0, -1) * Keybinds.Forward.strength;
        movement += new dvec2(0, 1) * Keybinds.Backward.strength;
        movement += new dvec2(-1, 0) * Keybinds.StrafeLeft.strength;
        movement += new dvec2(1, 0) * Keybinds.StrafeRight.strength;

        looking += new dvec2(Keybinds.LookLeft.strength - Keybinds.LookRight.strength, Keybinds.LookUp.strength - Keybinds.LookDown.strength);

        if (movement.LengthSqr > 1)
            movement = movement.Normalized;

        var movement3d = new dvec3(movement.x, 0, movement.y);

        // movement3d += new dvec3(0, 1, 0) * Keybinds.Jump.strength;
        // movement3d += new dvec3(0, -1, 0) * Keybinds.Crouch.strength;

        rotation += new dvec2((float)(looking.y * Constants.SecondsPerTick) * 1, (float)(looking.x * Constants.SecondsPerTick) * 1);

        movement3d = new dvec2(0, rotation.y).RotationVecToQuat() * movement3d * 4;
        var localVel = dvec2.Lerp(velocity.xz, movement3d.xz, 0.9);
        velocity = velocity.WithXZ(localVel);

        if (Keybinds.Jump.justPressed)
            Jump();

        if (Keybinds.Crouch.justPressed) {
            position -= new dvec3(0, 1, 0);
        }

        var transformUpdate = PacketPool.GetPacket<PlayerUpdated>();
        transformUpdate.Position = position;
        transformUpdate.Rotation = rotation;
        VoxelClient.Instance.connection!.SendPacket(transformUpdate);
    }
}
