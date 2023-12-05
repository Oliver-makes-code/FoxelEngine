namespace Voxel.Common.Util; 

public static class ConditionHelpers {
    public static bool IsNonNull<T>(T? value, out T newValue) where T : struct {
        if (value == null) {
            newValue = default;
            return false;
        }
        newValue = value.Value;
        return true;
    }
}
