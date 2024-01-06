using Voxel.Common.Collections;

namespace Voxel.Common.World.WorldSettings; 

// Registry of per-world settings
// Settings are sorted into Groups
// Groups have string-identified Values with arbitrary types

// TODO: Maybe make this an internal static class? Depends if we shove static utility methods into this
public static class WorldSettingsRegistry {
    
    private static BiDictionary<string, string> Settings = new();
    
    // ensure the setting exists inside the registry, and register if it doesnt
    internal static void RegisterNewSetting<T>(WorldSetting<T> setting, string initialValue = "null") {
        if (Settings.ContainsKey(setting.Group, setting.Value))
            return;
        Settings[setting.Group, setting.Value] = initialValue;
    }
    
    // parse the internal string into its correct type
    internal static T GetData<T>(WorldSetting<T> setting) {
        // by this point, the setting's data has to exist in the registry
        // so an exception here means something has gone terribly wrong :3
        string dataStr = Settings[setting.Group, setting.Value];
        
        return (T)Convert.ChangeType(dataStr, typeof(T));
    }

    internal static void SetData<T>(WorldSetting<T> setting, T value) {
        // by this point, the setting's data has to exist in the registry
        // so an exception here means something has gone terribly wrong :3
        Settings[setting.Group, setting.Value] = value?.ToString() ?? "null";
    }
}
