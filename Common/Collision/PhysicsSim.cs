using System.Collections.Concurrent;
using GlmSharp;
using Foxel.Common.Server;
using Foxel.Common.Util;
using Foxel.Common.World.Views;

namespace Foxel.Common.Collision;

/// <summary>
/// Static class that contains helper functions for physics operations.
/// </summary>
public static class PhysicsSim {
    public const double MaxStepHeight = 0.6;
    public const double Epsilon = 0.025;

    private static readonly ConcurrentQueue<List<CollidedBox>> ColliderCache = new();

    /// <summary>
    /// Calculates the delta between the current position of a box, using move and slide, against a given collision provider.
    /// </summary>
    /// <returns></returns>
    public static dvec3 MoveAndSlide(Box movingBox, dvec3 movement, ColliderProvider provider, bool allowStepUp, out dvec3 translateBy, int depth = 3) {
        translateBy = dvec3.Zero;
        if (depth == 0)
            return dvec3.Zero;

        //Console.WriteLine((boundingBox.center - first.hit.point).Length);

        //If none of them hit, then there's nothing obstructing us, so move freely.
        translateBy = movement;
        if (!CastBox(movingBox, movement, provider, out var collided)) {
            // Snap down to floor

            if (allowStepUp && CastBox(movingBox.Translated(movement), new(0, -MaxStepHeight, 0), provider, out collided)) {
                translateBy.y -= collided.hit.distance;
                translateBy.y += Epsilon;
            }

            return movement;
        }
        
        var hit = collided.hit;
        var box = collided.box;

        // Snap up to floor
        double minY = movingBox.min.y;
        double maxY = box.max.y;
        double height = maxY - minY;
        if (allowStepUp && height > Epsilon && height <= MaxStepHeight + Epsilon) {
            VoxelServer.Logger.Info("h");
            var stepUp = new dvec3(0, height + Epsilon, 0);
            var translated = movingBox.Translated(stepUp);
            translateBy = movement + stepUp;
            if (!CastBox(movingBox, stepUp, provider, out _) && !CastBox(translated, movement, provider, out _))
                return movement;
            VoxelServer.Logger.Info("h2");
        }

        var moved = movement.Normalized * glm.Max(hit.distance - Epsilon, 0);
        var left = movement - moved;
        var projected = left - hit.normal * dvec3.Dot(left, hit.normal);
        var newBox = movingBox.Translated(moved);

        //TODO: replace! recursive code bad!
        //probably 3-iteration loop is better here.
        //reduce the size of the list as you go.
        var next = MoveAndSlide(newBox, projected, provider, allowStepUp, out translateBy, depth - 1);
        translateBy += moved;
        return moved + next;
    }

    /// <summary>
    /// Raycasts a box through the scene.
    /// </summary>
    /// <returns></returns>
    public static bool CastBox(Box movingBox, dvec3 movementVector, ColliderProvider provider, out CollidedBox box) {
        //The total area of possible collisions we should check for is basically our hitbox
        // and every hitbox that could be between us and the point we're moving to.
        // NOTE: for non-axis-aligned raycast directions, this area can scale massively.
        Box totalArea = movingBox.Encapsulate(movingBox.Translated(movementVector));
        var colliders = provider.GatherColliders(totalArea);

        //No colliders found
        if (colliders.Count == 0) {
            box = default;
            return false;
        }

        if (!ColliderCache.TryDequeue(out var sortedList))
            sortedList = [];

        double moveLength = movementVector.Length;

        //Sort list by closest collider.
        for (var i = 0; i < colliders.Count; i++) {
            var colided = new CollidedBox {
                box = colliders[i]
            };

            //Hit is both if we've hit the raycast and if the raycast hit was less than the distance we wanted to move.
            colided.didHit = colided.box.Raycast(movingBox, movementVector.Normalized, out var boxHit);
            colided.hit = boxHit;

            if (colided.hit.distance < 0 || colided.hit.startedInside) {
                colided.hit.distance = float.PositiveInfinity;
                colided.didHit = false;
            }

            sortedList.Add(colided);
        }
        sortedList.Sort((a, b) => a.hit.distance.CompareTo(b.hit.distance));

        box = sortedList.Where(it => it.didHit).FirstOrDefault(new CollidedBox {
            didHit = false
        });
        var hit = box.hit;

        //Cleanup
        sortedList.Clear();
        ColliderCache.Enqueue(sortedList);

        return box.didHit && box.hit.distance < moveLength;
    }

    /// This needs some touching up. I just quickly ported it to get something that works.
    public static bool Raycast(this BlockView world, RaySegment segment, out BlockRaycastHit blockHit) {
        blockHit = default;
        var rayOrigin = segment.Position;
        var delta = segment.Delta;
        var rayDest = rayOrigin + delta;

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
            tMaxX = GetTMax(rayOrigin.x, tDeltaX, stepX),
            tMaxY = GetTMax(rayOrigin.y, tDeltaY, stepY),
            tMaxZ = GetTMax(rayOrigin.z, tDeltaZ, stepZ),
            // Positions
            x = rayOrigin.x,
            y = rayOrigin.y,
            z = rayOrigin.z;

        var endPos = rayDest.WorldToBlockPosition();
        
        while (true) {
            var blockPos = new dvec3(x, y, z).WorldToBlockPosition();
            var state = world.GetBlockState(blockPos);

            if (!state.Settings.IgnoresCollision) {
                var shape = state.Block.GetShape(state);
                BlockRaycastHit? min = null;
                foreach (var box in shape.LocalBoxes(blockPos)) {
                    if (box.Expanded(0.01).Raycast(segment, out var hit)) {
                        var _hit = new BlockRaycastHit(hit) {
                            blockPos = blockPos
                        };

                        min ??= _hit;
                        min = min.Value.distance < _hit.distance ? min : _hit;
                    }
                }
                if (Conditions.IsNonNull(min, out blockHit))
                    return true;
            }

            if (blockPos == endPos)
                return false;
            
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

    public struct CollidedBox {
        public bool didHit;
        public RaycastHit hit;
        public Box box;
    }
}
