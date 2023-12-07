using System.Reflection;

namespace Common.Util.Serialization.Tagged;

/// <summary>
/// Keeps track of all the variables inside of a soecific type for serialization.
/// </summary>
internal class VTaggedTypeSpec {
    public readonly Type TaggedType;

    public readonly IReadOnlyList<VTaggedProperty> Properties;

    public VTaggedTypeSpec(Type type) {
        TaggedType = type;
        bool globallyTagged = type.GetCustomAttribute<VTaggedAttribute>() != null;

        List<VTaggedProperty> properties = new List<VTaggedProperty>();

        foreach (var info in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
            var attrib = info.GetCustomAttribute<VTaggedAttribute>();

            var name = attrib?.Name ?? info.Name;
            var oldNames = attrib?.OldNames ?? Array.Empty<string>();

            if (attrib == null && !globallyTagged)
                continue;

            var property = new VTaggedProperty(info, name, oldNames);
            properties.Add(property);
        }

        Properties = properties;
    }
}
