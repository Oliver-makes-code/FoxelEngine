using System;
using System.IO;
using Tomlyn;

namespace Voxel.Common.Config;

public static class ConfigHelper {
    private readonly static TomlModelOptions options = new() {
        IgnoreMissingProperties = true,
        IncludeFields = true,
    };

    private static string? GetFileText(string filePath) {
        try {
            return File.ReadAllText(filePath);
        } catch (Exception) {
            return null;
        }
    }

    private static void WriteFileText(string filePath, string text) {
        try {
            File.WriteAllText(filePath, text);
        } catch (Exception) {}
    }

    public static T? LoadFile<T>(string filePath) where T : class, new() {
        var value = GetFileText(filePath);
        if (value == null)
            return null;
        
        return Toml.ToModel<T>(value, null, options);
    }

    public static void SaveFile<T>(string filePath, T t) where T : class {
        WriteFileText(filePath, Toml.FromModel(t, options));
    }
}
