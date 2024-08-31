using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GlmSharp;
using Foxel.Common.Util;
using Foxel.Common.World;

namespace Foxel.Client.Rendering.Occlusion;

public static class OcclusionGraph {
    /// Builds the occlusion graph, returning the connected faces.
    public static ChunkOcclusionBitSet Build([NotNull] Chunk chunk) {
        var occlusion = ChunkOcclusionBitSet.None;
        var visited = new HashSet<ivec3>();

        foreach (var pos in Iteration.Cubic(PositionExtensions.ChunkSize)) {
            // Ignore already visited positions, solid blocks,
            // or blocks not on the outside of the chunk
            if (
                visited.Contains(pos) ||
                chunk.GetBlock(pos).IsNonSolid ||
                (pos > 0 & pos < PositionExtensions.ChunkSize - 1).All
            )
                continue;
            // Get the connected blocks to the current position
            var connectedBlocks = chunk.FloodFill(pos);
            // Update the occlusion graph
            occlusion |= Check(connectedBlocks);
            // Add the connected blocks to the visited positions
            visited.UnionWith(connectedBlocks);
        }
        return occlusion;
    }

    /// Get the connected faces of a single flood fill pass
    private static ChunkOcclusionBitSet Check(HashSet<ivec3> nodes) {
        // Check if any faces connect
        bool isDown = nodes.Any(it => it.y == 0);
        bool isUp = nodes.Any(it => it.y == PositionExtensions.ChunkSize);
        bool isNorth = nodes.Any(it => it.z == 0);
        bool isSouth = nodes.Any(it => it.z == PositionExtensions.ChunkSize);
        bool isWest = nodes.Any(it => it.x == 0);
        bool isEast = nodes.Any(it => it.x == PositionExtensions.ChunkSize);

        var up = isUp ? ChunkOcclusionBitSet.Up : 0;
        var down = isDown ? ChunkOcclusionBitSet.Down : 0;
        var north = isNorth ? ChunkOcclusionBitSet.North : 0;
        var south = isSouth ? ChunkOcclusionBitSet.South : 0;
        var east = isEast ? ChunkOcclusionBitSet.East : 0;
        var west = isWest ? ChunkOcclusionBitSet.West : 0;

        // There might be a better way to do this.
        return (
            // Each value contains all possible connections,
            // We want to whittle it down, only keeping the connections that
            // are common with the others.
            (up & (down | north | south | east | west)) |
            (down & (north | south | east | west)) |
            (north & (south | east | west)) |
            (south & (east | west)) |
            (east & west)
        );
    }
}
