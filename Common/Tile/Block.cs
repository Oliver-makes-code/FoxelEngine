namespace Voxel.Common.Tile;

public class Block {
    public readonly string Name;
    public uint id { get; internal set; }

    public readonly BlockSettings Settings;
    public bool IsAir => Settings.IsAir;
    public float Solidity => Settings.Solidity;

    public Block(string name, BlockSettings settings) {
        Name = name;
        Settings = settings;
    }

    public Block(string name, BlockSettings.Builder builder) : this(name, builder.Build()) {}

    public Block(string name) : this(name, BlockSettings.Default) {}


    public override string ToString() => Name;
}

public class BlockSettings {
    public static readonly BlockSettings Default = new Builder().Build();

    public readonly bool IsAir;
    public float Solidity => IsAir ? 0 : 1;

    private BlockSettings(bool isAir) {
        IsAir = isAir;
    }

    public class Builder {
        public bool IsAir;
        public Builder() {}

        public Builder(BlockSettings settings) {
            IsAir = settings.IsAir;
        }

        public Builder(Block block) : this(block.Settings) {}

        public BlockSettings Build() => new(IsAir);
    }
}
