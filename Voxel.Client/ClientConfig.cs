using System.Collections.Generic;
using System.Runtime.Serialization;
using Voxel.Client.Keybinding;
using Voxel.Common.Config;

namespace Voxel.Client;

public class ClientConfig {
    public static ClientConfig instance { get; } = ConfigHelper.LoadFile<ClientConfig>("Voxel.Client.toml") ?? new();
    
    public General general = new();
    
    public Dictionary<string, string[]> keybindings = new();

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

        public float fov = 45;

        public int renderDistance = 4;

        public static float DeadzoneRight {
            get => instance.general.deadzoneRight;
            set => instance.general.deadzoneRight = value;
        }

        public static float DeadzoneLeft {
            get => instance.general.deadzoneLeft;
            set => instance.general.deadzoneLeft = value;
        }

        public static float Fov {
            get => instance.general.fov;
            set => instance.general.fov = value;
        }

        public static int RenderDistance {
            get => instance.general.renderDistance;
            set => instance.general.renderDistance = value;
        }
    }
}
