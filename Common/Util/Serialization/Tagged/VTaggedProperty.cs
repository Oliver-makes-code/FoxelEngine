using System.Reflection;

namespace Common.Util.Serialization.Tagged;

/// <summary>
/// Tracks an individual property set by a VTaggedTypeSpec.
/// </summary>
internal class VTaggedProperty {
    public readonly PropertyInfo PropertyInfo;
    public readonly string Name;
    public readonly string[] OldNames;

    private readonly Action<object, object> setter;
    private readonly Func<object, object> getter;

    public VTaggedProperty(PropertyInfo info, string name, string[] oldNames) {
        PropertyInfo = info;
        name = name;
        OldNames = oldNames;
    }
}
