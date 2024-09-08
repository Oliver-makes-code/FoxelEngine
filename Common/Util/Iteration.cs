using GlmSharp;

namespace Foxel.Common.Util;

public static class Iteration {
    public static IEnumerable<ivec3> Cubic(ivec3 min, ivec3 max) {
        for (int x = min.x; x < max.x; x++) 
            for (int y = min.y; y < max.y; y++)
                for (int z = min.z; z < max.z; z++)
                        yield return new(x, y, z);
    }

    public static IEnumerable<ivec3> Cubic(int min, int max)
        => Cubic(new ivec3(min, min, min), new ivec3(max, max, max));

    public static IEnumerable<ivec3> Cubic(int max)
        => Cubic(0, max);
    
    public static IEnumerable<ivec2> Square(ivec2 min, ivec2 max) {
        for (int x = min.x; x < max.x; x++) 
            for (int y = min.y; y < max.y; y++)
                yield return new(x, y);
    }

    public static IEnumerable<ivec2> Square(int min, int max)
        => Square(new ivec2(min, min), new ivec2(max, max));

    public static IEnumerable<ivec2> Square(int max)
        => Square(0, max);
}
