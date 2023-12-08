using Common.Network.Packets.Utils;
using GlmSharp;
using SharpGen.Runtime;
using Voxel.Client.Keybinding;
using Voxel.Common.Network.Packets.C2S.Gameplay;

namespace Voxel.Client.World.Entity;

public class ControlledClientPlayerEntity : ClientPlayerEntity {

    public override void Tick() {
        var movement = Keybinds.Move.axis;
        var looking = Keybinds.Look.axis;

        movement += new dvec2(0, -1) * Keybinds.Forward.strength;
        movement += new dvec2(0, 1) * Keybinds.Backward.strength;
        movement += new dvec2(-1, 0) * Keybinds.StrafeLeft.strength;
        movement += new dvec2(1, 0) * Keybinds.StrafeRight.strength;

        if (movement.LengthSqr > 1)
            movement = movement.Normalized;

        var movement3d = new dvec3(movement.x, 0, movement.y);

        movement3d += new dvec3(0, 1, 0) * Keybinds.Jump.strength;
        movement3d += new dvec3(0, -1, 0) * Keybinds.Crouch.strength;

        position += movement3d;
        rotation += (float)looking.x;

        var transformUpdate = PacketPool.GetPacket<PlayerUpdated>();
        transformUpdate.Position = position;
        transformUpdate.Rotation = rotation;
        VoxelClient.Instance.connection!.SendPacket(transformUpdate);
    }
}
