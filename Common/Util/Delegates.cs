namespace Voxel.Common.Util;

public static class Delegates {
    public static TDelegate Combine<TDelegate>(params TDelegate[] delegates) where TDelegate : Delegate
        => (Delegate.Combine(delegates) as TDelegate)!;
}
