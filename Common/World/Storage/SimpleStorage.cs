using Foxel.Common.Util;
using Foxel.Common.World.Content;
using Foxel.Common.World.Content.Blocks;
using Foxel.Common.World.Content.Blocks.State;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Storage;

/// <summary>
/// Stores blocks in accordance with their global ID.
///
/// TODO: Pack this somehow? Do we need to?
/// </summary>
public sealed class SimpleStorage : ChunkStorage {
    public static new readonly Codec<ChunkStorage> Codec = RecordCodec<ChunkStorage>.Create(
        BlockPalette.NetCodec.Field<ChunkStorage>("palette", it => ((SimpleStorage)it).Palette),
        Codecs.UShort.Array().Field<ChunkStorage>("blocks", it => ((SimpleStorage)it).PaletteItems),
        (palette, data) => new SimpleStorage(palette, data)
    );

    private static readonly Stack<ushort[]> ChunkDataCache = new();

    public readonly BlockPalette Palette;
    public readonly ushort[] PaletteItems = GetChunkData();

    public SimpleStorage() {
        Palette = new();
    }

    public SimpleStorage(BlockPalette palette, ushort[] data) {
        Palette = palette;
        data.CopyTo(PaletteItems.AsSpan());
    }

    public SimpleStorage(BlockState fill) : this() {
        Array.Fill(PaletteItems, Palette.GetOrCreateEntry(fill));
    }

    private static ushort[] GetChunkData() {
        lock (ChunkDataCache)
            if (ChunkDataCache.TryPop(out var value))
                return value;

        return new ushort[PositionExtensions.ChunkCapacity];
    }

    public override ChunkStorage GenerateCopy()
        => new SimpleStorage(Palette, PaletteItems);

    public override void Dispose() {
        lock (ChunkDataCache) {
            ChunkDataCache.Push(PaletteItems);
        }
    }

    public bool ReduceIfPossible(Chunk target, out ChunkStorage newStorage) {
        ushort startingId = PaletteItems[0];

        //If any block doesn't match starting block, cannot be reduced.
        for (var i = 1; i < PaletteItems.Length; i++) {
            if (PaletteItems[i] != startingId) {
                newStorage = this;
                return false;
            }
        }

        Dispose();
        newStorage = new SingleStorage(Palette.GetState(startingId)!.Value, target);
        return true;
    }

    public override ChunkStorage WithChunk(Chunk chunk)
        => GenerateCopy();
        
    public override Codec<ChunkStorage> GetCodec()
        => Codec;

    internal override void SetBlock(BlockState toSet, int index) {
        PaletteItems[index] = Palette.GetOrCreateEntry(toSet);
    }

    internal override BlockState GetBlock(int index)
        => Palette.GetState(PaletteItems[index])!.Value;
}
