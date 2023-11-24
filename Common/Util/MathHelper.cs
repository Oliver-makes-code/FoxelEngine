using System.Runtime.CompilerServices;

namespace Voxel.Common.Util; 

public static class MathHelper {
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float LerpF(float from, float to, float amount)
        => from + (from - to) * amount;
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static double LerpD(double from, double to, double amount)
        => from + (from - to) * amount;
}
