using System.Runtime.CompilerServices;

namespace Voxel.Common.Util;

public static class MathHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float LerpF(float from, float to, float amount)
        => from + (to - from) * amount;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static double LerpD(double from, double to, double amount)
        => from + (from - to) * amount;

    public static float Clamp(float t, float min, float max) =>
        MathF.Max(MathF.Min(t, max), min);

    public static float Repeat(float t, float length) =>
        Clamp(t - MathF.Floor(t / length) * length, 0.0f, length);

    public static int Repeat(int t, int length) =>
        (int)MathF.Floor(Clamp(t - MathF.Floor((float)t / length) * length, 0.0f, length));
}
