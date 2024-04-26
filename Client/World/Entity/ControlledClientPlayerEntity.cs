using System;
using GlmSharp;
using Voxel.Common.Content;
using Voxel.Common.Network.Packets.C2S.Gameplay;
using Voxel.Common.Network.Packets.C2S.Gameplay.Actions;
using Voxel.Common.Network.Packets.Utils;
using Voxel.Common.Util;
using Voxel.Core.Util.Profiling;
using Voxel.Core.Util;
using Voxel.Client.Input;

namespace Voxel.Client.World.Entity;

public class ControlledClientPlayerEntity : ClientPlayerEntity {
    private static readonly Profiler.ProfilerKey PlayerKey = Profiler.GetProfilerKey("Update Player Input");

    private const float CameraSpeedMultiplier = 3;

    private vec2 cameraPanTimers; // x is horizontal, y is vertical
    private SinusoidEase cameraPanEase = new(new(0.1f, 0.6f), new(1f, CameraSpeedMultiplier));

    public ControlledClientPlayerEntity() {}

    public void Update(double delta) {
        using (PlayerKey.Push()) {
            if (ActionGroups.NextSlot.WasJustPressed() || ActionGroups.LastSlot.WasJustPressed()) {
                SetSelectedSlot(selectedHotbarSlot + (ActionGroups.NextSlot.WasJustPressed() ? 1 : 0) - (ActionGroups.LastSlot.WasJustPressed() ? 1 : 0));
                
                VoxelClient.instance!.screen!.MarkDirty();
            }

            var movement = ActionGroups.Movement.GetValue();
            var looking = -ActionGroups.Look.GetValue() * 2;

            if (movement.LengthSqr > 1)
                movement = movement.Normalized;

            var movement3d = new dvec3(movement.x, 0, movement.y);

            rotation += new dvec2((float)(looking.y * delta) * 1, (float)(looking.x * delta) * 1);

            if (rotation.x < -MathF.PI/2)
                rotation.x = -MathF.PI/2;
            if (rotation.x > MathF.PI/2)
                rotation.x = MathF.PI/2;

            movement3d = new dvec2(0, rotation.y).RotationVecToQuat() * movement3d * 4;
            var localVel = dvec2.Lerp(velocity.xz, movement3d.xz, 25 * delta);
            velocity = velocity.WithXZ(localVel);

            if (ActionGroups.Jump.GetValue())
                Jump();

            var transformUpdate = PacketPool.GetPacket<PlayerUpdatedC2SPacket>();
            transformUpdate.Position = position;
            transformUpdate.Rotation = rotation;
            VoxelClient.instance?.connection?.SendPacket(transformUpdate);


            if (ActionGroups.Attack.WasJustPressed())
                BreakBlock();

            if (ActionGroups.Use.WasJustPressed())
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
        if (selectedHotbarSlot > 3)
            return;
        ResourceKey id = selectedHotbarSlot switch {
            0 => new("stone"),
            1 => new("dirt"),
            2 => new("grass"),
            _ => new("cobblestone")
        };
        if (!ContentDatabase.Instance.Registries.Blocks.IdToRaw(id, out var raw))
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
