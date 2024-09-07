using Foxel.Common.World.Content.Blocks.State;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content.Blocks;

public static class BlockStore {
    public static class BlockCodecs {
        public static readonly RecordCodec<Block> Basic = RecordCodec<Block>.Create(
            BlockSettings.Codec.DefaultedField<Block>("settings", it => it.Settings, () => BlockSettings.Default),
            (settings) => new(settings)
        );
        public static readonly RecordCodec<Block> Grass = RecordCodec<Block>.Create(
            PartialBlockState.Codec.Field<Block>("decays_into", it => ((GrassBlock)it).DecayedBlock),
            BlockSettings.Codec.DefaultedField<Block>("settings", it => it.Settings, () => BlockSettings.Default),
            (decayedBlock, settings) => new GrassBlock(decayedBlock, settings)
        );
    }

    public static class Blocks {
        public static readonly ContentReference<Block> Air = new(ContentStores.Blocks, new("air"));
        public static readonly ContentReference<Block> Grass = new(ContentStores.Blocks, new("grass"));
        public static readonly ContentReference<Block> Dirt = new(ContentStores.Blocks, new("dirt"));
        public static readonly ContentReference<Block> Stone = new(ContentStores.Blocks, new("stone"));
        public static readonly ContentReference<Block> Cobblestone = new(ContentStores.Blocks, new("cobblestone"));
    }

    private static void RegisterBlock(ResourceKey key, RecordCodec<Block> codec) {
        ContentStores.BlockCodecs.Register(key, codec);
    }

    internal static void RegisterStaticContent() {
        RegisterBlock(new("basic"), BlockCodecs.Basic);
        RegisterBlock(new("grass"), BlockCodecs.Grass);
    }
}
