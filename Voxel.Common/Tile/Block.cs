namespace Voxel.Common.Tile;

public class Block {
    public readonly ushort id;
    public readonly string name;
    public readonly BlockSettings settings;

    public bool IsSolidBlock => settings.IsSolidBlock;

    public Block(ushort id, string name, BlockSettings settings) {
        this.id = id;
        this.name = name;
        this.settings = settings;

        Blocks.BlockList.Add(id, this);
    }

    public Block(ushort id, string name, BlockSettings.Builder builder) : this(id, name, builder.Build()) {}
}

public class BlockSettings {
    public static readonly BlockSettings Default = new Builder().Build();

    public readonly bool IsSolidBlock;

    private BlockSettings(bool isSolidBlock) {
        IsSolidBlock = isSolidBlock;
    }

    public class Builder {
        public bool IsSolidBlock = true;
        public Builder() {}

        public Builder(BlockSettings settings) {
            IsSolidBlock = settings.IsSolidBlock;
        }

        public BlockSettings Build() => new(IsSolidBlock);
    }
}
