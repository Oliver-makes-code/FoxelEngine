// Based off of this paper: http://www.cse.yorku.ca/~amana/research/grid.pdf

using GlmSharp;
using Voxel.Common.Util;
using Voxel.Common.World.Views;

namespace Voxel.Common.World;

public static class Raycast {
    public static HitResult? Cast(this BlockView world, dvec3 start, dvec3 end) { 
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

        var endPos = end.WorldToBlockPosition();

        var axis = (tMaxX < tMaxY) switch {
            true when tMaxX < tMaxZ => xAxis,
            false when tMaxY < tMaxZ => yAxis,
            _ => zAxis
        };

        while (true) {
            var blockPos = new dvec3(x, y, z).WorldToBlockPosition();

            if (world.GetBlock(blockPos).IsSolidBlock) {
                // TODO: Account for non-cubic colliders at some point
                // Gets the one integer intersection coordinate
                // axis * blockPos isolates one coordinate from blockPos
                // adding (-1, -1, -1) * the min of axis and (0, 0, 0) offsets the integer coordinate if the vector is intersecting from the other side
                var worldPos = axis * blockPos + -ivec3.Ones * ivec3.Min(axis, ivec3.Zero);
                var direction = delta.Normalized;
                
                // intersect with a yz plane
                if (axis.x != 0) {
                    double time = (start.x - worldPos.x) / direction.x;
                    worldPos.y = (int)(direction.y * time);
                    worldPos.z = (int)(direction.z * time);
                    return new(blockPos, worldPos, axis);
                }
                // intersect with an xz plane
                if (axis.y != 0) {
                    double time = (start.y - worldPos.y) / direction.y;
                    worldPos.x = (int)(direction.x * time);
                    worldPos.z = (int)(direction.z * time);
                    return new(blockPos, worldPos, axis);
                }
                // intersect with a xy plane
                if (axis.z != 0) {
                    double time = (start.z - worldPos.z) / direction.z;
                    worldPos.x = (int)(direction.x * time);
                    worldPos.y = (int)(direction.y * time);
                    return new(blockPos, worldPos, axis);
                }
            }
                
            if (blockPos == endPos)
                return null;
            
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
        }
    }
    
    private static double Mod1(double a)
        => (a % 1 + 1) % 1;

    private static double GetTMax(double start, double tDelta, double step)
        => FixNaN(tDelta * (step > 0 ? 1 - Mod1(start) : Mod1(start)));

    private static double FixNaN(double d)
        => double.IsNaN(d) ? 0 : d;

    public record struct HitResult(ivec3 BlockPos, dvec3 WorldPos, ivec3 Axis) {
        public readonly ivec3 BlockPos = BlockPos;
        public readonly dvec3 WorldPos = WorldPos;
        public readonly ivec3 Axis = Axis;
    }
}
