using Foxel.Common.World.Content.Blocks.State;

namespace Foxel.Common.World.Content.Blocks;

public class NewBlock {
    public readonly BlockStateMap Map;
    public readonly BlockState DefaultState;

    public NewBlock() {
        var builder = new BlockStateMap.Builder();
        AddStates(builder);
        Map = builder.Build();
        DefaultState = DefineDefaultState();
    }

    public virtual BlockState DefineDefaultState()
        => new(this);

    public virtual void AddStates(BlockStateMap.Builder builder) {}
}
