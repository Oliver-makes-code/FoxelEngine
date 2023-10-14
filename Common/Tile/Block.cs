namespace Voxel.Common.Tile;

public class Block {
    public readonly string Name;
    public readonly BlockSettings Settings;

    public uint id;

    public bool IsSolidBlock => Settings.IsSolidBlock;

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

    public readonly bool IsSolidBlock;
    public float GetSolidityFloat => IsSolidBlock ? 1 : 0;

    private BlockSettings(bool isSolidBlock) {
        IsSolidBlock = isSolidBlock;
    }

    public class Builder {
        public bool IsSolidBlock = true;
        public Builder() {}

        public Builder(BlockSettings settings) {
            IsSolidBlock = settings.IsSolidBlock;
        }

        public Builder(Block block) : this(block.Settings) {}

        public BlockSettings Build() => new(IsSolidBlock);
    }
}
