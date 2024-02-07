using System.Diagnostics.CodeAnalysis;
using Voxel.Common.Util.Serialization;
using Voxel.Core.Util;

namespace Voxel.Common.Util.Registration;

public class SimpleRegistry<T> : Registry<T> where T : notnull {
    private ResourceKey[] rawToId;
    private T[] rawToEntry;

    private readonly Dictionary<ResourceKey, uint> idToRaw = [];
    private readonly Dictionary<ResourceKey, T> idToEntry = [];

    private readonly Dictionary<T, uint> entryToRaw = [];
    private readonly Dictionary<T, ResourceKey> entryToId = [];

    private readonly Dictionary<ResourceKey, T> registeredEntries = [];

    protected virtual void Put(T entry, ResourceKey id, uint raw) {
        rawToId[raw] = id;
        rawToEntry[raw] = entry;

        idToRaw[id] = raw;
        idToEntry[id] = entry;

        entryToRaw[entry] = raw;
        entryToId[entry] = id;
    }

    public T RawToEntryDirect(uint raw) => rawToEntry[raw];

    public bool RawToId(uint raw, [NotNullWhen(true)] out ResourceKey id) {
        id = rawToId[raw];
        return true;
    }
    public bool RawToEntry(uint raw, [NotNullWhen(true)] out T? entry) {
        entry = rawToEntry[raw];
        return true;
    }

    public bool IdToRaw(ResourceKey id, out uint raw) => idToRaw.TryGetValue(id, out raw);
    public bool IdToEntry(ResourceKey id, [NotNullWhen(true)] out T? entry) => idToEntry.TryGetValue(id, out entry);

    public bool EntryToRaw(T entry, out uint raw) => entryToRaw.TryGetValue(entry, out raw);
    public bool EntryToId(T entry, out ResourceKey id) => entryToId.TryGetValue(entry, out id);

    public T Register(T toRegister, ResourceKey id) {
        registeredEntries[id] = toRegister;
        return toRegister;
    }

    public IEnumerable<(T, ResourceKey, uint)> Entries() {
        for (uint raw = 0; raw < rawToId.Length; raw++)
            yield return (rawToEntry[raw], rawToId[raw], raw);
    }

    public virtual void GenerateIds() {
        var currentID = 0u;

        rawToId = new ResourceKey[registeredEntries.Count];
        rawToEntry = new T[registeredEntries.Count];

        foreach ((ResourceKey id, var entry) in registeredEntries)
            Put(entry, id, currentID++);
    }

    public virtual void Write(VDataWriter writer) {
        writer.Write(registeredEntries.Count);

        foreach ((ResourceKey id, uint raw) in idToRaw) {
            writer.Write(id);
            writer.Write(raw);
        }
    }

    public virtual void Read(VDataReader reader) {
        idToRaw.Clear();
        idToEntry.Clear();
        entryToRaw.Clear();
        entryToId.Clear();

        var count = reader.ReadInt();

        rawToId = new ResourceKey[count];
        rawToEntry = new T[count];

        for (int i = 0; i < count; i++) {
            var id = reader.ReadResourceKey();
            var raw = reader.ReadUInt();

            if (!registeredEntries.TryGetValue(id, out var entry))
                continue;

            Put(entry, id, raw);
        }
    }


    public void Clear() {
        idToRaw.Clear();
        idToEntry.Clear();
        entryToRaw.Clear();
        entryToId.Clear();
        registeredEntries.Clear();
    }
}
