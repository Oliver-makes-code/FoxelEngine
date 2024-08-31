using System;
using GlmSharp;
using Foxel.Common.Collision;
using Foxel.Common.Tile;

namespace Foxel.Common.World.Views;

public interface BlockView : IDisposable {

    public void SetBlock(ivec3 position, Block block);
    public Block GetBlock(ivec3 position);
}
