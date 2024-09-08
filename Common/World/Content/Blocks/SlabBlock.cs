using Foxel.Common.Collision;
using Foxel.Common.World.Content.Blocks.State;

namespace Foxel.Common.World.Content.Blocks;

public class SlabBlock : Block {
    public static readonly BlockShape Shape = new([new Box(new(0, 0, 0), new(1, 0.5, 1))]);

    public SlabBlock(BlockSettings settings) : base(settings) {}

    public override BlockShape GetShape(BlockState state)
        => Shape;
}
