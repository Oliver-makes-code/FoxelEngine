using System.Runtime.Serialization;
using Tomlyn.Model;
using Voxel.Client.Keybinding;
using Voxel.Common.Config;

namespace Voxel.Client;

public class ClientConfig {
    
    public Dictionary<string, string[]> keybinds = new();

    public static ClientConfig instance { get; } = ConfigHelper.LoadFile<ClientConfig>("Voxel.Client.toml") ?? new();

    public static void Load() {
        Keybinds.ReadFromConfig();
    }

    public static void Save() {
        Keybinds.WriteToConfig();

        ConfigHelper.SaveFile("Voxel.Client.toml", instance);
    }
}
