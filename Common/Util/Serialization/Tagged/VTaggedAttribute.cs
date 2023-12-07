namespace Common.Util.Serialization.Tagged;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class VTaggedAttribute : Attribute {
    public readonly string? Name;
    public readonly string[]? OldNames;


    public VTaggedAttribute() {
        Name = null;
        OldNames = Array.Empty<string>();
    }

    public VTaggedAttribute(string name) {
        Name = name;
        OldNames = Array.Empty<string>();
    }

    public VTaggedAttribute(string name, params string[] oldNames) {
        Name = name;
        OldNames = oldNames;
    }
}
