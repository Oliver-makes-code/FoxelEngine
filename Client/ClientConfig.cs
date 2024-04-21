using System.Collections.Generic;
using System.Runtime.Serialization;
using Voxel.Client.Keybinding;
using Voxel.Common.Config;

namespace Voxel.Client;

public class ClientConfig {
    public static ClientConfig instance { get; } = ConfigHelper.LoadFile<ClientConfig>("Voxel.Client.toml") ?? new();

    public static Dictionary<string, string[]> keybindings {
        get => instance._keybindings;
        set => instance._keybindings = value;
    }

    public General general = new();

    [DataMember(Name = "keybindings")]
    public Dictionary<string, string[]> _keybindings = new();

    public static void Load() {
        Keybinds.ReadFromConfig();
    }

    public static void Save() {
        Keybinds.WriteToConfig();

        ConfigHelper.SaveFile("Voxel.Client.toml", instance);
    }

    public class General {
        public static double deadzoneRight {
            get => instance.general._deadzoneRight;
            set => instance.general._deadzoneRight = value;
        }

        public static double deadzoneLeft {
            get => instance.general._deadzoneLeft;
            set => instance.general._deadzoneLeft = value;
        }

        public static double snapRight {
            get => instance.general._snapRight;
            set => instance.general._snapRight = value;
        }

        public static double snapLeft {
            get => instance.general._snapLeft;
            set => instance.general._snapLeft = value;
        }

        public static float fov {
            get => instance.general._fov;
            set => instance.general._fov = value;
        }

        public static int renderDistance {
            get => instance.general._renderDistance;
            set => instance.general._renderDistance = value;
        }

        public static int msaaLevel {
            get => instance.general._msaa;
            set => instance.general._msaa = value;
        }

        public static int chunkBuildThreadCount {
            get => instance.general._chunkBuildThreadCount;
            set => instance.general._chunkBuildThreadCount = value;
        }
        
        [DataMember(Name = "guo_scale")]
        public uint _guiScale = 1;
        [DataMember(Name = "deadzone_right")]
        public double _deadzoneRight = 0;
        [DataMember(Name = "deadzone_left")]
        public double _deadzoneLeft = 0;
        [DataMember(Name = "snap_right")]
        public double _snapRight = 0.25;
        [DataMember(Name = "snap_left")]
        public double _snapLeft = 0.25;

        [DataMember(Name = "fov")]
        public float _fov = 45;

        [DataMember(Name = "render_distance")]
        public int _renderDistance = 4;
        [DataMember(Name = "MSAA")]
        public int _msaa = 1;

        [DataMember(Name = "chunk_build_thread_count")]
        public int _chunkBuildThreadCount = 4;
    }
}
