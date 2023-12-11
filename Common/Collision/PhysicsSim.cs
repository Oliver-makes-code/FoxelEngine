using System.Collections.Concurrent;
using GlmSharp;

namespace Voxel.Common.Collision;

/// <summary>
/// Static class that contains helper functions for physics operations.
/// </summary>
public static class PhysicsSim {

    private static readonly ConcurrentQueue<List<CollidedAABB>> ColliderCache = new();

    /// <summary>
    /// Calculates the delta between the current position of an AABB, using move and slide, against a given collision provider.
    /// </summary>
    /// <returns></returns>
    public static dvec3 MoveAndSlide(AABB boundingBox, dvec3 movement, ColliderProvider provider, int depth = 3) {

        if (depth == 0)
            return dvec3.Zero;
        
        //The total area of possible collisions we should check for is basically our hitbox
        // and every hitbox that could be between us and the point we're moving to.
        // NOTE: for non-axis-aligned `movement` variables, this area can scale massively.
        AABB totalArea = boundingBox.Encapsulate(boundingBox.Translated(movement));
        var colliders = provider.GatherColliders(totalArea);

        if (colliders.Count == 0) {
            //Console.WriteLine("No colliders...");
            return movement;
        }

        if (!ColliderCache.TryDequeue(out var sortedList))
            sortedList = new();

        var moveLength = movement.Length;

        //Sort list by closest collider.
        for (var i = 0; i < colliders.Count; i++) {
            var box = new CollidedAABB();
            box.box = colliders[i];

            //Hit is both if we've hit the raycast and if the raycast hit was less than the distance we wanted to move.
            box.didHit = box.box.Raycast(boundingBox, movement.Normalized, out var hit);
            box.hit = hit;

            if (box.hit.distance < 0)
                box.hit.distance = float.PositiveInfinity;

            sortedList.Add(box);
            
            //DebugRenderer.DrawAABB(box.box.Expanded(boundingBox));
        }
        sortedList.Sort((a, b) => a.hit.distance.CompareTo(b.hit.distance));

        //Get nearest collision from sorted list.
        var first = sortedList[0];

        //Don't need list anymore.
        sortedList.Clear();
        ColliderCache.Enqueue(sortedList);
        
        //DebugRenderer.DrawLine(first.hit.point, first.hit.point + first.hit.normal);
        
        //Console.WriteLine((boundingBox.center - first.hit.point).Length);

        //If none of them hit, then there's nothing obstructing us, so move freely.
        if (!first.didHit || first.hit.distance > moveLength)
            return movement;
        
        var moved = movement.Normalized * glm.Max(first.hit.distance - 0.01, 0);
        var left = movement - moved;
        var projected = left - first.hit.normal * dvec3.Dot(left, first.hit.normal);
        var newBox = boundingBox.Translated(moved);

        //var movementLeft = sqrDist - first.hit.distance;

        //TODO - replace! recursive code bad!
        //probably 3-iteration loop is better here.
        //reduce the size of the list as you go.
        return moved + MoveAndSlide(newBox, projected, provider, depth-1);
    }

    public struct CollidedAABB {
        public bool didHit;
        public RayCastHit hit;
        public AABB box;
    }
}
