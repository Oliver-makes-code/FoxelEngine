using System.Collections.Generic;
using System.Runtime.Serialization;
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

    public static void Load() {}

    public static void Save() {
        ConfigHelper.SaveFile("Voxel.Client.toml", instance);
    }

    public class General {
        public static float deadzoneRight {
            get => instance.general._deadzoneRight;
            set => instance.general._deadzoneRight = value;
        }

        public static float deadzoneLeft {
            get => instance.general._deadzoneLeft;
            set => instance.general._deadzoneLeft = value;
        }

        public static float snapRight {
            get => instance.general._snapRight;
            set => instance.general._snapRight = value;
        }

        public static float snapLeft {
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

        public static int guiScale {
            get => instance.general._guiScale;
            set => instance.general._guiScale = value;
        }
        
        [DataMember(Name = "gui_scale")]
        public int _guiScale = 3;
        [DataMember(Name = "deadzone_right")]
        public float _deadzoneRight = 0;
        [DataMember(Name = "deadzone_left")]
        public float _deadzoneLeft = 0;
        [DataMember(Name = "snap_right")]
        public float _snapRight = 0.25f;
        [DataMember(Name = "snap_left")]
        public float _snapLeft = 0.25f;

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
