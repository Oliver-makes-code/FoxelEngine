using System;
using GlmSharp;
using Voxel.Client.Keybinding;
using Voxel.Common.Content;
using Voxel.Common.Network.Packets.C2S.Gameplay;
using Voxel.Common.Network.Packets.C2S.Gameplay.Actions;
using Voxel.Common.Network.Packets.Utils;
using Voxel.Common.Util;
using Voxel.Core.Util.Profiling;
using Voxel.Core.Util;

namespace Voxel.Client.World.Entity;

public class ControlledClientPlayerEntity : ClientPlayerEntity {
    private static readonly Profiler.ProfilerKey PlayerKey = Profiler.GetProfilerKey("Update Player Input");

    private const float CameraSpeedMultiplier = 3;

    private vec2 cameraPanTimers; // x is horizontal, y is vertical
    private SinusoidEase cameraPanEase = new(new(0.1f, 0.6f), new(1f, CameraSpeedMultiplier));

    public ControlledClientPlayerEntity() {

    }

    public void Update(double delta) {
        using (PlayerKey.Push()) {
            var movement = Keybinds.Move.axis;
            var looking = -Keybinds.Look.axis * 2;

            movement += new dvec2(0, -1) * Keybinds.Forward.strength;
            movement += new dvec2(0, 1) * Keybinds.Backward.strength;
            movement += new dvec2(-1, 0) * Keybinds.StrafeLeft.strength;
            movement += new dvec2(1, 0) * Keybinds.StrafeRight.strength;

            if (Keybinds.LookLeft.isPressed || Keybinds.LookRight.isPressed)
                cameraPanTimers.x += (float)delta * MathF.Abs((float)(Keybinds.LookLeft.strength - Keybinds.LookRight.strength));
            else
                cameraPanTimers.x = 0;
            if (Keybinds.LookUp.isPressed || Keybinds.LookDown.isPressed)
                cameraPanTimers.y += (float)delta * MathF.Abs((float)(Keybinds.LookUp.strength - Keybinds.LookDown.strength));
            else
                cameraPanTimers.y = 0;

            looking += new dvec2(Keybinds.LookLeft.strength - Keybinds.LookRight.strength, Keybinds.LookUp.strength - Keybinds.LookDown.strength) * new dvec2(cameraPanEase.F(cameraPanTimers.x), cameraPanEase.F(cameraPanTimers.y));

            if (movement.LengthSqr > 1)
                movement = movement.Normalized;

            var movement3d = new dvec3(movement.x, 0, movement.y);

            // movement3d += new dvec3(0, 1, 0) * Keybinds.Jump.strength;
            // movement3d += new dvec3(0, -1, 0) * Keybinds.Crouch.strength;

            rotation += new dvec2((float)(looking.y * delta) * 1, (float)(looking.x * delta) * 1);
            if (VoxelClient.isMouseCapruted)
                rotation += VoxelClient.instance!.InputManager.MouseDelta.swizzle.yx * -1 / 192;

            if (rotation.x < -MathF.PI/2)
                rotation.x = -MathF.PI/2;
            if (rotation.x > MathF.PI/2)
                rotation.x = MathF.PI/2;

            movement3d = new dvec2(0, rotation.y).RotationVecToQuat() * movement3d * 4;
            var localVel = dvec2.Lerp(velocity.xz, movement3d.xz, 25 * delta);
            velocity = velocity.WithXZ(localVel);

            if (Keybinds.Jump.isPressed)
                Jump();

            if (Keybinds.Crouch.justPressed)
                position -= new dvec3(0, 1, 0);

            var transformUpdate = PacketPool.GetPacket<PlayerUpdatedC2SPacket>();
            transformUpdate.Position = position;
            transformUpdate.Rotation = rotation;
            VoxelClient.instance?.connection?.SendPacket(transformUpdate);


            if (Keybinds.Attack.justPressed)
                BreakBlock();

            if (Keybinds.Use.justPressed)
                PlaceBlock();
        }
    }

    private void BreakBlock() {
        if (!ContentDatabase.Instance.Registries.Blocks.IdToRaw(new("air"), out var raw))
            return;

        var pkt = PacketPool.GetPacket<BreakBlockC2SPacket>();
        pkt.Init(this);
        pkt.BlockRawID = raw;

        VoxelClient.instance?.connection?.SendPacket(pkt);
    }

    private void PlaceBlock() {
        if (!ContentDatabase.Instance.Registries.Blocks.IdToRaw(new("stone"), out var raw))
            return;

        var pkt = PacketPool.GetPacket<PlaceBlockC2SPacket>();
        pkt.Init(this);
        pkt.BlockRawID = raw;

        VoxelClient.instance?.connection?.SendPacket(pkt);
    }

    // TODO: Define this elsewhere later probably
    private struct SinusoidEase {
        public vec2 domain;
        public vec2 range;

        public SinusoidEase(vec2 domain, vec2 range) {
            this.domain = domain;
            this.range = range;
        }

        public float F(float t) {
            if (t < domain.x) return range.x;
            if (t > domain.y) return range.y;

            float ease = -0.5f * MathF.Cos(MathF.PI * t / (domain.y - domain.x)) + 0.5f;
            return ease * (range.y - range.x) + range.x;
        }
    }
}
