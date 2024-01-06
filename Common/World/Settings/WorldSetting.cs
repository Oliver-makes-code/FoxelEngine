namespace Voxel.Common.World.WorldSettings; 

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
