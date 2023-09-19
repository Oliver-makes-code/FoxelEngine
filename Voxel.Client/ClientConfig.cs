using System.Runtime.Serialization;
using Voxel.Client.Keybinding;
using Voxel.Common.Config;

namespace Voxel.Client;

public class ClientConfig {
    public General general = new();
    
    public Dictionary<string, string[]> keybindings = new();

    public static ClientConfig instance { get; } = ConfigHelper.LoadFile<ClientConfig>("Voxel.Client.toml") ?? new();

    public static Dictionary<string, string[]> Keybindings {
        get => instance.keybindings;
        set => instance.keybindings = value;
    }

    public static void Load() {
        Keybinds.ReadFromConfig();
    }

    public static void Save() {
        Keybinds.WriteToConfig();

        ConfigHelper.SaveFile("Voxel.Client.toml", instance);
    }

    public class General {
        public float deadzoneRight = 0;
        public float deadzoneLeft = 0;

        public static float DeadzoneRight {
            get => instance.general.deadzoneRight;
            set => instance.general.deadzoneRight = value;
        }

        public static float DeadzoneLeft {
            get => instance.general.deadzoneLeft;
            set => instance.general.deadzoneLeft = value;
        }
    }
}
