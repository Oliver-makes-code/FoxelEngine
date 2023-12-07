using System.Diagnostics.CodeAnalysis;

namespace Voxel.Common.Util; 

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
}
