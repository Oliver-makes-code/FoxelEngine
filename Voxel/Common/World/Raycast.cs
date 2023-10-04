using System;
using GlmSharp;
using Voxel.Common.Util;

namespace Voxel.Common.World;

public static class Raycast {
    private static double Mod1(double a)
        => (a % 1 + 1) % 1;

    private static double GetTMax(double start, double tDelta, double step)
        => tDelta * (step > 0 ? 1 - Mod1(start) : Mod1(start));

    public static HitResult? Cast(this VoxelWorld voxelWorld, vec3 start, vec3 end, ivec3 looking) {
        var startPos = (ivec3)start;

        if (voxelWorld.GetBlock(startPos).IsSolidBlock)
            return new(startPos, looking);

        var delta = end - start;

        double
            // Delta
            deltaX = delta.x,
            deltaY = delta.y,
            deltaZ = delta.z,
            // Step
            stepX = Math.Sign(deltaX),
            stepY = Math.Sign(deltaY),
            stepZ = Math.Sign(deltaZ),
            // tDelta
            tDeltaX = 1 / Math.Abs(deltaX),
            tDeltaY = 1 / Math.Abs(deltaY),
            tDeltaZ = 1 / Math.Abs(deltaZ),
            // tMax
            tMaxX = GetTMax(start.x, tDeltaX, stepX),
            tMaxY = GetTMax(start.y, tDeltaY, stepY),
            tMaxZ = GetTMax(start.z, tDeltaZ, stepZ),
            // Positions
            x = start.x,
            y = start.y,
            z = start.z;

        //TODO - convert to sign mul instead of branch?
        var xAxis = stepX > 0 ? ivec3.UnitX : -ivec3.UnitX;
        var yAxis = stepY > 0 ? ivec3.UnitY : -ivec3.UnitY;
        var zAxis = stepZ > 0 ? ivec3.UnitZ : -ivec3.UnitZ;

        var endPos = (ivec3)end;

        while (true) {
            ivec3 axis;

            switch (tMaxX < tMaxY) {
                case true when tMaxX < tMaxZ:
                    axis = xAxis;
                    x += stepX;
                    tMaxX += tDeltaX;
                    break;
                case false when tMaxY < tMaxZ:
                    axis = yAxis;
                    y += stepY;
                    tMaxY += tDeltaY;
                    break;
                default:
                    axis = zAxis;
                    z += stepZ;
                    tMaxZ += tDeltaZ;
                    break;
            }

            var pos = new dvec3(x, y, z).WorldToBlockPosition();

            if (voxelWorld.GetBlock(pos).IsSolidBlock)
                return new(pos, axis);

            if (pos == endPos)
                return null;
        }
    }

    public readonly struct HitResult {
        public readonly ivec3 Pos;
        public readonly ivec3 Axis;

        public HitResult(ivec3 pos, ivec3 axis) {
            Pos = pos;
            Axis = axis;
        }
    }
}
