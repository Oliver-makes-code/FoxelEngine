namespace Voxel.Common.World.Settings; 

// Registry of per-world settings
// Settings are sorted into Groups
// Groups have string-identified Values with arbitrary types

// TODO: Maybe make this an internal static class? Depends if we shove static utility methods into this
public static class WorldSettingsRegistry {
    
    private static Dictionary<string, Dictionary<string, string>> groups = new();
    
    // ensure the setting exists inside the registry, and register if it doesnt
    internal static void RegisterNewSetting<T>(WorldSetting<T> setting, string initialValue = "null") {
        groups.TryGetValue(setting.Group, out var group);
        // init the new setting to null
        if (group == null) {
            var newGroup = new Dictionary<string, string>();
            newGroup.Add(setting.Value, initialValue);
            groups.Add(setting.Group, newGroup);

            return;
        }

        group.TryGetValue(setting.Value, out var data);
        // init the new setting to null
        if (data == null) {
            group.Add(setting.Value, initialValue);
            return;
        }
        
        // if control has gotten to this point, the new setting is just a reference to a pre-existing setting
    }
    
    // parse the internal string into its correct type
    internal static T GetData<T>(WorldSetting<T> setting) {
        // by this point, the setting's data has to exist in the registry
        // so an exception here means something has gone terribly wrong :3
        string dataStr = groups[setting.Group][setting.Value];
        
        return (T)Convert.ChangeType(dataStr, typeof(T));
    }

    internal static void SetData<T>(WorldSetting<T> setting, T value) {
        // by this point, the setting's data has to exist in the registry
        // so an exception here means something has gone terribly wrong :3
        groups[setting.Group][setting.Value] = value.ToString();
    }
}

// A key into the WorldSettingsRegistry
// A WorldSetting stores no data on its own, it just points to a value in the registry.
// Note: Repeated accesses are unperformant for now, as they require repeatedly casting the data to and from a string
// TODO: Caching would make repeated accesses more performant, but make setting more expensive. That's probably worth it in this context? 
public readonly struct WorldSetting<T> {
    public WorldSetting(string group, string value) {
        Group = group;
        Value = value;
        WorldSettingsRegistry.RegisterNewSetting(this);
    }

    public WorldSetting(string group, string value, T initialValue) {
        Group = group;
        Value = value;
        WorldSettingsRegistry.RegisterNewSetting(this, initialValue!.ToString());
    }
    // name of the group and value this setting references
    public readonly string Group, Value;
    
    public string Path {
        get => $"{Group}:{Value}";
    }
    
    // fetches the data this setting points at from the WorldSettingsRegistry
    public T Data {
        get => WorldSettingsRegistry.GetData(this);
        set => WorldSettingsRegistry.SetData(this, value);
    }
}
