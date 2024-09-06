using Foxel.Common.Server;
using Foxel.Common.World.Content.Blocks.State;
using Foxel.Core.Util;
using GlmSharp;

namespace Foxel.Common.World.Content.Blocks;

public class GrassNewBlock : Block {
    public readonly ContentReference<Block> DecayedBlock;

    public GrassNewBlock(ResourceKey decayedBlock, BlockSettings settings) : base(settings) {
        DecayedBlock = new(ContentStores.Blocks, decayedBlock);
    }

    public override void RandomTick(VoxelWorld world, BlockState state, ivec3 pos) {
        if (world.GetBlockState(pos + new ivec3(0, 1, 0)).Block.Settings.IgnoresCollision || world.Random.NextSingle() > 0.75)
            return;
        world.SetBlockState(pos, DecayedBlock.Get().DefaultState);
    }

    public override bool TicksRandomly()
        => true;
}
