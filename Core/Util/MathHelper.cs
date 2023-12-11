using System.Runtime.CompilerServices;
using GlmSharp;

namespace Voxel.Core.Util;

public static class MathHelper {
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float LerpF(float from, float to, float amount)
        => from + (to - from) * amount;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static double LerpD(double from, double to, double amount)
        => from + (from - to) * amount;

    public static float Clamp(float t, float min, float max) =>
        MathF.Max(MathF.Min(t, max), min);
    
    public static double Clamp(double t, double min, double max) =>
        Math.Max(Math.Min(t, max), min);

    public static float Repeat(float t, float length) =>
        Clamp(t - MathF.Floor(t / length) * length, 0.0f, length);
    
    public static double Repeat(double t, double length) =>
        Clamp(t - Math.Floor(t / length) * length, 0.0, length);

    public static int Repeat(int t, int length) =>
        (int)MathF.Floor(Clamp(t - MathF.Floor((float)t / length) * length, 0.0f, length));


    public static dquat RotationVecToQuat(this dvec2 vec) => dquat.FromAxisAngle(vec.y, dvec3.UnitY) * dquat.FromAxisAngle(vec.x, dvec3.UnitX);
    public static dquat RotationVecToQuat(this vec2 vec) => dquat.FromAxisAngle(vec.y, dvec3.UnitY) * dquat.FromAxisAngle(vec.x, dvec3.UnitX);
}
