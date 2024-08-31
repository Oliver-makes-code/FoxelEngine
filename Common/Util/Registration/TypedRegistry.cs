using System.Reflection;
using Foxel.Core.Util;

namespace Foxel.Common.Util.Registration;

public class TypedRegistry<T> : SimpleRegistry<TypedRegistry<T>.TypedEntry> where T : class {

    private readonly Dictionary<Type, uint> typeToRaw = [];
    private readonly Dictionary<Type, ResourceKey> typeToId = [];
    private readonly Dictionary<Type, TypedEntry> typeToEntry = [];

    private readonly Dictionary<uint, Type> rawToType = [];
    private readonly Dictionary<ResourceKey, Type> idToType = [];
    private readonly Dictionary<TypedEntry, Type> entryToType = [];

    protected override void Put(TypedEntry entry, ResourceKey id, uint raw) {
        base.Put(entry, id, raw);

        typeToRaw[entry.type] = raw;
        typeToId[entry.type] = id;
        typeToEntry[entry.type] = entry;

        rawToType[raw] = entry.type;
        idToType[id] = entry.type;
        entryToType[entry] = entry.type;
    }

    public void Register<TReg>(ResourceKey id) where TReg : T, new() {
        var typedEntry = new TypedEntry();
        typedEntry.Setup<TReg>();
        base.Register(typedEntry, id);
    }

    private void TRegisterUniqueNameIdgaf<TReg>(ResourceKey id) where TReg : T, new() => Register<TReg>(id);

    public void Register(ResourceKey id, Type t) {
        var genericmethod = GetType().GetMethod(nameof(TRegisterUniqueNameIdgaf), BindingFlags.Instance | BindingFlags.NonPublic)?.MakeGenericMethod(t);
        genericmethod?.Invoke(this, [ id ]);
    }

    public bool TypeToRaw(Type type, out uint raw) => typeToRaw.TryGetValue(type, out raw);
    public bool TypeToId(Type type, out ResourceKey id) => typeToId.TryGetValue(type, out id);
    public bool TypeToEntry(Type type, out TypedEntry entry) => typeToEntry.TryGetValue(type, out entry);

    public bool RawToType(uint raw, out Type type) => rawToType.TryGetValue(raw, out type);
    public bool IdToType(ResourceKey id, out Type type) => idToType.TryGetValue(id, out type);
    public bool EntryToType(TypedEntry entry, out Type type) => entryToType.TryGetValue(entry, out type);


    public class TypedEntry {
        public Type type { get; private set; }
        public Func<T> factory { get; private set; }

        public void Setup<TReg>() where TReg : T, new() {
            type = typeof(TReg);
            SetFactory<TReg>();
        }

        public void Setup(Type t) {
            var genericmethod = GetType().GetMethod(nameof(Setup))?.MakeGenericMethod(t);
            genericmethod?.Invoke(this, null);
        }

        public void SetFactory<TReg>() where TReg : T, new() => factory = () => new TReg();
    }
}
