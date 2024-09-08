using Foxel.Common.World.Content.Blocks.State;
using Foxel.Core.Util;
using GlmSharp;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content.Blocks;

public class Block {
    public readonly BlockStateMap Map;
    public readonly BlockState DefaultState;

    public readonly BlockSettings Settings;

    public int id => ContentStores.Blocks.GetId(this);
    public ResourceKey key => ContentStores.Blocks.GetKey(this);

    public Block(BlockSettings settings) {
        Settings = settings;
        var builder = new BlockStateMap.Builder();
        AddStates(builder);
        Map = builder.Build();
        DefaultState = DefineDefaultState();
    }

    public virtual BlockState DefineDefaultState()
        => new(this);

    public virtual bool TicksRandomly()
        => false;

    public virtual void AddStates(BlockStateMap.Builder builder) {}

    public virtual void RandomTick(VoxelWorld world, BlockState state, ivec3 pos) {}

    public virtual BlockShape GetShape(BlockState state)
        => BlockShape.FullCube;
}

public readonly struct BlockSettings {
    public static readonly Codec<BlockSettings> Codec = RecordCodec<BlockSettings>.Create(
        Codecs.Bool.DefaultedField<BlockSettings>("ignores_collision", it => it.IgnoresCollision, () => false),
        (ignoresCollision) => new(ignoresCollision)
    );

    public static readonly BlockSettings Default = new(false);

    public readonly bool IgnoresCollision;
    public readonly float Solidity => IgnoresCollision ? 0 : 1;
    public readonly bool IsNonSolid => Solidity < 1;

    private BlockSettings(bool ignoresCollision) {
        IgnoresCollision = ignoresCollision;
    }
}
