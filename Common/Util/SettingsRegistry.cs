using System.ComponentModel.Design.Serialization;
using System.Runtime.CompilerServices;
using Foxel.Common.Collections;

namespace Foxel.Common.Util;

// Registry of settings
// Settings are sorted into Groups
// Groups have string-identified Values with arbitrary types
public class SettingsRegistry {
    
    private static Dictionary<string, SettingsRegistry> registries;

    SettingsRegistry(string name) {
        Name = name;
    }
    private readonly string Name;
    private readonly BiDictionary<string, string> Settings = new();
    
    // Get a registry by name, and instantiate it if it doesnt exist.
    // Used to make separate settings registries
    internal static SettingsRegistry GetRegistry(string registryName) {
        if (!registries.TryGetValue(registryName, out var registry)) {
            registry = new SettingsRegistry(registryName);
            registries.Add(registryName, registry);
        }

        return registry;
    }
    
    // ensure the setting exists inside the registry, and register if it doesnt
    internal void RegisterNewSetting<TValue>(Setting<TValue> setting, string initialValue = "null") {
        if (Settings.ContainsKey(setting.Group, setting.Value))
            return;
        Settings[setting.Group, setting.Value] = initialValue;
    }
    
    // parse the internal string into its correct type
    internal TValue GetData<TValue>(Setting<TValue> setting) {
        // by this point, the setting's data has to exist in the registry
        // so an exception here means something has gone terribly wrong :3
        string dataStr = Settings[setting.Group, setting.Value];
        
        return (TValue)Convert.ChangeType(dataStr, typeof(TValue));
    }
    
    internal void SetData<TValue>(Setting<TValue> setting, TValue value) {
        // by this point, the setting's data has to exist in the registry
        // so an exception here means something has gone terribly wrong :3
        Settings[setting.Group, setting.Value] = value?.ToString() ?? "null";
    }
}

// A key into a SettingsRegistry
// A Setting stores no data on its own, it just points to a value in the registry.
// Note: Repeated accesses are unperformant for now, as they require repeatedly casting the data to and from a string
// TODO: Caching would make repeated accesses more performant, but make setting more expensive. That's probably worth it in this context? 
public class Setting<TValue> {
    public Setting(string targetRegistry, string group, string value) {
        Group = group;
        Value = value;
        Registry = SettingsRegistry.GetRegistry(targetRegistry);
        
        Registry.RegisterNewSetting(this);
    }
    public Setting(string targetRegistry, string group, string value, TValue initialValue) {
        Group = group;
        Value = value;
        Registry = SettingsRegistry.GetRegistry(targetRegistry);
        
        Registry.RegisterNewSetting(this);
    }
    // name of the group and value this setting references
    public readonly string Group, Value;
    
    public string Path {
        get => $"{Group}:{Value}";
    }
    
    // fetches the data this setting points at from the WorldSettingsRegistry
    public TValue Data {
        get => Registry.GetData(this);
        set => Registry.SetData(this, value);
    }

    private readonly SettingsRegistry Registry;
}
