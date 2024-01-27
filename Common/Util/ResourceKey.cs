using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Voxel.Common.Util;

public readonly struct ResourceKey {
    public const string ValidCharsPat = "^[a-zA-Z-_/.]+$";
    public static readonly Regex ValidChars = new Regex(ValidCharsPat);

    public readonly string Group;
    public readonly string Value;

    public ResourceKey(string group, string value) {
        if (!ValidChars.IsMatch(group))
            throw new ArgumentException($"Group can only contain {ValidCharsPat}");
        if (!ValidChars.IsMatch(value))
            throw new ArgumentException($"Value can only contain {ValidCharsPat}");

        Group = group;
        Value = value;
    }

    public override int GetHashCode()
        => Group.GetHashCode() * 17 + Value.GetHashCode();

    public override bool Equals(object? obj)
        => Conditions.IsNonNull(obj as ResourceKey?, out var key)
        ? Group == key.Group && Value == key.Value
        : false;

    public override string ToString()
        => $"{Group}:{Value}";
}
