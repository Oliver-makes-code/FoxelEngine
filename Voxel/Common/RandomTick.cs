using System;
using Voxel.Common.World;
using GlmSharp;
using Voxel.Common.Tile;

namespace Voxel.Common; 

public static class RandomTick {
    public static bool RollProbability(double percentageChancePerSecond, Random rng) {
        const int SampleSize = 32 * 32 * 32; // number of blocks possible to sample
        const int TickRate = 20; // number of samples per second
        int tickBatchSize = 3; // number of blocks ticked simultaneously every tick | TODO: make configurable

        double threshold = (SampleSize * percentageChancePerSecond) / (TickRate * tickBatchSize);
        return rng.NextDouble() < threshold;
    }
    
    public delegate void OnTick(VoxelWorld world, Random rng, ivec3 tilePos);

    public static OnTick ChanceToReplace(double percentageChancePerSecond, Func<Block> replaceWith)
        => (world, rng, tilePos) => {
            if (RollProbability(percentageChancePerSecond, rng)) world.SetBlock(tilePos, replaceWith());
        };
}
