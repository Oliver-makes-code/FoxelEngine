using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Voxel.Core.Util;

public readonly partial struct ResourceKey {
    public const string DefaultGroup = "voxel";

    public const string ValidCharsPat = "^[a-zA-Z0-9\\-_/.]+$";

    public readonly string Group;
    public readonly string Value;

    private ResourceKey(string group, string value) {
        if (!ValidChars().IsMatch(group))
            throw new ArgumentException($"Group can only contain {ValidCharsPat}. Got {group}");
        if (!ValidChars().IsMatch(value))
            throw new ArgumentException($"Value can only contain {ValidCharsPat}. Got {value}");

        Group = group;
        Value = value;
    }

    public static ResourceKey Of(string first, string? second = null) {
        if (second != null)
            return new(first, second);
        if (!first.Contains(':'))
            return new(DefaultGroup, first);
        var split = first.Split(':');
        return new(split[0], split[1]);
    }

    public override int GetHashCode()
        => Group.GetHashCode() * 17 + Value.GetHashCode();

    public override bool Equals(object? obj)
        => obj is ResourceKey key
        && Group == key.Group
        && Value == key.Value;

    public override string ToString()
        => $"{Group}:{Value}";
    
    [GeneratedRegex(ValidCharsPat)]
    private static partial Regex ValidChars();
}
