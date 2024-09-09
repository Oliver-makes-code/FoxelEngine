using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Greenhouse.Libs.Serialization;

namespace Foxel.Core.Util;

public readonly struct ResourceKey {
    public const string DefaultGroup = "foxel";

    public const string ValidCharsPat = "^[a-zA-Z0-9\\-_/.]+$";

    public static readonly Codec<ResourceKey> Codec = new ResourceKeyCodec();
    public static readonly Regex ValidChars = new(ValidCharsPat);

    public readonly string Group;
    public readonly string Value;

    public ResourceKey(string group, string value) {
        if (!ValidChars.IsMatch(group))
            throw new ArgumentException($"Group can only contain {ValidCharsPat}. Got {group}");
        if (!ValidChars.IsMatch(value))
            throw new ArgumentException($"Value can only contain {ValidCharsPat}. Got {value}");

        Group = group;
        Value = value;
    }

    public ResourceKey(string key) {
        if (!key.Contains(':')) {
            Group = DefaultGroup;
            Value = key;
            return;
        }
        var split = key.Split(':');
        Group = split[0];
        Value = split[1];
    }

    public override int GetHashCode()
        => Group.GetHashCode() * 17 + Value.GetHashCode();

    public override bool Equals(object? obj)
        => obj is ResourceKey key
        && Group == key.Group
        && Value == key.Value;

    public ResourceKey WithGroup(string group)
        => new(group, Value);
    
    public ResourceKey WithValue(string value)
        => new(Group, value);
    
    public ResourceKey PrefixValue(string prefix)
        => new(Group, prefix+Value);

    public ResourceKey SuffixValue(string suffix)
        => new(Group, Value+suffix);

    public override string ToString()
        => $"{Group}:{Value}";

    public string ToFilePath()
        => $"{Group}/{Value}";

    public static bool operator ==(ResourceKey left, ResourceKey right)
        => left.Group == right.Group && left.Value == right.Value;

    public static bool operator !=(ResourceKey left, ResourceKey right)
        => !(left == right);
        
    private record ResourceKeyCodec : Codec<ResourceKey> {
        public override ResourceKey ReadGeneric(DataReader reader)
            => new(reader.Primitive().String());
        public override void WriteGeneric(DataWriter writer, ResourceKey value)
            => writer.Primitive().String(value.ToString());
    }
}
