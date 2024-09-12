using Foxel.Common.World.Content.Blocks.State;
using GlmSharp;

namespace Foxel.Common.World.Content.Blocks;

public class GrassBlock : Block {
    public readonly PartialBlockState DecayedBlock;

    public GrassBlock(PartialBlockState decayedBlock, BlockSettings settings) : base(settings) {
        DecayedBlock = decayedBlock;
    }

    public override void RandomTick(VoxelWorld world, BlockState state, ivec3 pos) {
        if (world.GetBlockState(pos + new ivec3(0, 1, 0)).Settings.IgnoresCollision || world.Random.NextSingle() < 0.75)
            return;
        world.SetBlockState(pos, DecayedBlock.Get());
    }

    public override bool TicksRandomly()
        => true;
}
