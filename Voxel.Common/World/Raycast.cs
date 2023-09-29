using System;
using Microsoft.Xna.Framework;

namespace Voxel.Common.World; 

public static class Raycast {
    private static float GetTMax(float start, float tDelta, float step)
        => tDelta * (step > 0 ? 1 - start % 1 : start % 1);
    
    public static TilePos? Cast(this World world, Vector3 start, Vector3 end) {
        var startPos = new TilePos(start);
        
        if (world.GetBlock(startPos).IsSolidBlock)
            return startPos;
        
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

        var endPos = new TilePos(end);

        while (true) {
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
            
            var pos = new TilePos(x, y, z);
            
            if (world.GetBlock(pos).IsSolidBlock)
                return pos;

            if (pos == endPos)
                return null;
        }
    }
}
