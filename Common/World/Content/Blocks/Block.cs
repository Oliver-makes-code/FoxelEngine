using Foxel.Common.World.Content.Blocks.State;

namespace Foxel.Common.World.Content.Blocks;

public class NewBlock {
    public readonly BlockStateMap Map;

    public NewBlock() {
        var builder = new BlockStateMap.Builder();
        AddStates(builder);
        Map = builder.Build();
    }

    public virtual void AddStates(BlockStateMap.Builder builder) {}
}
