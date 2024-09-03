using System.Diagnostics.CodeAnalysis;

namespace Foxel.Common.Util; 

public static class Conditions {
    public static bool IsNonNull<T>(T? value, out T newValue) where T : struct {
        if (value == null) {
            newValue = default;
            return false;
        }
        newValue = value.Value;
        return true;
    }

    public static bool IsNonNull<T>(T? value, [NotNullWhen(true)] out T? newValue) where T : class {
        newValue = value;
        return value != null;
    }

    public static bool TryCast<TBase, TChild>(TBase value, [NotNullWhen(true)] out TChild? child) where TChild : class, TBase {
        if (value is TChild c) {
            child = c;
            return true;
        }
        child = null;
        return false;
    }

    public static bool TryCast<TBase, TChild>(TBase value, [NotNullWhen(true)] out TChild? child) where TChild : struct, TBase {
        if (value is TChild c) {
            child = c;
            return true;
        }
        child = null;
        return false;
    }
}
