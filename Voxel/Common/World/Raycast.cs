using System;
using GlmSharp;
using Microsoft.Xna.Framework;

namespace Voxel.Common.World; 

public static class Raycast {
    private static float Mod1(float a)
        => (a % 1 + 1) % 1;

    private static Vector3 MakePositive(Vector3 value, out Vector3 signs) {
        signs = new(MathF.Sign(value.X), MathF.Sign(value.Y), MathF.Sign(value.Z));

        return new(MathF.Abs(value.X), MathF.Abs(value.Y), MathF.Abs(value.Z));
    }

    private static float GetTMax(float start, float tDelta, float step)
        => tDelta * (step > 0 ? 1 - Mod1(start) : Mod1(start));
    
    public static HitResult? Cast(this World world, vec3 start, vec3 end, TilePos.Axis looking) {
        var startPos = new TilePos(start);
        
        if (world.GetBlock(startPos).IsSolidBlock)
            return new(startPos, looking);

        var delta = end - start;
        
        float
            // Delta
            deltaX = delta.x,
            deltaY = delta.y,
            deltaZ = delta.z,
            // Step
            stepX = MathF.Sign(deltaX),
            stepY = MathF.Sign(deltaY),
            stepZ = MathF.Sign(deltaZ),
            // tDelta
            tDeltaX = 1 / MathF.Abs(deltaX),
            tDeltaY = 1 / MathF.Abs(deltaY),
            tDeltaZ = 1 / MathF.Abs(deltaZ),
            // tMax
            tMaxX = GetTMax(start.x, tDeltaX, stepX),
            tMaxY = GetTMax(start.y, tDeltaY, stepY),
            tMaxZ = GetTMax(start.z, tDeltaZ, stepZ),
            // Positions
            x = start.x,
            y = start.y,
            z = start.z;

        var xAxis = stepX > 0 ? TilePos.Axis.PositiveX : TilePos.Axis.NegativeX;
        var yAxis = stepY > 0 ? TilePos.Axis.PositiveY : TilePos.Axis.NegativeY;
        var zAxis = stepZ > 0 ? TilePos.Axis.PositiveZ : TilePos.Axis.NegativeZ;

        var endPos = new TilePos(end);

        while (true) {
            TilePos.Axis axis;
            
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
            
            var pos = new TilePos(x, y, z);
            
            if (world.GetBlock(pos).IsSolidBlock)
                return new(pos, axis);

            if (pos == endPos)
                return null;
        }
    }

    public readonly struct HitResult {
        public readonly TilePos pos;
        public readonly TilePos.Axis axis;

        public HitResult(TilePos pos, TilePos.Axis axis) {
            this.pos = pos;
            this.axis = axis;
        }
    }

    
}
