using System;
using System.Collections.Generic;
using GlmSharp;
using Voxel.Common.Collision;

namespace Voxel.Test.Tests; 

public class AABBSuite : TestSuite {

    protected override Dictionary<string, Test> DefineTests()
        => new() {
            ["K.K. Slider"] = () => {
                var boxA = new AABB(
                    new(0),
                    new(1)
                );
                var boxB = new AABB(
                    new(2, 0, 0),
                    new(3, 1, 1)
                );

                Assert(boxA.SlideWith(boxB, new(2, 0, 0), out _) == 0.5, "Sliding 1x1x1 cubes on one axis");
            }
        };
}
