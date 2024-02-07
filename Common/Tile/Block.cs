using GlmSharp;
using Voxel.Core.Util;
using Voxel.Common.World;

namespace Voxel.Common.Tile;

public class Block {
    public readonly ResourceKey Name;
    public uint id { get; internal set; }

    public readonly BlockSettings Settings;
    public bool IsAir => Settings.IsAir;
    public float Solidity => Settings.Solidity;
    public bool IsNonSolid => Settings.IsNonSolid;
    public bool TicksRandomly => Settings.TicksRandomly;

    public Block(ResourceKey name, BlockSettings settings) {
        Name = name;
        Settings = settings;
    }

    public Block(ResourceKey name, BlockSettings.Builder builder) : this(name, builder.Build()) {}

    public Block(ResourceKey name) : this(name, BlockSettings.Default) {}


    public override string ToString() => Name.ToString();

    public virtual void RandomTick(VoxelWorld world, ivec3 pos) {}
}

public class BlockSettings {
    public static readonly BlockSettings Default = new Builder().Build();

    public readonly bool IsAir;
    public float Solidity => IsAir ? 0 : 1;
    public bool IsNonSolid => Solidity < 1;
    public readonly bool TicksRandomly;

    private BlockSettings(bool isAir, bool ticksRandomly) {
        IsAir = isAir;
        TicksRandomly = ticksRandomly;
    }

    public class Builder {
        public bool isAir;
        public bool ticksRandomly;
        public Builder() {}

        public Builder(BlockSettings settings) {
            isAir = settings.IsAir;
            ticksRandomly = settings.TicksRandomly;
        }

        public Builder(Block block) : this(block.Settings) {}

        public BlockSettings Build() => new(isAir, ticksRandomly);
    }
}
