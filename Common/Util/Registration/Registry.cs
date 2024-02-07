using System.Diagnostics.CodeAnalysis;

namespace Voxel.Common.Util.Registration;

public interface Registry<T> : BaseRegistry {
    public bool RawToId(uint raw, [NotNullWhen(true)] out string? id);
    public bool RawToEntry(uint raw, [NotNullWhen(true)] out T? entry);

    public bool IdToRaw(string id, out uint raw);
    public bool IdToEntry(string id, [NotNullWhen(true)] out T? entry);

    public bool EntryToRaw(T entry, out uint raw);
    public bool EntryToId(T entry, [NotNullWhen(true)] out string? id);

    public T Register(T toRegister, string id);


    IEnumerable<(T, string, uint)> Entries();
}
