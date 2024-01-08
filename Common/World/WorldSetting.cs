using Voxel.Common.Util;

namespace Voxel.Common.World;

public class WorldSetting<TValue> : Setting<TValue> {
    public WorldSetting(string group, string value) : base("World", group, value) { }
    public WorldSetting(string group, string value, TValue defaultValue) : base("World", group, value, defaultValue) { }
}
