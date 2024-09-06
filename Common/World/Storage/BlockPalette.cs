using Foxel.Common.Util;
using Foxel.Common.Util.Collections;
using Foxel.Common.World.Content.Blocks.State;

namespace Foxel.Common.World.Storage;

public class BlockPalette {
    public readonly BlockState[] Entries = new BlockState[PositionExtensions.ChunkCapacity];
    public readonly Dictionary<BlockState, ushort> EntrySet = [];
    public readonly BitVector FilledEntries = new(PositionExtensions.ChunkCapacity);

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
}
