// Based off of this paper: http://www.cse.yorku.ca/~amana/research/grid.pdf

using GlmSharp;
using Voxel.Common.Collision;
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

        var endPos = end.WorldToBlockPosition();
        
        while (true) {
            var blockPos = new dvec3(x, y, z).WorldToBlockPosition();

            if (world.GetBlock(blockPos).IsSolidBlock) {
                if (new AABB(blockPos, blockPos + new dvec3(1))
                    .RayIntersects(start, end, out var pos, out var normal)) {
                    return new(blockPos, pos, normal);
                }
            }
                
            if (blockPos == endPos)
                return null;
            
            switch (tMaxX < tMaxY) {
                case true when tMaxX < tMaxZ:
                    x += stepX;
                    tMaxX += tDeltaX;
                    break;
                case false when tMaxY < tMaxZ:
                    y += stepY;
                    tMaxY += tDeltaY;
                    break;
                default:
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

    public record struct HitResult(ivec3 BlockPos, dvec3 WorldPos, ivec3 Normal) {
        public override string ToString()
            => $"HitResult(BlockPos = {BlockPos}, WorldPos = {WorldPos}, Normal = {Normal})";
    }
}
