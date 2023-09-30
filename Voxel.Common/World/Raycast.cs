using System;
using Microsoft.Xna.Framework;

namespace Voxel.Common.World; 

public static class Raycast {
    private static float GetTMax(float start, float tDelta, float step)
        => tDelta * (step > 0 ? 1 - start % 1 * MathF.Sign(start) : start % 1 * MathF.Sign(start));
    
    public static HitResult? Cast(this World world, Vector3 start, Vector3 end, TilePos.Axis looking) {
        var startPos = new TilePos(start);
        
        if (world.GetBlock(startPos).IsSolidBlock)
            return new(startPos, looking);
        
        float
            // Delta
            deltaX = end.X - start.X,
            deltaY = end.Y - start.Y,
            deltaZ = end.Z - start.Z,
            // Step
            stepX = MathF.Sign(deltaX),
            stepY = MathF.Sign(deltaY),
            stepZ = MathF.Sign(deltaZ),
            // tDelta
            tDeltaX = 1 / MathF.Abs(deltaX),
            tDeltaY = 1 / MathF.Abs(deltaY),
            tDeltaZ = 1 / MathF.Abs(deltaZ),
            // tMax
            tMaxX = GetTMax(start.X, tDeltaX, stepX),
            tMaxY = GetTMax(start.Y, tDeltaY, stepY),
            tMaxZ = GetTMax(start.Z, tDeltaZ, stepZ),
            // Positions
            x = start.X,
            y = start.Y,
            z = start.Z;

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
