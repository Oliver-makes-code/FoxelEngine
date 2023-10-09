using System.Collections;
using System.Collections.Generic;
using GlmSharp;

namespace Voxel.Common.Util;

public static class Iteration {

    public static IEnumerable<ivec3> Cubic(ivec3 min, ivec3 max) => new PositionIteratorVec3 { Min = min, Max = max };
    public static IEnumerable<ivec3> Cubic(int min, int max) => new PositionIteratorInt { Min = min, Max = max };

    public static IEnumerable<ivec3> Cubic(int max) => Cubic(0, max);

    private class PositionIteratorVec3 : IEnumerable<ivec3> {
        public ivec3 Min;
        public ivec3 Max;

        public IEnumerator<ivec3> GetEnumerator() {
            for (var x = Min.x; x < Max.x; x++)
            for (var y = Min.y; y < Max.y; y++)
            for (var z = Min.z; z < Max.z; z++)
                yield return new(x, y, z);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private class PositionIteratorInt : IEnumerable<ivec3> {

        public int Min;
        public int Max;

        public IEnumerator<ivec3> GetEnumerator() {
            for (var x = Min; x < Max; x++)
            for (var y = Min; y < Max; y++)
            for (var z = Min; z < Max; z++)
                yield return new(x, y, z);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
