using System;
using GlmSharp;
using Voxel.Common.Tile;

namespace Voxel.Common.World.Views;

public interface BlockView : IDisposable {

    public void SetBlock(ivec3 position, Block block);
    public Block GetBlock(ivec3 position);
}
