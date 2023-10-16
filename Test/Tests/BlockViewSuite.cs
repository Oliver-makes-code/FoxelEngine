using System;
using System.Collections.Generic;
using GlmSharp;
using Voxel.Common.Tile;
using Voxel.Common.Util;
using Voxel.Common.World;
using Voxel.Test.Mocks;

namespace Voxel.Test.Tests; 

public class BlockViewSuite : TestSuite {

    protected override Dictionary<string, Test> DefineTests()
        => new() {
            ["Raycast"] = () => {
                // Mock block view with all tiles below 0 solid
                var mock = new BlockViewMock(pos => pos.y < 0 ? Blocks.Stone : Blocks.Air);

                var rng = new Random(0);
                double RandomCoord()
                    => rng.NextDouble() * 200d - 100d;
                
                // Test downward casts
                for (int i = 0; i < 100; i++) {
                    // world pos on the top surface of the tested block
                    var randomPos = new dvec3(RandomCoord(), 0, RandomCoord());
                    
                    var hit = mock.Cast(randomPos + dvec3.UnitY, randomPos - dvec3.UnitY);
                    Assert(hit?.BlockPos == randomPos.WorldToBlockPosition() - ivec3.UnitY, "Hit correct block");
                    Assert((hit?.WorldPos - randomPos)?.Length < 0.0001f, $"Hit correct world position");
                }
            }
        };
}
