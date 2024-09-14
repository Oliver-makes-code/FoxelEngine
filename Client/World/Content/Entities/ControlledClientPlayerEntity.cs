using System;
using GlmSharp;
using Foxel.Common.Network.Packets.C2S.Gameplay;
using Foxel.Common.Network.Packets.C2S.Gameplay.Actions;
using Foxel.Common.Network.Packets.Utils;
using Foxel.Common.Util;
using Foxel.Core.Util.Profiling;
using Foxel.Core.Util;
using Foxel.Client.Input;
using Foxel.Common.World.Content.Blocks;

namespace Foxel.Client.World.Content.Entities;

public class ControlledClientPlayerEntity : ClientPlayerEntity {
    private static readonly Profiler.ProfilerKey PlayerKey = Profiler.GetProfilerKey("Update Player Input");

    public ControlledClientPlayerEntity() {}

    public void Update(double delta) {
        using (PlayerKey.Push()) {
            if (ActionGroups.NoClip.WasJustPressed())
                applyGravity = !applyGravity;

            if (ActionGroups.NextSlot.WasJustPressed() || ActionGroups.LastSlot.WasJustPressed()) {
                SetSelectedSlot(selectedHotbarSlot + (ActionGroups.NextSlot.WasJustPressed() ? 1 : 0) - (ActionGroups.LastSlot.WasJustPressed() ? 1 : 0));
                
                VoxelClient.instance!.screen!.MarkDirty();
            }

            var looking = -ActionGroups.Look.GetValue() / 32;
            if (!VoxelClient.isMouseCapruted)
                looking = vec2.Zero;

            rotation += new dvec2(looking.y, looking.x);

            if (rotation.x < -MathF.PI/2)
                rotation.x = -MathF.PI/2;
            if (rotation.x > MathF.PI/2)
                rotation.x = MathF.PI/2;

            var movement = ActionGroups.Movement.GetValue();

            if (movement.LengthSqr > 1)
                movement = movement.Normalized;

            var movement3d = new dvec3(movement.x, 0, movement.y);

            movement3d = new dvec2(0, rotation.y).RotationVecToQuat() * movement3d * 4;
            var localVel = dvec2.Lerp(velocity.xz, movement3d.xz, 25 * delta);
            velocity = velocity.WithXZ(localVel);

            if (applyGravity) {
                if (ActionGroups.Jump.GetValue())
                    Jump();
            } else {
                float vel = 0;
                if (ActionGroups.Jump.GetValue())
                    vel += 1;
                if (ActionGroups.Crouch.GetValue())
                    vel -= 1;
                velocity = velocity.WithY(vel * 8);
            }

            var transformUpdate = PacketPool.GetPacket<PlayerUpdatedC2SPacket>();
            transformUpdate.position = position;
            transformUpdate.rotation = rotation;
            VoxelClient.instance?.connection?.SendPacket(transformUpdate);


            if (ActionGroups.Attack.WasJustPressed())
                BreakBlock();

            if (ActionGroups.Use.WasJustPressed())
                Use();
        }
    }

    private void BreakBlock() {
        var pkt = PacketPool.GetPacket<BreakBlockC2SPacket>();
        pkt.Init(this);
        pkt.state = BlockStore.Blocks.Air.Get().DefaultState;

        VoxelClient.instance?.connection?.SendPacket(pkt);
    }

    private void Use() {
        var pkt = PacketPool.GetPacket<PlayerUseActionC2SPacket>();
        pkt.Init(this);

        pkt.slot = selectedHotbarSlot;

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
