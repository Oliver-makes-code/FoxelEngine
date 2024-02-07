using System.Diagnostics.CodeAnalysis;
using Voxel.Core.Util;

namespace Voxel.Common.Util.Registration;

public interface Registry<T> : BaseRegistry {
    public bool RawToId(uint raw, [NotNullWhen(true)] out ResourceKey id);
    public bool RawToEntry(uint raw, [NotNullWhen(true)] out T? entry);

    public bool IdToRaw(ResourceKey id, out uint raw);
    public bool IdToEntry(ResourceKey id, [NotNullWhen(true)] out T? entry);

    public bool EntryToRaw(T entry, out uint raw);
    public bool EntryToId(T entry, out ResourceKey id);

    public T Register(T toRegister, ResourceKey id);


    IEnumerable<(T, ResourceKey, uint)> Entries();
}
