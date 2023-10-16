using GlmSharp;
using Voxel.Common.Tile;
using Voxel.Common.World.Views;

namespace Voxel.Test.Mocks; 

public class BlockViewMock : BlockView {
    public delegate Block BlockDelegate(ivec3 position);

    private BlockDelegate Delegate;

    public BlockViewMock(BlockDelegate @delegate) {
        Delegate = @delegate;
    }

    public void SetBlock(ivec3 position, Block block) {}
    
    public Block GetBlock(ivec3 position)
        => Delegate(position);

    public void Dispose() {}
}
