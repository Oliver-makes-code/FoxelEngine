using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Tomlyn;

namespace Voxel.Common.Config;

public static class ConfigHelper {
    private static readonly TomlModelOptions Options = new() {
        IgnoreMissingProperties = true,
        IncludeFields = true,
    };

    public static T? LoadFile<T>(string filePath) where T : class, new() 
        => GetFileText(filePath, out string? value) ? Toml.ToModel<T>(value, null, Options) : null;

    public static bool SaveFile<T>(string filePath, T t) where T : class
        => WriteFileText(filePath, Toml.FromModel(t, Options));

    private static bool GetFileText(string filePath, [NotNullWhen(true)] out string? text) {
        try {
            text = File.ReadAllText(filePath);
            return true;
        } catch (Exception) {
            text = null;
            return false;
        }
    }

    private static bool WriteFileText(string filePath, string text) {
        try {
            File.WriteAllText(filePath, text);
            return true;
        } catch (Exception) {
            return false;
        }
    }
}
