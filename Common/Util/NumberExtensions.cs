using System.Runtime.CompilerServices;

namespace Foxel.Common.Util;

public static class NumberExtensions {
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int CeilDiv(this int a, int b) {
        var (quotient, remainder) = int.DivRem(a, b);
        return remainder == 0
            ? quotient
            : quotient + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static long CeilDiv(this long a, long b) {
        var (quotient, remainder) = long.DivRem(a, b);
        return remainder == 0
            ? quotient
            : quotient + 1;
    }
}
