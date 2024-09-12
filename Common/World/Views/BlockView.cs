using GlmSharp;
using Foxel.Common.World.Content.Blocks.State;

namespace Foxel.Common.World.Views;

public interface BlockView : IDisposable {

    public void SetBlockState(ivec3 position, BlockState block);
    public BlockState GetBlockState(ivec3 position);
}
