using System;
using System.Collections.Generic;
using Voxel.Common.Tile;
using Voxel.Common.World;
using Voxel.Test.Mocks;

namespace Voxel.Test.Tests; 

public class BlockViewSuite : TestSuite {

    protected override Dictionary<string, Test> DefineTests()
        => new() {
            ["Raycast"] = () => {
                var mock = new BlockViewMock(pos => pos.y < 0 ? Blocks.Stone : Blocks.Air);

                Console.WriteLine(mock.Cast(new(0.5, 10.2, 0.8), new(5.333, -9.4, -5.1674)));
            }
        };
}
