using GlmSharp;
using Foxel.Common.Content;
using Foxel.Core.Util;
using Foxel.Common.World;

namespace Foxel.Common.Tile; 

public class GrassBlock : Block {
    public GrassBlock(ResourceKey name, BlockSettings settings) : base(name, settings) {}
    public GrassBlock(ResourceKey name, BlockSettings.Builder builder) : base(name, builder) {}
    public GrassBlock(ResourceKey name) : base(name) {}

    public override void RandomTick(VoxelWorld world, ivec3 pos) {
        if (world.GetBlock(pos + new ivec3(0, 1, 0)).IsAir)
            return;
        world.SetBlock(pos, MainContentPack.Instance.Dirt);
    }
}
