using System;

namespace Voxel.Common.Util; 

public static class Iteration {
    public delegate void CubicCallback<in T>(T x, T y, T z);
    
    public static void Cubic(int min, int max, CubicCallback<int> callback) {
        for (int x = min; x < max; x++) {
            for (int y = min; y < max; y++) {
                for (int z = min; z < max; z++) {
                    callback(x, y, z);
                }
            }
        }
    }
    
    public static void Cubic(int max, CubicCallback<int> callback)
        => Cubic(0, max, callback);
}
