using Foxel.Common.Util;
using Foxel.Common.Collections;
using Foxel.Common.World.Content.Blocks.State;
using Greenhouse.Libs.Serialization;
using PeterO.Numbers;

namespace Foxel.Common.World.Storage;

public class BlockPalette : IDisposable {
    public static readonly Codec<BlockPalette> NetCodec = new ProxyCodec<Entry[], BlockPalette>(
        Entry.Codec.Array(),
        FromEntryList,
        ToEntryList
    );

    private static readonly Stack<BlockState[]> EntriesCache = new();
    public readonly BlockState[] Entries = GetEntries();
    public readonly Dictionary<BlockState, ushort> EntrySet;
    public readonly BitVector FilledEntries;

    public BlockPalette() {
        EntrySet = [];
        FilledEntries = new(PositionExtensions.ChunkCapacity);
    }

    public BlockPalette(BlockPalette toClone) {
        toClone.Entries.CopyTo(Entries.AsSpan());
        EntrySet = new(toClone.EntrySet);
        FilledEntries = new(toClone.FilledEntries);
    }

    private static BlockPalette FromEntryList(Entry[] entries) {
        var palette = new BlockPalette();
        foreach (var entry in entries) {
            palette.EntrySet.Add(entry.state, entry.location);
            palette.FilledEntries.Set(entry.location);
            palette.Entries[entry.location] = entry.state;
        }
        return palette;
    }

    private static Entry[] ToEntryList(BlockPalette palette) {
        var entries = new Entry[palette.EntrySet.Keys.Count];
        int i = 0;
        foreach (var (state, location) in palette.EntrySet) {
            entries[i] = new() {
                state = state,
                location = location
            };
            i++;
        }
        return entries;
    }

    private static BlockState[] GetEntries() {
        lock (EntriesCache)
            if (EntriesCache.TryPop(out var value))
                return value;

        return new BlockState[PositionExtensions.ChunkCapacity];
    }

    public BlockState? GetState(ushort idx) {
        if (FilledEntries.Get(idx))
            return Entries[idx];
        return null;
    }

    public ushort GetOrCreateEntry(BlockState state) {
        if (EntrySet.TryGetValue(state, out ushort idx))
            return idx;
        ushort empty = (ushort)FilledEntries.UnsetIndices().First();
        EntrySet.Add(state, empty);
        FilledEntries.Set(empty);
        Entries[empty] = state;
        return empty;
    }

    public void RemoveEntryIfExists(BlockState state) {
        if (!EntrySet.TryGetValue(state, out ushort idx))
            return;
        EntrySet.Remove(state);
        FilledEntries.Unset(idx);
    }

    public void Dispose() {
        lock (EntriesCache) {
            EntriesCache.Push(Entries);
        }
    }

    private struct Entry {
        public static readonly Codec<Entry> Codec = RecordCodec<Entry>.Create(
            Codecs.UShort.Field<Entry>("location", it => it.location),
            BlockState.Codec.Field<Entry>("state", it => it.state),
            (location, state) => new() {
                location = location,
                state = state
            }
        );

        public ushort location;
        public BlockState state;
    }
}
