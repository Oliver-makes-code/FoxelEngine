using System.Diagnostics.CodeAnalysis;
using Voxel.Common.Util.Serialization;

namespace Voxel.Common.Util.Registration;

public class SimpleRegistry<T> : Registry<T> where T : notnull {
    private string[] rawToID;
    private T[] rawToEntry;

    private readonly Dictionary<string, uint> idToRaw = new();
    private readonly Dictionary<string, T> idToEntry = new();

    private readonly Dictionary<T, uint> entryToRaw = new();
    private readonly Dictionary<T, string> entryToId = new();

    private readonly Dictionary<string, T> registeredEntries = new();

    protected virtual void Put(T entry, string id, uint raw) {
        rawToID[raw] = id;
        rawToEntry[raw] = entry;

        idToRaw[id] = raw;
        idToEntry[id] = entry;

        entryToRaw[entry] = raw;
        entryToId[entry] = id;
    }

    public T RawToEntryDirect(uint raw) => rawToEntry[raw];

    public bool RawToID(uint raw, [NotNullWhen(true)] out string? id) {
        id = rawToID[raw];
        return true;
    }
    public bool RawToEntry(uint raw, [NotNullWhen(true)] out T? entry) {
        entry = rawToEntry[raw];
        return true;
    }

    public bool IdToRaw(string id, out uint raw) => idToRaw.TryGetValue(id, out raw);
    public bool IdToEntry(string id, [NotNullWhen(true)] out T? entry) => idToEntry.TryGetValue(id, out entry);

    public bool EntryToRaw(T entry, out uint raw) => entryToRaw.TryGetValue(entry, out raw);
    public bool EntryToID(T entry, [NotNullWhen(true)] out string? id) => entryToId.TryGetValue(entry, out id);

    public T Register(T toRegister, string id) {
        registeredEntries[id] = toRegister;
        return toRegister;
    }
    public IEnumerable<(T, string, uint)> Entries() {
        for (uint raw = 0; raw < rawToID.Length; raw++)
            yield return (rawToEntry[raw], rawToID[raw], raw);
    }

    public virtual void GenerateIDs() {
        var currentID = 0u;

        rawToID = new string[registeredEntries.Count];
        rawToEntry = new T[registeredEntries.Count];

        foreach ((string? id, var entry) in registeredEntries)
            Put(entry, id, currentID++);
    }

    public virtual void Write(VDataWriter writer) {
        writer.Write(registeredEntries.Count);

        foreach ((string id, uint raw) in idToRaw) {
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

        rawToID = new string[count];
        rawToEntry = new T[count];

        for (int i = 0; i < count; i++) {
            var id = reader.ReadString();
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
